using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>Applies ksp_cheatsheet mockup colors to a KSP PopupDialog shell.</summary>
    internal static class SpritesGlobal
    {
        // Cache des sprites 9-slice, indexé par leurs paramètres. Le nombre de combinaisons distinctes
        // est petit (couleurs de la palette) : le cache plafonne donc à quelques entrées.
        private static readonly Dictionary<string, Sprite> _borderCache = new Dictionary<string, Sprite>();

        private static string BorderKey(string kind, Color fill, Color border, int thickness)
        {
            return $"{kind}|{fill}|{border}|{thickness}";
        }
        
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

        // Kept as an alias for callers that predate the cache. Delegates to Border so every
        // fill/border/thickness combination is generated once and shared, instead of a fresh
        // (uncached) Texture2D per call.
        public static Sprite MakeChipSprite(Color fill, Color border, int thickness)
        {
            return Border(fill, border, thickness);
        }

        public static Sprite Border(Color fill, Color border, int thickness)
        {
            string key = BorderKey("border", fill, border, thickness);
            if (_borderCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

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
            _borderCache[key] = sprite;
            return sprite;
        }

        /// <summary>Sprite 9-slice : fond + bordure en haut et en bas uniquement (séparateurs horizontaux).</summary>
        public static Sprite HorizontalBorders(Color fill, Color border, int thickness)
        {
            string key = BorderKey("hborder", fill, border, thickness);
            if (_borderCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            int height = 2 * thickness + 1;
            var tex = TextureUtils.MakeHorizontalBordersTexture(fill, border, thickness);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, 1f, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect,
                // (left, bottom, right, top) — pas de bordure horizontale, thickness en haut + bas
                new Vector4(0f, thickness, 0f, thickness));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _borderCache[key] = sprite;
            return sprite;
        }
    }
}
