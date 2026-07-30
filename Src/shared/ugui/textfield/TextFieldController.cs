using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.shared.ugui.textfield
{
    /// <summary>
    /// Drives a shared text field (single- or multi-line). Fully encapsulates the KSP keyboard lock:
    /// the game keyboard is locked while the field holds the focus, and unlocked on blur (or when the
    /// field is destroyed). Exposes the value through the OnValueChanged event and the Get/SetText API.
    /// Drives the clear button when the field was built with one: shown only while the field holds a
    /// value, and clearing the field when clicked.
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

        // Null when the field is built without a clear button.
        private PointerHandler _clearButton;
        public TextFieldController WithClearButton(PointerHandler clearButton)
        {
            this._clearButton = clearButton;
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
            if (_clearButton != null)
            {
                _clearButton.OnClick = Clear;
            }
            // The initial value is set by the builder, before any listener exists.
            UpdateClearButton(GetText());
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
            if (_clearButton != null)
            {
                _clearButton.OnClick = null;
            }
            // Safety net: if the field is destroyed while focused, the lock would otherwise stay active
            // and freeze the game controls.
            InputLockManager.RemoveControlLock(LockId);
        }

        // Unity lifecycle callback (invoked automatically on deactivation, never called by consumers).
        public void OnDisable()
        {
            // Safety net: a focused field getting deactivated (e.g. its host popup closes) may not
            // receive onDeselect, which would leave the keyboard lock active and freeze the controls.
            InputLockManager.RemoveControlLock(LockId);
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnInputValueChanged(string value)
        {
            // Refreshed before the guard below: a programmatic SetText does not notify the consumers,
            // but it does change what the field holds, so the button has to follow it too.
            UpdateClearButton(value);
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

        /// <summary>Gives the keyboard focus to the field, opening it for typing.</summary>
        public void Activate()
        {
            if (_input != null) _input.ActivateInputField();
        }

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

        /// <summary>
        /// Empties the field on the user's behalf: fires OnValueChanged (unlike SetText) and leaves
        /// the keyboard focus on the field. Bound to the clear button when the field has one.
        /// </summary>
        public void Clear()
        {
            if (_input == null) return;
            // Straight assignment rather than SetText: emptying the field IS a user-driven value
            // change, and consumers have to hear about it.
            _input.text = string.Empty;
            Activate();
        }

        // ============================================
        // Clear button
        // ============================================

        // Shows the clear button only while the field holds a value. No-op without a clear button.
        private void UpdateClearButton(string value)
        {
            if (_clearButton == null) return;
            bool visible = !string.IsNullOrEmpty(value);
            if (_clearButton.gameObject.activeSelf == visible) return;

            // Hiding the button under the pointer (which is what a click on it does) means uGUI never
            // sends it the matching exit: without this, it would come back tinted as if hovered.
            if (!visible && _clearButton.OnExit != null)
            {
                _clearButton.OnExit();
            }
            _clearButton.gameObject.SetActive(visible);
        }
    }
}
