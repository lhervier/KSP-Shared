using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class DefaultPalette
    {
        // Default values
        public const float PaddingLeft = 8f;
        public const float PaddingRight = 8f;
        public const float PaddingTop = 5f;
        public const float PaddingBottom = 5f;
        public const float Spacing = 6f;
        
        // Default colors
        public static readonly Color AccentColor = new Color(141f / 255f, 190f / 255f, 69f / 255f);
        public static readonly Color LabelColor = new Color(187f / 255f, 187f / 255f, 187f / 255f);
        public static readonly Color FieldBackgroundColor = new Color(42f / 255f, 42f / 255f, 42f / 255f);
        public static readonly Color SeparatorColor = new Color(42f / 255f, 42f / 255f, 42f / 255f);

        // Icons
        public const int IconSize = 18;
        
        // Buttons
        public const float ButtonSize = 18f;
        public const int ButtonFontSize = 13;
        public static readonly Color ButtonColor = new Color(42f / 255f, 42f / 255f, 42f / 255f);
        public static readonly Color ButtonHoverColor = new Color(56f / 255f, 56f / 255f, 56f / 255f);
        public static readonly Color ButtonTextColor = new Color(187f / 255f, 187f / 255f, 187f / 255f);
        public static readonly Color ButtonDisabledTextColor = new Color(187f / 255f, 187f / 255f, 187f / 255f, 0.25f);
        
        // Checkbox
        public const float CheckboxSize = 12f;
        public const float CheckmarkInset = 2f;
        public const int CheckboxFontSize = 12;
    }
}
