using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using static com.github.lhervier.ksp.shared.ugui.styles.Utils;

namespace com.github.lhervier.ksp.shared.ugui.styles
{
    public static class DefaultPalette
    {
        private static readonly ModLogger LOGGER = new ModLogger("Fonts");

        // =======================================================
        // Custom font
        // =======================================================

        // Font used by every text element of the plugin UI.
        // Resolved lazily because UISkinManager is only populated once the game UI is loaded.
        private static TMP_FontAsset _font;
        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = BuildFont();
                }
                return _font;
            }
        }

        /// <summary>
        /// Builds the plugin font: the game's UI font, extended with every other loaded font
        /// asset as fallback. Returns null (without caching) while the game UI is not loaded yet.
        /// </summary>
        private static TMP_FontAsset BuildFont()
        {
            TMP_FontAsset baseFont = UISkinManager.TMPFont;
            if (baseFont == null)
            {
                return null;
            }

            // The game font's atlas is static (SDF): it only contains the characters baked by the
            // game, unlike the legacy dynamic font that rasterized any glyph on demand. Missing
            // glyphs (arrows, carets, ...) are resolved through TMP's fallback chain instead, by
            // exposing every other font asset shipped with the game (the CJK ones in particular
            // cover most symbols). The game's asset is cloned so it stays untouched for stock UI.
            var font = Object.Instantiate(baseFont);
            font.name = baseFont.name + " (" + Constants.ModName + ")";
            font.fallbackFontAssets = new List<TMP_FontAsset>();
            if (baseFont.fallbackFontAssets != null)
            {
                font.fallbackFontAssets.AddRange(baseFont.fallbackFontAssets);
            }
            foreach (TMP_FontAsset candidate in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (candidate == baseFont || candidate == font || font.fallbackFontAssets.Contains(candidate))
                {
                    continue;
                }
                font.fallbackFontAssets.Add(candidate);
            }

            var names = new StringBuilder();
            foreach (TMP_FontAsset fallback in font.fallbackFontAssets)
            {
                names.Append(names.Length == 0 ? "" : ", ").Append(fallback.name);
            }
            LOGGER.LogInfo("UI font: " + baseFont.name + ", fallbacks: [" + names + "]");

            return font;
        }

        /// <summary>
        /// Returns the first candidate whose characters are all renderable with <see cref="Font"/>
        /// (fallbacks included). The last candidate is returned when none matches, so it should be
        /// the safest one (plain ASCII or an already proven glyph).
        /// </summary>
        public static string PickGlyph(params string[] candidates)
        {
            TMP_FontAsset font = Font;
            if (font != null)
            {
                for (int i = 0; i < candidates.Length - 1; i++)
                {
                    bool available = true;
                    foreach (char c in candidates[i])
                    {
                        if (!font.HasCharacter(c, true))
                        {
                            available = false;
                            break;
                        }
                    }
                    if (available)
                    {
                        return candidates[i];
                    }
                }
            }
            return candidates[candidates.Length - 1];
        }

        // ===============================================================
        // Shared icons
        // ===============================================================

        // Circular refresh arrow, used wherever the "↻" glyph would be (missing from the game fonts).
        public static string RefreshIconPath = Constants.ModName + "/Textures/refresh_icon";

        // ==============================================================
        // Default values
        // ==============================================================
        public const float PaddingLeft = 8f;
        public const float PaddingRight = 8f;
        public const float PaddingTop = 5f;
        public const float PaddingBottom = 5f;
        public const float Spacing = 6f;

        // Default colors
        public static readonly Color AccentColor = Rgb(141, 190, 69);
        public static readonly Color AccentBorderColor = Rgb(74, 110, 32);
        public static readonly Color AccentBgColor = Rgba(141, 190, 69, 0.10f);
        public static readonly Color LabelColor = Rgb(187, 187, 187);
        public static readonly Color FieldBackgroundColor = new Color(42f / 255f, 42f / 255f, 42f / 255f);
        public static readonly Color SeparatorColor = new Color(42f / 255f, 42f / 255f, 42f / 255f);

        public static readonly Color DangerColor = Rgb(192, 89, 79);
        public static readonly Color WarmColor = Rgb(169, 138, 74);

        // Icons
        public const int IconSize = 18;
    }
}
