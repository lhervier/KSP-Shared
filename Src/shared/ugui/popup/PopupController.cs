using UnityEngine;
using System;
using System.Collections;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popup
{
    /// <summary>
    /// Persistent handle for a uGUI popup. Placed on one of the mod's GameObjects (not on the window
    /// itself, so it survives KSP destroying the window), it drives lazy spawning, show/hide, state
    /// persistence (position + open state), and notifies its transitions through OnOpenChanged. The
    /// low-level assembly of a window (PopupDialog, chrome, title bar, content, overlays) is delegated to
    /// the PopupBuilder, invoked on demand through the injected spawn function.
    /// </summary>
    public class PopupController : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("PopupController");

        public EventData<Vector2> OnPositionCaptured = new EventData<Vector2>("PopupController.OnPositionCaptured");

        /// <summary>Fired on every open/close transition; query <see cref="IsOpen"/> for the direction.</summary>
        public EventVoid OnOpenChanged = new EventVoid("PopupController.OnOpenChanged");

        /// <summary>True while a window is shown to the user.</summary>
        public bool IsOpen { get; private set; }

        // Injected by the builder at Build time, before Start() runs.
        private PopupSettings _settings;
        public PopupController WithSettings(PopupSettings settings)
        {
            this._settings = settings;
            return this;
        }

        private Func<PopupSpawnResult> _spawn;
        public PopupController WithSpawnResultProducer(Func<PopupSpawnResult> spawn)
        {
            _spawn = spawn;
            return this;
        }

        // Per-spawn state: valid while a window instance is alive (fake-null once KSP destroys it).
        private PopupDialog _popupDialog;
        private CanvasGroup _canvasGroup;
        private ButtonController _closeButtonController;

        // =========================
        // Life cycle
        // =========================

        /// <summary>
        /// Unity callback. Sets up the controller-lifetime hooks and restores the persisted open state;
        /// its counterpart is <see cref="OnDestroy"/>.
        /// </summary>
        public void Start()
        {
            // Bound to the controller, not to any window instance: they must survive spawn/despawn cycles.
            GameEvents.onLevelWasLoaded.Add(OnSceneLoaded);
            GameEvents.onGameSceneLoadRequested.Add(OnSceneUnloading);

            // Reopen the window if it was left open. Safe to fire OnOpenChanged here: Unity runs this
            // Start() only after the owner's own Start() (which created us via AddComponent) has returned,
            // so the owner has already subscribed to OnOpenChanged.
            RestoreState();
        }

        /// <summary>
        /// Unity callback. Tears down what <see cref="Start"/> set up, and dismisses a still-open window.
        /// </summary>
        public void OnDestroy()
        {
            GameEvents.onLevelWasLoaded.Remove(OnSceneLoaded);
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneUnloading);

            // The controller lives on the mod GO and outlives the window it drives. When the mod GO is
            // torn down (scene exit), a window spawned with persistAcrossScenes would otherwise survive
            // WITHOUT a controller — and the next scene would spawn a second one. Dismiss it here. This is
            // teardown, not a user close: do NOT touch the persisted open state (we want to reopen next scene).
            if (_popupDialog != null)
            {
                _popupDialog.onDestroy.RemoveListener(OnPopupDestroyed);
                _popupDialog.Dismiss();
            }
            DetachWindow();
        }

        // =====================
        // Public API
        // =====================

        /// <summary>
        /// Open the window: spawn it on the first call (or after KSP closed it), then reveal it at its
        /// last saved position.
        /// </summary>
        public void Show()
        {
            // == null is Unity-destruction aware: after KSP destroyed the window (Escape), _popupDialog is
            // fake-null here, which triggers a fresh spawn.
            if (_popupDialog == null)
            {
                PopupSpawnResult result = _spawn?.Invoke();
                if (result == null || result.PopupDialog == null)
                {
                    LOGGER.LogError("Unable to spawn the popup window");
                    return;
                }
                AttachWindow(result);
            }

            _popupDialog.gameObject.SetActive(true);
            // Restore the dragged position one frame later: KSP re-applies the spawn position on the
            // layout pass that follows activation, so applying it now would be overwritten.
            StartCoroutine(_ApplyUguiPositionAfterLayout());
            SetOpen(true);
        }

        /// <summary>
        /// Hide the window, saving its position. The window stays alive so it can be shown again.
        /// </summary>
        public void Hide()
        {
            if (_popupDialog != null)
            {
                CaptureWindowPosition();
                _popupDialog.gameObject.SetActive(false);
            }
            SetOpen(false);
        }

        /// <summary>Toggle the open state.</summary>
        public void Toggle()
        {
            if (IsOpen) Hide();
            else Show();
        }

        /// <summary>Restore the persisted open state: reopen the window if it was open.</summary>
        // Invoked from Start(); private because the controller restores itself — mods only subscribe to
        // OnOpenChanged and react.
        private void RestoreState()
        {
            if (_settings != null && _settings.HasWindowVisible && _settings.WindowVisible)
            {
                Show();
            }
        }

        /// <summary>
        /// Destroy the window on the owner's request (the controller itself stays on the mod GO). The
        /// state becomes closed.
        /// </summary>
        public void Dismiss()
        {
            if (_popupDialog != null)
            {
                // Unhook first: dismissing triggers the destruction, and we must not re-enter
                // OnPopupDestroyed to notify a close we requested ourselves.
                _popupDialog.onDestroy.RemoveListener(OnPopupDestroyed);
                _popupDialog.Dismiss();
                DetachWindow();
            }
            SetOpen(false);
        }

        public GameObject GetGameObject()
        {
            return _popupDialog == null ? null : _popupDialog.popupWindow;
        }

        // =====================
        // Window instance wiring
        // =====================

        /// <summary>Adopt a freshly spawned window and subscribe its per-spawn listeners.</summary>
        private void AttachWindow(PopupSpawnResult result)
        {
            _popupDialog = result.PopupDialog;
            _canvasGroup = result.CanvasGroup;
            _closeButtonController = result.CloseButtonController;

            _popupDialog.onDestroy.AddListener(OnPopupDestroyed);
            if (_closeButtonController != null)
            {
                _closeButtonController.OnClick.Add(Hide);
            }
        }

        /// <summary>Drop the reference to the window instance (its components die with the window itself).</summary>
        private void DetachWindow()
        {
            _popupDialog = null;
            _canvasGroup = null;
            _closeButtonController = null;
        }

        /// <summary>
        /// Called when KSP destroys the window on its own (e.g. the user presses Escape) — a close not
        /// initiated through <see cref="Hide"/> or <see cref="Dismiss"/>.
        /// </summary>
        private void OnPopupDestroyed()
        {
            // Grab the position while the transform is still alive, then drop the dead instance and
            // resync the owner (toolbar toggle, etc.) through OnOpenChanged.
            CaptureWindowPosition();
            DetachWindow();
            SetOpen(false);
        }

        /// <summary>
        /// Re-asserts the window's interactivity after a KSP scene change (no-op if none is alive).
        /// </summary>
        // Not named OnLevelWasLoaded: that name collides with Unity's deprecated magic message, which
        // expects an (int) parameter and logs a warning when found on a MonoBehaviour.
        private void OnSceneLoaded(GameScenes scene)
        {
            RestoreInteractivity();
        }

        /// <summary>
        /// Called when KSP is about to leave the current scene, while the window transform is still alive.
        /// Grabs the dragged position so it survives a scene change even if the window was never closed —
        /// KSP's PopupDialog exposes no drag/move event, so this is our last chance to persist it.
        /// </summary>
        private void OnSceneUnloading(GameScenes scene)
        {
            CaptureWindowPosition();
        }

        // =====================
        // Internal helpers
        // =====================

        private IEnumerator _ApplyUguiPositionAfterLayout()
        {
            // Wait one frame so KSP's layout pass (which re-applies the spawn position) has settled.
            yield return null;
            if (_settings.HasWindowPosition)
            {
                SetPosition(_settings.WindowPosition);
            }
            Reveal();
        }

        /// <summary>Persist the open state and notify — on a real transition only.</summary>
        private void SetOpen(bool open)
        {
            if (IsOpen == open) return;
            IsOpen = open;
            if (_settings != null)
            {
                _settings.SetWindowVisible(open);
                _settings.Save();
            }
            OnOpenChanged.Fire();
        }

        /// <summary>Move the window to the given position, preserving its current z.</summary>
        private void SetPosition(Vector2 position)
        {
            if (_popupDialog == null || _popupDialog.RTrf == null) return;
            Vector3 lp = _popupDialog.RTrf.localPosition;
            _popupDialog.RTrf.localPosition = new Vector3(position.x, position.y, lp.z);
        }

        /// <summary>Make the window visible.</summary>
        private void Reveal()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>Re-enable pointer interaction on the window.</summary>
        private void RestoreInteractivity()
        {
            // KSP bug: on a scene change, UIMasterController.OnSceneChange clears the modal stack via
            // UnregisterModalDialogs() WITHOUT restoring blocksRaycasts on the surviving non-modal
            // dialogs. Our window persists across scenes, so if a modal dialog was up before the
            // transition (e.g. the KSC "exit to main menu" confirmation), the window stays visible
            // but non-interactive. We re-assert the resting state of a non-modal dialog.
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>Persist the window's current position (no-op if none is alive).</summary>
        private void CaptureWindowPosition()
        {
            if (_popupDialog != null && _popupDialog.RTrf != null)
            {
                _settings.SetWindowPosition(_popupDialog.RTrf.localPosition);
                _settings.Save();
                OnPositionCaptured.Fire(_settings.WindowPosition);
            }
        }
    }
}
