using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui
{
    public static class UGUILabels
    {
        /// <summary>
        /// Adds a TextMeshProUGUI to the given object, preconfigured with the plugin defaults:
        /// UI font, sprite asset (inline &lt;sprite&gt; tags), no raycast, single line that
        /// overflows instead of wrapping. Callers override what they need afterwards
        /// (text, size, color, alignment, wrapping...).
        /// </summary>
        public static TextMeshProUGUI AddLabel(GameObject go)
        {
            var label = go.AddComponent<TextMeshProUGUI>();
            label.font = DefaultPalette.Font;
            label.spriteAsset = SpritesIcons.SpriteAsset;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }
    }
}
