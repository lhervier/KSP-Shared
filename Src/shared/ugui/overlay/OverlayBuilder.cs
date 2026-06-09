using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.overlay
{
    /// <summary>
    /// An invisible, full-screen click trap. A click anywhere on it fires the controller's OnClose
    /// event — used to dismiss a transient surface (dropdown, menu…) when the user clicks away.
    /// </summary>
    public class OverlayBuilder : IUGUIBuilder<OverlayController>
    {
        public OverlayController Build()
        {
            var overlayGo = new GameObject("Overlay", typeof(RectTransform));

            // A parent VerticalLayoutGroup would otherwise place us in its flow.
            // Tell it to ignore us so our anchors take effect.
            var layoutElement = overlayGo.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // Centered on the parent, oversized so it covers anything around the screen
            var rect = overlayGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(3000f, 3000f);
            rect.anchoredPosition = Vector2.zero;

            // Fully transparent but raycastTarget=true: invisible click trap
            var image = overlayGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            // A click anywhere on the overlay fires OnClose. Disable color transitions so the
            // overlay stays invisible during hover/press states.
            var button = overlayGo.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0f;
            button.colors = colors;

            return overlayGo
                .AddComponent<OverlayController>()
                .Overlay(button);
        }
    }
}
