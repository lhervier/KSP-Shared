using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.textfield
{
    /// <summary>
    /// Builds a reusable text field (a styled TMP_InputField: bordered background, clipped viewport,
    /// text + placeholder). Single-line by default, multi-line via Multiline(true). The KSP keyboard
    /// lock is handled by the returned TextFieldController. Default colors/metrics: TextFieldPalette.
    /// </summary>
    public class TextFieldBuilder : IUGUIBuilder<TextFieldController>
    {
        // ===================================================
        // Builder parameters
        // ===================================================

        private Transform _parent;
        public TextFieldBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _text = string.Empty;
        public TextFieldBuilder WithText(string text)
        {
            this._text = text ?? string.Empty;
            return this;
        }

        private string _placeholder = string.Empty;
        public TextFieldBuilder WithPlaceholder(string placeholder)
        {
            this._placeholder = placeholder ?? string.Empty;
            return this;
        }

        private bool _multiline = false;
        public TextFieldBuilder WithMultilineState(bool multiline)
        {
            this._multiline = multiline;
            return this;
        }

        // Field height. Default: TextFieldPalette.FieldHeight (suited to single-line); a multi-line
        // caller usually sets a larger height.
        private float _height = TextFieldPalette.FieldHeight;
        public TextFieldBuilder WithHeight(float height)
        {
            this._height = height;
            return this;
        }

        private int _fontSize = TextFieldPalette.FontSize;
        public TextFieldBuilder WithFontSize(int fontSize)
        {
            this._fontSize = fontSize;
            return this;
        }

        // =======================================
        // Build
        // =======================================

        public TextFieldController Build()
        {
            var inputGo = new GameObject("TextField", typeof(RectTransform));
            if (_parent != null) inputGo.transform.SetParent(_parent, false);

            var le = inputGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = _height;

            var bg = inputGo.AddComponent<Image>();
            bg.sprite = SpritesGlobal.Border(TextFieldPalette.BgColor, TextFieldPalette.BorderColor, TextFieldPalette.BorderThickness);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
            bg.raycastTarget = true;

            var input = inputGo.AddComponent<TMP_InputField>();
            input.lineType = _multiline ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;

            int padH = Mathf.RoundToInt(TextFieldPalette.PaddingH);
            int padV = _multiline ? Mathf.RoundToInt(TextFieldPalette.PaddingV) : 0;
            TextAlignmentOptions align = _multiline ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Left;

            // Dedicated viewport inset by the padding. TMP_InputField assumes the text component fills
            // its viewport exactly, so the padding lives on the viewport (not as text offsets): the
            // caret-follow scrolling (which clamps to the viewport rect) and the on-blur position reset
            // then both honor the padding. The RectMask2D clips the overflowing text/placeholder.
            var viewport = NewFillingChild(inputGo.transform, "Viewport");
            viewport.offsetMin = new Vector2(padH, padV);
            viewport.offsetMax = new Vector2(-padH, -padV);
            viewport.gameObject.AddComponent<RectMask2D>();

            var placeholder = NewFieldText(viewport, "Placeholder", align);
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = TextFieldPalette.PlaceholderColor;
            placeholder.text = _placeholder;

            var text = NewFieldText(viewport, "Text", align);
            text.color = TextFieldPalette.TextColor;

            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = DefaultPalette.Font;
            input.text = _text;

            return inputGo
                .AddComponent<TextFieldController>()
                .WithInputField(input);
        }

        private TextMeshProUGUI NewFieldText(Transform parent, string objectName, TextAlignmentOptions align)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = UGUILabels.AddLabel(go);
            label.fontSize = _fontSize;
            label.alignment = align;
            label.richText = false;
            label.enableWordWrapping = _multiline;
            // Make the label fill the viewport EXACTLY — and do it AFTER adding the TMP component: adding a
            // TextMeshProUGUI resets the RectTransform's sizeDelta to TMP's default (~200x50), which on
            // stretch anchors inflates the rect well beyond the viewport and pushes the text outside the
            // field (the value then renders off to the left until TMP repositions it on focus).
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return label;
        }

        // Creates a child that stretches to fully fill its parent (zero offsets).
        private static RectTransform NewFillingChild(Transform parent, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
