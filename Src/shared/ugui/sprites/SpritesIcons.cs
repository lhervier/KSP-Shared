using System.Collections.Generic;
using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.sprites
{
    /// <summary>Icon sprites shared by several screens of the plugin UI.</summary>
    internal static class SpritesIcons
    {
        private static readonly ModLogger LOGGER = new ModLogger("Sprites");

        // =======================================================
        // TMP sprite asset (inline <sprite> tags in labels)
        // =======================================================

        private class SpriteDef
        {
            public string Name;
            public string TexturePath;
            public int Unicode;
            public float Scale;
        }

        // Sprites available to every TMP label of the plugin UI. The shared module declares
        // its own icons here; the mod registers additional entries (controller glyphs, ...)
        // through RegisterSprite at startup.
        private static readonly List<SpriteDef> _spriteDefs = new List<SpriteDef>
        {
            new SpriteDef { Name = "refresh", TexturePath = DefaultPalette.RefreshIconPath, Unicode = 0x21BB, Scale = 1f }, // ↻
        };

        /// <summary>
        /// Declares a sprite, made available in every plugin label that uses
        /// <see cref="SpriteAsset"/>: through a &lt;sprite name="..."&gt; rich-text tag, and as a
        /// fallback for the given unicode codepoint when the fonts cannot render it. Must be
        /// called before the UI is built. <paramref name="scale"/> adjusts the rendered size
        /// relative to the font ascender height (1 = exactly ascender-high).
        /// </summary>
        public static void RegisterSprite(string name, string texturePath, int unicode = 0, float scale = 1f)
        {
            _spriteDefs.Add(new SpriteDef { Name = name, TexturePath = texturePath, Unicode = unicode, Scale = scale });
            _spriteAsset = null;
        }

        private static TMP_SpriteAsset _spriteAsset;

        /// <summary>
        /// Sprite asset to assign to TMP labels (<see cref="TMP_Text.spriteAsset"/>) so the
        /// registered sprites resolve. Null (without caching) while the game database is not
        /// loaded yet.
        /// </summary>
        public static TMP_SpriteAsset SpriteAsset
        {
            get
            {
                if (_spriteAsset != null)
                {
                    return _spriteAsset;
                }

                if (GameDatabase.Instance == null)
                {
                    return null;
                }

                // One single-sprite asset per texture, chained through fallbackSpriteAssets:
                // a TMP_SpriteAsset holds a single sprite sheet texture, and GameDatabase
                // textures are not always CPU-readable, so they cannot be packed into a
                // runtime atlas. Both the <sprite name=...> tag resolution and the
                // missing-character lookup walk the fallback chain.
                TMP_SpriteAsset root = null;
                foreach (SpriteDef def in _spriteDefs)
                {
                    TMP_SpriteAsset asset = BuildSingleSpriteAsset(def);
                    if (asset == null)
                    {
                        continue;
                    }
                    if (root == null)
                    {
                        root = asset;
                    }
                    else
                    {
                        root.fallbackSpriteAssets.Add(asset);
                    }
                }

                _spriteAsset = root;
                return _spriteAsset;
            }
        }

        /// <summary>
        /// Tells whether the given sprite name resolves through <see cref="SpriteAsset"/>
        /// (fallbacks included), i.e. whether a &lt;sprite name="..."&gt; tag would render.
        /// </summary>
        public static bool HasSprite(string name)
        {
            TMP_SpriteAsset asset = SpriteAsset;
            if (asset == null)
            {
                return false;
            }
            int spriteIndex;
            return TMP_SpriteAsset.SearchForSpriteByHashCode(
                asset,
                TMP_TextUtilities.GetSimpleHashCode(name),
                true,
                out spriteIndex
            ) != null;
        }

        /// <summary>
        /// Builds a sprite asset holding a single sprite covering the whole texture, or null
        /// when the texture cannot be loaded.
        /// </summary>
        private static TMP_SpriteAsset BuildSingleSpriteAsset(SpriteDef def)
        {
            var tex = GameDatabase.Instance.GetTexture(def.TexturePath, false);
            if (tex == null)
            {
                LOGGER.LogWarning("Sprite texture not found: " + def.TexturePath);
                return null;
            }

            var asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = def.Name;
            asset.hideFlags = HideFlags.HideAndDontSave;
            asset.spriteSheet = tex;
            asset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

            ShaderUtilities.GetShaderPropertyIDs();
            asset.material = new Material(Shader.Find("TextMeshPro/Sprite"));
            asset.material.SetTexture(ShaderUtilities.ID_MainTex, tex);
            asset.material.hideFlags = HideFlags.HideAndDontSave;

            asset.spriteInfoList = new List<TMP_Sprite>
            {
                new TMP_Sprite
                {
                    id = 0,
                    name = def.Name,
                    // Same hash as the one computed by the rich-text tag parser for the
                    // "name" attribute value, so <sprite name=...> lookups match.
                    hashCode = TMP_TextUtilities.GetSimpleHashCode(def.Name),
                    unicode = def.Unicode,
                    x = 0f,
                    y = 0f,
                    width = tex.width,
                    height = tex.height,
                    pivot = new Vector2(0.5f, 0.5f),
                    xOffset = 0f,
                    // Baseline-anchored: TMP renders the sprite at (ascender * scale) high,
                    // and yOffset = height puts its top at the ascender line.
                    yOffset = tex.height,
                    xAdvance = tex.width,
                    scale = def.Scale,
                }
            };
            asset.UpdateLookupTables();
            return asset;
        }
    }
}
