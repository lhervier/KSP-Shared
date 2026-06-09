using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.button
{
    public class ButtonController : MonoBehaviour
    {
        private const float DisabledAlpha = 0.25f;

        private Text _label;
        private Button _button;
        private CanvasGroup _canvasGroup;
        private bool _interactable = true;

        // Text color of the button when at rest.
        // This value is captured at initialization, and used when changing the interactable state.
        private Color _restingTextColor = ButtonPalette.ButtonTextColor;
        
        public EventVoid OnClick = new EventVoid("KSPShared.UGUI.Button.OnClick");

        // =====================================================
        // Life cycle
        // =====================================================

        public ButtonController Label(Text label)
        {
            this._label = label;
            return this;
        }

        public ButtonController Button(Button button)
        {
            this._button = button;
            return this;
        }

        public ButtonController CanvasGroup(CanvasGroup canvasGroup)
        {
            this._canvasGroup = canvasGroup;
            return this;
        }

        public ButtonController Interactable(bool interactable)
        {
            this._interactable = interactable;
            return this;
        }

        public void Start()
        {
            if( this._button != null )
            {
                _button.onClick.AddListener(_OnClick);
            }
            this.SetInteractable(_interactable);
            if (_label != null) 
            {
                this._restingTextColor = _label.color;
            }
        }

        public void OnDestroy()
        {
            if( this._button != null )
            {
                _button.onClick.RemoveListener(_OnClick);
            }
        }

        private void _OnClick()
        {
            OnClick.Fire();
        }

        // ============================================
        // Public API
        // ============================================

        public void SetLabel(string text)
        {
            if (_label == null) return;
            _label.text = text;
        }

        public bool IsInteractable()
        {
            return _button != null && _button.interactable;
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null) _button.interactable = interactable;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = interactable ? 1f : DisabledAlpha;
                _canvasGroup.blocksRaycasts = interactable;
                _canvasGroup.interactable = interactable;
            }
            // Reset to the resting color in case the label was left in the "hover white" state when disabled.
            if (_label != null) _label.color = _restingTextColor;
        }
    }
}