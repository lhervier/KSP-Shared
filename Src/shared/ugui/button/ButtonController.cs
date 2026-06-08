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
        public EventVoid OnClick = new EventVoid("KSPShared.UGUI.Button.OnClick");

        public void InitLabel(Text label)
        {
            this._label = label;
        }

        public void InitButton(Button button)
        {
            this._button = button;
        }

        public void InitCanvasGroup(CanvasGroup canvasGroup)
        {
            this._canvasGroup = canvasGroup;
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
            // Reset to default in case the label was left in the "hover white" state when disabled.
            if (_label != null) _label.color = DefaultPalette.ButtonTextColor;
        }
    }
}