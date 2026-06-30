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

        public void LateUpdate()
        {
            // The field keeps resetOnDeActivation off so the value stays visible without focus (TMP would
            // otherwise move the text to a position it snapshots before the first layout pass, i.e. wrong
            // for a runtime-built field, hiding the value until next focus). The trade-off is that TMP no
            // longer clears the caret when unfocused and re-shows it on any text/vertex update; rather than
            // chase every such path, we just keep the caret's visibility matched to the focus state here.
            // While focused, TMP's own blink keeps working (it toggles the caret mesh, not this flag).
            if (_caret == null && _input != null && _input.textViewport != null)
            {
                _caret = _input.textViewport.GetComponentInChildren<TMP_SelectionCaret>(true);
            }
            if (_caret != null && _input != null && _caret.enabled != _input.isFocused)
            {
                _caret.enabled = _input.isFocused;
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
        }
    }
}
