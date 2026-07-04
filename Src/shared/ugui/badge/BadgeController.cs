using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.badge
{
    /// <summary>
    /// Runtime handle over a badge built by <see cref="BadgeBuilder"/>. Lets callers mutate the badge
    /// after creation: its text, its colors (to switch between states such as accent / danger), its
    /// leading icon and its visibility.
    /// </summary>
    public class BadgeController : MonoBehaviour
    {
        // =====================================================
        // Wiring (set by the builder)
        // =====================================================

        private Image _background;
        public BadgeController WithBackgroundComponent(Image background)
        {
            this._background = background;
            return this;
        }

        private TextMeshProUGUI _label;
        public BadgeController WithLabelComponent(TextMeshProUGUI label)
        {
            this._label = label;
            return this;
        }

        private Image _icon;
        public BadgeController WithIconComponent(Image icon)
        {
            this._icon = icon;
            return this;
        }

        private int _borderThickness = 1;
        public BadgeController WithBorderThickness(int thickness)
        {
            this._borderThickness = thickness;
            return this;
        }

        // =====================================================
        // Public API
        // =====================================================

        public void SetText(string text)
        {
            if (_label != null) _label.text = text;
        }

        /// <summary>
        /// Recolors the badge: the label color and the background/border sprite. The sprite is resolved
        /// through the shared cache, so recoloring back and forth between a few states is cheap.
        /// </summary>
        public void SetColors(Color textColor, Color bgColor, Color borderColor)
        {
            if (_label != null) _label.color = textColor;
            if (_background != null) _background.sprite = SpritesGlobal.Border(bgColor, borderColor, _borderThickness);
        }

        /// <summary>Text and colors in one call, for state-driven badges.</summary>
        public void SetState(string text, Color textColor, Color bgColor, Color borderColor)
        {
            SetText(text);
            SetColors(textColor, bgColor, borderColor);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Sets the leading icon sprite, hiding the icon slot when null. No-op when the badge was built
        /// without an icon slot (no initial icon was provided to the builder).
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (_icon == null) return;
            _icon.sprite = sprite;
            _icon.gameObject.SetActive(sprite != null);
        }
    }
}
