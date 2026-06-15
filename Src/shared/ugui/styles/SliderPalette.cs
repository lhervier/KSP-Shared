using UnityEngine;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    /// <summary>
    /// Default colors and metrics for the shared slider (SliderBuilder): dark track, accent-green fill
    /// and handle. Nothing is hard-coded in the builder: everything comes from here.
    /// </summary>
    public static class SliderPalette
    {
        // Height of the area the slider occupies in a layout (tall enough for the handle).
        public const float Height = 16f;
        // Thickness of the track (the thin background bar, vertically centered).
        public const float TrackHeight = 4f;
        // Side length of the square handle.
        public const float HandleSize = 14f;
        public const int HandleBorderThickness = 1;

        public static readonly Color TrackColor = Rgb(42, 42, 42);                 // #2a2a2a
        public static readonly Color FillColor = DefaultPalette.AccentColor;        // accent green
        public static readonly Color HandleColor = DefaultPalette.AccentColor;      // accent green
        public static readonly Color HandleBorderColor = DefaultPalette.AccentBorderColor;
    }
}
