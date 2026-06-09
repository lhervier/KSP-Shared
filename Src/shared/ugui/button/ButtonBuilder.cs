using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.button
{
    public class ButtonBuilder : IUGUIBuilder<ButtonController>
    {
        private float _size = ButtonPalette.ButtonSize;
        private int _fontSize = ButtonPalette.ButtonFontSize;
        
        private string _objectName = "Button";
        private string _label = string.Empty;
        private bool _interactable = true;

        private Color _backgroundColor = ButtonPalette.ButtonColor;
        private Color _hoverColor = ButtonPalette.ButtonHoverColor;
        private Color _textColor = ButtonPalette.ButtonTextColor;
        
        // Auto width mode (text-button) : width follow the content via ContentSizeFitter, and
        // _size became the height. _paddingH is padding left and right horizontaly.
        private bool _autoWidth = false;
        private float _paddingH = 0f;

        // ===========================================================
        // Builder parameters
        // ===========================================================

        /// <summary>Button size : square edges, or height in auto width mode.</summary>
        public ButtonBuilder Size(float size)
        {
            this._size = size;
            return this;
        }

        public ButtonBuilder FontSize(int fontSize)
        {
            this._fontSize = fontSize;
            return this;
        }

        public ButtonBuilder ObjectName(string objectName)
        {
            this._objectName = objectName;
            return this;
        }

        public ButtonBuilder Label(string label)
        {
            this._label = label;
            return this;
        }

        public ButtonBuilder Interactable(bool interactable)
        {
            this._interactable = interactable;
            return this;
        }

        public ButtonBuilder BackgroundColor(Color backgroundColor)
        {
            this._backgroundColor = backgroundColor;
            return this;
        }

        public ButtonBuilder HoverColor(Color hoverColor)
        {
            this._hoverColor = hoverColor;
            return this;
        }

        public ButtonBuilder TextColor(Color textColor)
        {
            this._textColor = textColor;
            return this;
        }

        public ButtonBuilder AutoWidth(float paddingH)
        {
            this._autoWidth = true;
            this._paddingH = paddingH;
            return this;
        }

        public ButtonBuilder DisableAutoWidth()
        {
            this._autoWidth = false;
            this._paddingH = 0f;
            return this;
        }

        // ===========================================================
        // Build the button
        // ===========================================================

        public ButtonController Build()
        {
            var buttonGo = new GameObject(_objectName, typeof(RectTransform));
            
            var layoutElement = buttonGo.AddComponent<LayoutElement>();
            if (_autoWidth)
            {
                // Largeur libre (pilotée par le contenu), hauteur fixée.
                layoutElement.preferredHeight = _size;
                layoutElement.minHeight = _size;
            }
            else
            {
                // Carré de côté _size.
                layoutElement.preferredWidth = _size;
                layoutElement.preferredHeight = _size;
                layoutElement.minWidth = _size;
                layoutElement.minHeight = _size;
            }

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
            
            // CanvasGroup applies a global alpha to the button (background + text + future children),
            // and also blocks raycasts when disabled — matches the mockup's .ka:disabled { opacity: .25 }.
            var canvasGroup = buttonGo.AddComponent<CanvasGroup>();
            Text label = _autoWidth ? BuildAutoWidthLabel(buttonGo) : BuildFixedLabel(buttonGo);
            
            // Hover tint (label → white) only for fixed/square buttons, via PointerHandler rather than
            // EventTrigger: PointerHandler does NOT implement IScrollHandler/IDragHandler, so the mouse
            // wheel and drag bubble up to a parent ScrollRect instead of being swallowed by the button.
            // Text buttons keep their configured color (no hover tint), matching their styled look.
            if (!_autoWidth)
            {
                var hover = buttonGo.AddComponent<PointerHandler>();
                hover.OnEnter = () => label.color = Color.white;
                hover.OnExit = () => label.color = _textColor;
            }

            // Apply the initial interactable state via the controller (single source of truth)
            return buttonGo
                .AddComponent<ButtonController>()
                .Button(button)
                .CanvasGroup(canvasGroup)
                .Label(label)
                .Interactable(_interactable);
        }

        /// <summary>
        /// Centered label that doesn't affect the button's width (square mode).
        /// </summary>
        /// <param name="buttonGo">The parent game object</param>
        /// <returns></returns>
        private Text BuildFixedLabel(GameObject buttonGo)
        {
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.AddComponent<Text>();
            label.text = _label;
            label.font = HighLogic.UISkin.font;
            label.fontSize = _fontSize;
            label.color = _textColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>
        /// Label thte drives the button width (auto width mode).
        /// </summary>
        /// <param name="buttonGo">The parent game object</param>
        /// <returns></returns>
        private Text BuildAutoWidthLabel(GameObject buttonGo)
        {
            var layout = buttonGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(_paddingH), Mathf.RoundToInt(_paddingH), 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = buttonGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.text = _label;
            label.font = HighLogic.UISkin.font;
            label.fontSize = _fontSize;
            label.color = _textColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }
    }
}