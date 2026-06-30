using System.Collections;
using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.shared.ugui.textfield
{
    /// <summary>
    /// Drives a shared text field (single- or multi-line). Fully encapsulates the KSP keyboard lock:
    /// the game keyboard is locked while the field holds the focus, and unlocked on blur (or when the
    /// field is destroyed). Exposes the value through the OnValueChanged event and the Get/SetText API.
    /// </summary>
    public class TextFieldController : MonoBehaviour
    {
        /// <summary>Fired on every text change (keyboard input or SetText).</summary>
        public EventData<string> OnValueChanged = new EventData<string>("KSPShared.UGUI.TextField.OnValueChanged");

        /// <summary>Fired on submit (Enter in single-line, or loss of focus).</summary>
        public EventData<string> OnEndEdit = new EventData<string>("KSPShared.UGUI.TextField.OnEndEdit");

        // Lock id unique to this instance, to avoid any collision between fields.
        private string _lockId;
        private string LockId => _lockId ?? (_lockId = "KSPShared.TextField." + GetInstanceID());

        // True during SetText: assigning .text programmatically fires onValueChanged synchronously, so
        // this flag lets us not propagate that "non-user" change to OnValueChanged.
        private bool _suppressNotify = false;

        // ============================================
        // Life cycle
        // ============================================

        private TMP_InputField _input;
        public TextFieldController WithInputField(TMP_InputField input)
        {
            this._input = input;
            return this;
        }

        public void Start()
        {
            if (_input != null)
            {
                _input.onValueChanged.AddListener(OnInputValueChanged);
                _input.onEndEdit.AddListener(OnInputEndEdit);
                _input.onSelect.AddListener(OnInputSelected);
                _input.onDeselect.AddListener(OnInputDeselected);
            }
        }

        // Hides the caret whenever the field is not focused. The field keeps resetOnDeActivation off (so the
        // value stays visible without focus, see TextFieldBuilder), which means TMP no longer clears the
        // caret when unfocused and re-shows it on many internal paths (text changes, vertex/layout updates)
        // that are hard to intercept one by one. So we enforce it every frame: deactivating the caret
        // GameObject hides it for good — TMP keeps re-setting the caret mesh on its CanvasRenderer regardless
        // of Graphic.enabled, but a deactivated GameObject renders nothing. TMP re-shows and blinks the caret
        // on its own once it is active again on focus. The check is trivial and guarded; the cost is
        // negligible. We never touch the text position: left alone, TMP keeps the value correctly placed.
        public void LateUpdate()
        {
            if (_input == null) return;
            if (_caret == null && _input.textViewport != null)
            {
                _caret = _input.textViewport.GetComponentInChildren<TMP_SelectionCaret>(true);
            }
            if (_caret != null && _caret.gameObject.activeSelf != _input.isFocused)
            {
                _caret.gameObject.SetActive(_input.isFocused);
            }
        }
        private TMP_SelectionCaret _caret;

        public void OnDestroy()
        {
            if (_input != null)
            {
                _input.onValueChanged.RemoveListener(OnInputValueChanged);
                _input.onEndEdit.RemoveListener(OnInputEndEdit);
                _input.onSelect.RemoveListener(OnInputSelected);
                _input.onDeselect.RemoveListener(OnInputDeselected);
            }
            // Safety net: if the field is destroyed while focused, the lock would otherwise stay active
            // and freeze the game controls.
            InputLockManager.RemoveControlLock(LockId);
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnInputValueChanged(string value)
        {
            if (_suppressNotify) return;
            OnValueChanged.Fire(value);
        }

        private void OnInputEndEdit(string value)
        {
            OnEndEdit.Fire(value);
        }

        private void OnInputSelected(string _)
        {
            InputLockManager.SetControlLock(ControlTypes.All, LockId);
        }

        private void OnInputDeselected(string _)
        {
            InputLockManager.RemoveControlLock(LockId);
        }

        // ============================================
        // Public API
        // ============================================

        public string GetText()
        {
            return _input != null ? _input.text : string.Empty;
        }

        /// <summary>
        /// Sets the text. Does NOT fire OnValueChanged (model-to-view sync without a feedback loop).
        /// Use actual keyboard input to trigger the event.
        /// </summary>
        public void SetText(string text)
        {
            if (_input == null) return;
            _suppressNotify = true;
            try
            {
                _input.text = text ?? string.Empty;
            }
            finally
            {
                _suppressNotify = false;
            }
            // A caller may set the value while the field is not laid out yet (e.g. refreshing fields from
            // OnEnable, before the first layout pass). TMP then builds the text mesh against a zero-sized
            // rect and leaves it invisible until the field is focused. Regenerate the mesh once layout has
            // settled (next frame). Position is left untouched.
            if (isActiveAndEnabled) StartCoroutine(RegenerateMeshNextFrame());
        }

        private IEnumerator RegenerateMeshNextFrame()
        {
            yield return null;
            if (_input != null && _input.textComponent != null) _input.textComponent.ForceMeshUpdate();
        }
    }
}
