using UnityEngine;
using com.github.lhervier.ksp.steaminput.ui.styles;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>Icon sprites shared by several screens of the plugin UI.</summary>
    internal static class SpritesIcons
    {
        private static Sprite _refreshIconSprite;
        public static Sprite RefreshIconSprite
        {
            get
            {
                if (_refreshIconSprite != null)
                {
                    return _refreshIconSprite;
                }

                if (GameDatabase.Instance == null)
                {
                    return null;
                }

                var tex = GameDatabase.Instance.GetTexture(
                    DefaultPalette.RefreshIconPath,
                    false
                );
                if (tex == null)
                {
                    return null;
                }

                _refreshIconSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                _refreshIconSprite.hideFlags = HideFlags.HideAndDontSave;
                return _refreshIconSprite;
            }
        }
    }
}
