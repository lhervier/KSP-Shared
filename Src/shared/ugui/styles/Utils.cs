using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public class Utils
    {
        public static Color Rgb(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);
        public static Color Rgba(int r, int g, int b, float a) => new Color(r / 255f, g / 255f, b / 255f, a);

    }
}