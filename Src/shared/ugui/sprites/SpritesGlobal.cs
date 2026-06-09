using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>Applies ksp_cheatsheet mockup colors to a KSP PopupDialog shell.</summary>
    internal static class SpritesGlobal
    {
        private static Sprite _fillSprite;
        public static Sprite FillSprite
        {
            get
            {
                if (_fillSprite != null)
                {
                    return _fillSprite;
                }

                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                tex.hideFlags = HideFlags.HideAndDontSave;
                _fillSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
                _fillSprite.hideFlags = HideFlags.HideAndDontSave;
                return _fillSprite;
            }
        }

        public static Sprite MakeChipSprite(Color fill, Color border, int thickness)
        {
            int size = 2 * thickness + 1;
            var tex = TextureUtils.MakeBorderTexture(fill, border, thickness);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect,
                new Vector4(thickness, thickness, thickness, thickness));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
