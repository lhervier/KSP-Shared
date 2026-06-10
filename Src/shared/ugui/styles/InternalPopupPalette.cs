using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class InternalPopupPalette
    {
        // Inset from the parent edges (keeps the host window's 1px chrome border visible behind the dim layer).
        public const float BorderThickness = 1f;

        // Modal dim layer covering the parent behind the card.
        public static readonly Color DimColor = new Color(8f / 255f, 8f / 255f, 8f / 255f, 0.78f);

        // Centered card.
        public const float CardWidth = 352f;
        public const float CardPadding = 14f;
        public const float CardSpacing = 9f;
        public const int CardBorderThickness = 1;
        public static readonly Color CardBackgroundColor = new Color(26f / 255f, 26f / 255f, 26f / 255f);
        public static readonly Color CardBorderColor = new Color(85f / 255f, 85f / 255f, 85f / 255f);

        // Title.
        public const int TitleFontSize = 11;
    }
}
