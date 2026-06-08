using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    internal sealed class SteamInputTextures
    {
        public static Texture2D MakeBorderTexture(Color fill, Color border, int thickness)
        {
            var size = 2 * thickness + 1;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isBorder = x < thickness || x >= size - thickness
                                || y < thickness || y >= size - thickness;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        /// <summary>
        /// 1×(2*thickness + 1) texture with top and bottom borders only (no left/right).
        /// Used as a 9-slice sprite with border = (0, thickness, 0, thickness) for horizontal-only borders.
        /// </summary>
        public static Texture2D MakeHorizontalBordersTexture(Color fill, Color border, int thickness)
        {
            var height = 2 * thickness + 1;
            var tex = new Texture2D(1, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            {
                var isBorder = y < thickness || y >= height - thickness;
                tex.SetPixel(0, y, isBorder ? border : fill);
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}
