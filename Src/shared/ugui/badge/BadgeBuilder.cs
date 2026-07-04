using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.badge
{
    /// <summary>
    /// Builds a small badge/chip: a sliced border background holding a centered label, plus an optional
    /// leading icon. The width is driven by the content (label + padding) unless a minimum square size
    /// is requested. Returns a <see cref="BadgeController"/> so the badge can be mutated afterwards.
    ///
    /// Fluent builder in the style of <c>ButtonBuilder</c>: chain <c>WithXxx(...)</c> then call
    /// <c>Build()</c>. Colors default to the shared accent palette; callers override what they need.
    /// </summary>
    public class BadgeBuilder : IUGUIBuilder<BadgeController>
    {
        // Gap between the leading icon and the label, when an icon is present.
        private const float IconSpacing = 4f;

        // ===========================================================
        // Builder parameters
        // ===========================================================

        private string _objectName = "Badge";
        public BadgeBuilder WithObjectName(string objectName)
        {
            this._objectName = objectName;
            return this;
        }

        private Transform _parent;
        public BadgeBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _text = string.Empty;
        public BadgeBuilder WithText(string text)
        {
            this._text = text;
            return this;
        }

        private Color _textColor = DefaultPalette.AccentColor;
        private Color _bgColor = DefaultPalette.AccentBgColor;
        private Color _borderColor = DefaultPalette.AccentBorderColor;
        public BadgeBuilder WithColors(Color textColor, Color bgColor, Color borderColor)
        {
            this._textColor = textColor;
            this._bgColor = bgColor;
            this._borderColor = borderColor;
            return this;
        }

        private int _fontSize = 10;
        public BadgeBuilder WithFontSize(int fontSize)
        {
            this._fontSize = fontSize;
            return this;
        }

        private FontStyles _fontStyle = FontStyles.Normal;
        public BadgeBuilder WithFontStyle(FontStyles fontStyle)
        {
            this._fontStyle = fontStyle;
            return this;
        }

        private float _paddingH = 6f;
        private int _paddingV = 2;
        public BadgeBuilder WithPadding(float paddingH, int paddingV)
        {
            this._paddingH = paddingH;
            this._paddingV = paddingV;
            return this;
        }

        private int _borderThickness = 1;
        public BadgeBuilder WithBorderThickness(int thickness)
        {
            this._borderThickness = thickness;
            return this;
        }

        private Sprite _icon;
        private float _iconSize = 0f;   // 0 => square sized to the font size
        public BadgeBuilder WithIcon(Sprite icon)
        {
            this._icon = icon;
            return this;
        }
        public BadgeBuilder WithIcon(Sprite icon, float size)
        {
            this._icon = icon;
            this._iconSize = size;
            return this;
        }

        private float _minSize = 0f;
        public BadgeBuilder WithMinSize(float minSize)
        {
            this._minSize = minSize;
            return this;
        }

        private bool _hugContentSize = false;
        public BadgeBuilder WithContentSizeFitter()
        {
            this._hugContentSize = true;
            return this;
        }

        private string _tooltip;
        public BadgeBuilder WithTooltip(string tooltip)
        {
            this._tooltip = tooltip;
            return this;
        }

        private bool _visible = true;
        public BadgeBuilder WithVisible(bool visible)
        {
            this._visible = visible;
            return this;
        }

        // ===========================================================
        // Build the badge
        // ===========================================================

        public BadgeController Build()
        {
            var badgeGo = new GameObject(_objectName, typeof(RectTransform));
            if (_parent != null)
            {
                badgeGo.transform.SetParent(_parent, false);
            }

            // Square floor (e.g. keyboard-key chips): the badge never shrinks below this, but a long
            // label still grows it horizontally.
            if (_minSize > 0f)
            {
                var le = badgeGo.AddComponent<LayoutElement>();
                le.minWidth = _minSize;
                le.minHeight = _minSize;
            }

            // White tint: the real colors live in the generated (cached) 9-slice border sprite.
            var background = badgeGo.AddComponent<Image>();
            background.sprite = SpritesGlobal.Border(_bgColor, _borderColor, _borderThickness);
            background.type = Image.Type.Sliced;
            background.color = Color.white;
            // Only raycast when a tooltip needs to be hovered; otherwise stay out of the way.
            background.raycastTarget = _tooltip != null;

            var layout = badgeGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(_paddingH), Mathf.RoundToInt(_paddingH), _paddingV, _paddingV);
            layout.spacing = _icon != null ? IconSpacing : 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Hug the content width even inside a compressed parent (e.g. a shrinking title bar).
            if (_hugContentSize)
            {
                var fitter = badgeGo.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            // Icon first (left of the label) so the horizontal layout orders them naturally.
            Image icon = _icon != null ? BuildIcon(badgeGo) : null;
            TextMeshProUGUI label = BuildLabel(badgeGo);

            var controller = badgeGo.AddComponent<BadgeController>()
                .WithBackgroundComponent(background)
                .WithLabelComponent(label)
                .WithIconComponent(icon)
                .WithBorderThickness(_borderThickness);

            if (_tooltip != null)
            {
                Tooltips.Attach(badgeGo, _tooltip);
            }
            if (!_visible)
            {
                badgeGo.SetActive(false);
            }

            return controller;
        }

        /// <summary>Centered label filling the badge, sized by the horizontal layout + padding.</summary>
        private TextMeshProUGUI BuildLabel(GameObject badgeGo)
        {
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(badgeGo.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.text = _text;
            label.fontSize = _fontSize;
            label.fontStyle = _fontStyle;
            label.color = _textColor;
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        /// <summary>Fixed-size leading icon, its square set by the layout element.</summary>
        private Image BuildIcon(GameObject badgeGo)
        {
            float size = _iconSize > 0f ? _iconSize : _fontSize;
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(badgeGo.transform, false);
            var le = iconGo.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = size;
            le.minHeight = le.preferredHeight = size;

            var img = iconGo.AddComponent<Image>();
            img.sprite = _icon;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = Color.white;
            img.raycastTarget = false;
            return img;
        }
    }
}
