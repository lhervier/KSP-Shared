using UnityEngine;
using System.Collections;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popup
{
    public class PopupController : MonoBehaviour
    {
        public EventData<Vector2> OnPositionCaptured = new EventData<Vector2>("PopupController.OnPositionCaptured");
        public EventVoid OnClosed = new EventVoid("PopupController.OnClosed");

        // =========================
        // Life cycle
        // =========================

        // Dependencies injected by the builder right after AddComponent, before Start() runs.

        /// <summary>Inject the KSP popup this controller drives.</summary>
        private PopupDialog _popupDialog;
        public PopupController WithPopupDialog(PopupDialog popupDialog)
        {
            this._popupDialog = popupDialog;
            return this;
        }

        /// <summary>Inject the popup's canvas group.</summary>
        private CanvasGroup _canvasGroup;
        public PopupController WithCanvasGroup(CanvasGroup canvasGroup)
        {
            this._canvasGroup = canvasGroup;
            return this;
        }

        private ButtonController _closeButtonController;
        public PopupController WithCloseButtonController(ButtonController closeButtonController)
        {
            this._closeButtonController = closeButtonController;
            return this;
        }

        private PopupSettings _settings;
        public PopupController WithSettings(PopupSettings settings)
        {
            this._settings = settings;
            return this;
        }

        /// <summary>
        /// Unity callback. Sets up the controller; its counterpart is <see cref="OnDestroy"/>.
        /// </summary>
        public void Start()
        {
            _popupDialog?.onDestroy.AddListener(OnPopupDestroyed);
            GameEvents.onLevelWasLoaded.Add(OnSceneLoaded);
            GameEvents.onGameSceneLoadRequested.Add(OnSceneUnloading);

            if( this._closeButtonController != null )
            {
                this._closeButtonController.OnClick.Add(Hide);
            }
        }

        /// <summary>
        /// Unity callback. Tears down what <see cref="Start"/> set up.
        /// </summary>
        public void OnDestroy()
        {
            if( this._closeButtonController != null )
            {
                this._closeButtonController.OnClick.Remove(Hide);
            }

            // Pure cleanup: do NOT dismiss the dialog here. This runs on both teardown paths (the
            // owner-driven Dismiss and KSP destroying the popup itself), and in both the popup is
            // already being destroyed — dismissing again here would re-enter the teardown.
            GameEvents.onLevelWasLoaded.Remove(OnSceneLoaded);
            GameEvents.onGameSceneLoadRequested.Remove(OnSceneUnloading);
            _popupDialog?.onDestroy.RemoveListener(OnPopupDestroyed);
        }

        /// <summary>
        /// Close the window on the owner's request. The controller is destroyed as a result, so the
        /// owner must drop its reference afterwards.
        /// </summary>
        public void Dismiss()
        {
            // Unhook our KSP listener first: dismissing triggers the destruction, and we must not
            // re-enter OnPopupDestroyed to notify the owner of a close it requested itself.
            _popupDialog?.onDestroy.RemoveListener(OnPopupDestroyed);
            _popupDialog?.Dismiss();
        }

        /// <summary>
        /// Called when KSP destroys the popup on its own (e.g. the user presses Escape) — a close not
        /// initiated through <see cref="Hide"/> or <see cref="Dismiss"/>.
        /// </summary>
        private void OnPopupDestroyed()
        {
            // Grab the position while the transform is still alive, then let the owner resync
            // (reset the toolbar toggle, close the rest of the UI).
            CaptureWindowPosition();
            OnClosed.Fire();
        }

        /// <summary>
        /// Re-asserts the window's interactivity after a KSP scene change (no-op if it is closed).
        /// </summary>
        // Not named OnLevelWasLoaded: that name collides with Unity's deprecated magic message, which
        // expects an (int) parameter and logs a warning when found on a MonoBehaviour.
        private void OnSceneLoaded(GameScenes scene)
        {
            this.RestoreInteractivity();
        }

        /// <summary>
        /// Called when KSP is about to leave the current scene (quit to menu, scene switch…), while the
        /// window transform is still alive. Grabs the dragged position so it survives a scene change
        /// even if the window was never closed — KSP's PopupDialog exposes no drag/move event, so this
        /// is our last chance to persist a position the user changed by dragging.
        /// </summary>
        private void OnSceneUnloading(GameScenes scene)
        {
            CaptureWindowPosition();
        }

        // =====================
        // Public API
        // =====================

        public GameObject GetGameObject()
        {
            return _popupDialog.popupWindow;
        }

        /// <summary>
        /// Show the window at its last saved position. Also used to re-open it after a
        /// <see cref="Hide"/>.
        /// </summary>
        public void Show()
        {
            _popupDialog?.gameObject.SetActive(true);
            // Restore the dragged position one frame later: KSP re-applies the spawn position on the
            // layout pass that follows activation, so applying it now would be overwritten.
            StartCoroutine(_ApplyUguiPositionAfterLayout());
        }

        /// <summary>
        /// Apply the saved window position and reveal the window.
        /// </summary>
        private IEnumerator _ApplyUguiPositionAfterLayout()
        {
            // Wait one frame so KSP's layout pass (which re-applies the spawn position) has settled.
            yield return null;
            if( _settings.HasWindowPosition )
            {
                SetPosition(_settings.WindowPosition);
            }
            Reveal();
        }

        /// <summary>
        /// Hide the window, saving its position. The controller stays alive so the window can be
        /// shown again later.
        /// </summary>
        public void Hide()
        {
            CaptureWindowPosition();
            _popupDialog?.gameObject.SetActive(false);
            this.OnClosed.Fire();
        }

        // =====================
        // Internal API
        // =====================
        
        /// <summary>Move the window to the given position, preserving its current z.</summary>
        private void SetPosition(Vector2 position)
        {
            if( _popupDialog.RTrf == null ) return;
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

        /// <summary>Report the window's current position so the owner can persist it.</summary>
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
