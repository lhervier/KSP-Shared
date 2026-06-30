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

        public void OnEnable()
        {
            // The field keeps resetOnDeActivation off (so the value stays visible without focus); the side
            // effect is that TMP no longer clears the caret mesh on blur, so we hide it ourselves. This runs
            // on every activation (TMP — added before this component — has created the caret by now), so a
            // freshly shown field starts caret-less; OnInputSelected re-shows it on focus. Start() is too
            // early: TMP only creates the caret the first time it is enabled with a textComponent assigned.
            if (_input != null && !_input.isFocused) SetCaretVisible(false);
        }

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
            SetCaretVisible(true);
        }

        private void OnInputDeselected(string _)
        {
            InputLockManager.RemoveControlLock(LockId);
            SetCaretVisible(false);
        }

        // TMP creates the caret lazily as a TMP_SelectionCaret under the text viewport. Toggling its
        // Graphic.enabled fully shows/hides the caret mesh; while focused, TMP's own blink still works
        // (it empties the mesh on blink-off, independently of this enabled flag).
        private TMP_SelectionCaret _caret;
        private void SetCaretVisible(bool visible)
        {
            if (_caret == null && _input != null && _input.textViewport != null)
            {
                _caret = _input.textViewport.GetComponentInChildren<TMP_SelectionCaret>(true);
            }
            if (_caret != null) _caret.enabled = visible;
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
