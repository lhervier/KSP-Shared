using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class PopinPalette
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

        // Message (content of a ConfirmPopin).
        public const int MessageFontSize = 13;
        public static readonly Color MessageColor = Utils.Rgb(204, 204, 204);  // #ccc

        // Footer button bar.
        public const float FooterSpacing = 8f;   // gap between buttons
        public const float ButtonHeight = 28f;
        public const float ButtonPaddingH = 14f;
        public const int ButtonFontSize = 12;

        // Normal footer button (e.g. Cancel).
        public static readonly Color ButtonBgColor = Utils.Rgb(56, 56, 56);      // #383838
        public static readonly Color ButtonHoverColor = Utils.Rgb(72, 72, 72);   // #484848
        public static readonly Color ButtonTextColor = Utils.Rgb(187, 187, 187); // #bbb

        // Confirm footer button (positive action, e.g. Save). No hover tint (hover == background).
        public static readonly Color ButtonConfirmBgColor = Utils.Rgba(141, 190, 69, 0.12f);
        public static readonly Color ButtonConfirmHoverColor = Utils.Rgba(141, 190, 69, 0.12f);
        public static readonly Color ButtonConfirmTextColor = Utils.Rgb(141, 190, 69);  // #8dbe45

        // Alert footer button (destructive action, e.g. Remove). No hover tint (hover == background).
        public static readonly Color ButtonAlertBgColor = Utils.Rgba(192, 89, 79, 0.12f);
        public static readonly Color ButtonAlertHoverColor = Utils.Rgba(192, 89, 79, 0.12f);
        public static readonly Color ButtonAlertTextColor = Utils.Rgb(192, 89, 79);  // #c0594f
    }
}
