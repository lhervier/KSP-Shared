using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.shared.ugui.checkbox
{
    public class CheckboxController : MonoBehaviour
    {
        public EventData<bool> OnToggled = new EventData<bool>("KSPShared.UGUI.Checkbox.OnToggled");

        // ============================================
        // Life cycle
        // ============================================

        private PointerHandler _clickHandler;
        public CheckboxController WithClickHandler(PointerHandler clickHandler)
        {
            this._clickHandler = clickHandler;
            return this;
        }

        private GameObject _checkMark;
        public CheckboxController WithCheckmark(GameObject checkmark)
        {
            _checkMark = checkmark;
            return this;
        }

        private TextMeshProUGUI _label;
        public CheckboxController WithLabelComponent(TextMeshProUGUI label)
        {
            _label = label;
            return this;
        }

        public void Start()
        {
            if( _clickHandler != null )
            {
                _clickHandler.OnClick = OnClicked;
            }
        }

        public void OnDestroy()
        {
            if( _clickHandler != null )
            {
                _clickHandler.OnClick = null;
            }
        }

        // ==========================================================
        // Methods bounds to events
        // ==========================================================

        private void OnClicked()
        {
            // checkMark.activeSelf is the current (pre-click) visual state, so the new desired
            // state is its negation. The callee decides whether to apply it (e.g., by reading
            // the model's state, calling a toggle method, etc.).
            SetChecked(!_checkMark.activeSelf);
        }

        // ==========================================================
        // Public API
        // ==========================================================

        public bool IsChecked()
        {
            if( _checkMark == null ) return false;
            return _checkMark.activeInHierarchy;
        }

        public void SetChecked(bool isChecked)
        {
            if( _checkMark.activeSelf == isChecked ) return;
            _checkMark.SetActive(isChecked);
            OnToggled.Fire(isChecked);
        }

        public string GetLabel()
        {
            if( _label == null ) return string.Empty;
            return _label.text;
        }

        public void SetLabel(string label)
        {
            if( _label == null ) return;
            _label.text = label ?? string.Empty;
        }
    }
}
