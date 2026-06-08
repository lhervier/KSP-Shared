using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.checkbox
{
    public class CheckboxBuilder : IUGUIBuilder<CheckboxController>
    {
        private bool _isChecked = false;

        public void SetChecked(bool isChecked)
        {
            this._isChecked = isChecked;
        }

        public CheckboxController Build()
        {
            var go = new GameObject("Checkbox", typeof(RectTransform));
            CheckboxController controller = go.AddComponent<CheckboxController>();

            // Fixed 12x12 square
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = DefaultPalette.CheckboxSize;
            layoutElement.preferredHeight = DefaultPalette.CheckboxSize;
            layoutElement.minWidth = DefaultPalette.CheckboxSize;
            layoutElement.minHeight = DefaultPalette.CheckboxSize;

            // Dark background
            var bgImage = go.AddComponent<Image>();
            bgImage.sprite = SpritesGlobal.FillSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.color = DefaultPalette.FieldBackgroundColor;
            bgImage.raycastTarget = true;

            // Green inner fill that represents the "checked" state
            var checkmarkGo = new GameObject("Checkmark", typeof(RectTransform));
            checkmarkGo.transform.SetParent(go.transform, false);
            controller.BindCheckmark(checkmarkGo);

            var checkmarkRect = checkmarkGo.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = new Vector2(DefaultPalette.CheckmarkInset, DefaultPalette.CheckmarkInset);
            checkmarkRect.offsetMax = new Vector2(-DefaultPalette.CheckmarkInset, -DefaultPalette.CheckmarkInset);

            var checkmarkImage = checkmarkGo.AddComponent<Image>();
            checkmarkImage.sprite = SpritesGlobal.FillSprite;
            checkmarkImage.type = Image.Type.Simple;
            checkmarkImage.color = DefaultPalette.AccentColor;
            checkmarkImage.raycastTarget = false;

            // Initial visibility from the parameter; checkmarkGo.activeSelf is the source of truth
            // (so external updates via SetActive stay consistent with what the click handler reads).
            checkmarkGo.SetActive(_isChecked);

            var button = go.AddComponent<Button>();
            button.targetGraphic = bgImage;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;
            controller.BindButton(button);

            return controller;
        }
    }
}
