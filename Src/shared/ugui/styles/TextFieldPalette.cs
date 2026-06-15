using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    /// <summary>
    /// Default colors and metrics for the shared text field (TextFieldBuilder), mirroring the mockup's
    /// search field (background #0d0d0d, border #2a2a2a, text #e8e8e8). A builder may override
    /// individual values; otherwise everything comes from here.
    /// </summary>
    public static class TextFieldPalette
    {
        // Height of a single-line field (a multi-line one sets its own height through the builder).
        public const float FieldHeight = 22f;
        public const int FontSize = 12;

        // Inner padding between the border and the text (horizontal; also vertical when multi-line).
        public const float PaddingH = 7f;
        public const float PaddingV = 8f;

        public const int BorderThickness = 1;

        public static readonly Color BgColor = Rgb(13, 13, 13);          // #0d0d0d
        public static readonly Color BorderColor = Rgb(42, 42, 42);      // #2a2a2a
        public static readonly Color TextColor = Rgb(232, 232, 232);     // #e8e8e8
        public static readonly Color PlaceholderColor = Rgb(85, 85, 85); // #555
    }
}
