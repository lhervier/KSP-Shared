using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>Applies ksp_cheatsheet mockup colors to a KSP PopupDialog shell.</summary>
    internal static class SpritesPopupDialog
    {
        private static Sprite _windowChromeSprite;
        public static Sprite WindowChromeSprite
        {
            get
            {
                if (_windowChromeSprite != null)
                {
                    return _windowChromeSprite;
                }

                var thickness = (int) PopupPalette.PopupBorderThickness;
                var size = 2 * thickness + 1;
                var tex = TextureUtils.MakeBorderTexture(
                    PopupPalette.PopupBodyColor,
                    PopupPalette.PopupBorderColor,
                    thickness
                );
                _windowChromeSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0u,
                    SpriteMeshType.FullRect,
                    new Vector4(thickness, thickness, thickness, thickness));
                _windowChromeSprite.hideFlags = HideFlags.HideAndDontSave;
                return _windowChromeSprite;
            }
        }
    }
}
