using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.button
{
    public class ButtonBuilder : IUGUIBuilder<ButtonController>
    {
        private string _objectName = "Button";
        private string _buttonLabel = string.Empty;
        private bool _interactable = true;
        private Color _backgroundColor = DefaultPalette.ButtonColor;
        private Color _hoverColor = DefaultPalette.ButtonHoverColor;

        public void SetObjectName(string objectName)
        {
            this._objectName = objectName;
        }

        public void SetLabel(string label)
        {
            this._buttonLabel = label;
        }

        public void SetInteractable(bool interactable)
        {
            this._interactable = interactable;
        }

        public void SetBackgroundColor(Color backgroundColor)
        {
            this._backgroundColor = backgroundColor;
        }

        public void SetHoverColor(Color hoverColor)
        {
            this._hoverColor = hoverColor;
        }

        public ButtonController Build()
        {
            var buttonGo = new GameObject(_objectName, typeof(RectTransform));
            ButtonController controller = buttonGo.AddComponent<ButtonController>();

            var layoutElement = buttonGo.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = DefaultPalette.ButtonSize;
            layoutElement.preferredHeight = DefaultPalette.ButtonSize;
            layoutElement.minWidth = DefaultPalette.ButtonSize;
            layoutElement.minHeight = DefaultPalette.ButtonSize;

            // White background fill so the Button's color tint applies as-is (no multiplication)
            var image = buttonGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            // Button: hover/press color transitions on the background, plus the click handler
            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = _backgroundColor;
            colors.highlightedColor = _hoverColor;
            colors.pressedColor = _backgroundColor;
            colors.selectedColor = _backgroundColor;
            // Suppress Unity's default disabled gray tint; the CanvasGroup below does the fade.
            colors.disabledColor = _backgroundColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(() => controller.OnClick.Fire());
            controller.InitButton(button);

            // CanvasGroup applies a global alpha to the button (background + text + future children),
            // and also blocks raycasts when disabled — matches the mockup's .ka:disabled { opacity: .25 }.
            var canvasGroup = buttonGo.AddComponent<CanvasGroup>();
            controller.InitCanvasGroup(canvasGroup);

            // Button label, centered in the button
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.AddComponent<Text>();
            label.text = _buttonLabel;
            label.font = HighLogic.UISkin.font;
            label.fontSize = 13;
            label.color = DefaultPalette.ButtonTextColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            controller.InitLabel(label);

            // EventTrigger swaps the label color to white on hover. When disabled, the CanvasGroup
            // sets blocksRaycasts = false so these never fire — no need for an extra interactable check.
            var trigger = buttonGo.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => label.color = Color.white);
            trigger.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => label.color = DefaultPalette.ButtonTextColor);
            trigger.triggers.Add(exitEntry);

            // Apply the initial interactable state via the controller (single source of truth)
            controller.SetInteractable(_interactable);

            return controller;
        }
    }
}