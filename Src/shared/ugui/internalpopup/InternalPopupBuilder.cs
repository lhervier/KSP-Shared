using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.internalpopup
{
    /// <summary>
    /// Builds a modal "internal popup": a dimmed layer filling the parent, with a centered card holding,
    /// top to bottom, a title, a content subtree and a footer subtree. The content and footer come from
    /// the supplied builders. The returned <see cref="InternalPopupController"/> shows/closes it (it
    /// starts closed). The built content/footer controllers are exposed as <see cref="ContentController"/>
    /// / <see cref="FooterController"/> so the owner can wire them to its model after Build().
    /// </summary>
    public class InternalPopupBuilder<TContent, TFooter> : IUGUIBuilder<InternalPopupController>
        where TContent : MonoBehaviour
        where TFooter : MonoBehaviour
    {
        // ===============================================
        // Build parameters
        // ===============================================

        private Transform _parent;
        public InternalPopupBuilder<TContent, TFooter> Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _title = string.Empty;
        public InternalPopupBuilder<TContent, TFooter> Title(string title)
        {
            this._title = title;
            return this;
        }

        private Color _titleColor = Color.white;
        public InternalPopupBuilder<TContent, TFooter> TitleColor(Color titleColor)
        {
            this._titleColor = titleColor;
            return this;
        }

        private IUGUIBuilder<TContent> _contentBuilder;
        public InternalPopupBuilder<TContent, TFooter> Content(IUGUIBuilder<TContent> contentBuilder)
        {
            this._contentBuilder = contentBuilder;
            return this;
        }

        private IUGUIBuilder<TFooter> _footerBuilder;
        public InternalPopupBuilder<TContent, TFooter> Footer(IUGUIBuilder<TFooter> footerBuilder)
        {
            this._footerBuilder = footerBuilder;
            return this;
        }

        // ===============================================
        // Built controllers (available after Build)
        // ===============================================

        /// <summary>The content controller built from the content builder (set by Build).</summary>
        public TContent ContentController { get; private set; }

        /// <summary>The footer controller built from the footer builder (set by Build).</summary>
        public TFooter FooterController { get; private set; }

        // ===============================================
        // Build
        // ===============================================

        public InternalPopupController Build()
        {
            // Root: always active (so the controller's lifecycle runs and the owner can drive it from the
            // start), invisible, fills the parent interior minus its 1px chrome border, escapes any layout.
            var rootGo = new GameObject("InternalPopup", typeof(RectTransform));
            rootGo.transform.SetParent(_parent, false);

            var rootLe = rootGo.AddComponent<LayoutElement>();
            rootLe.ignoreLayout = true;

            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(InternalPopupPalette.BorderThickness, InternalPopupPalette.BorderThickness);
            rootRect.offsetMax = new Vector2(-InternalPopupPalette.BorderThickness, -InternalPopupPalette.BorderThickness);

            // Panel: the only part toggled by Show()/Close(). Dimmed and modal (blocks clicks behind it).
            var panelGo = new GameObject("Panel", typeof(RectTransform));
            panelGo.transform.SetParent(rootGo.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var dim = panelGo.AddComponent<Image>();
            dim.sprite = SpritesGlobal.FillSprite;
            dim.type = Image.Type.Simple;
            dim.color = InternalPopupPalette.DimColor;
            dim.raycastTarget = true;

            // Card: centered, fixed width, height fitted to its content.
            var cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(panelGo.transform, false);
            var cardRect = cardGo.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(InternalPopupPalette.CardWidth, 0f);

            var cardImage = cardGo.AddComponent<Image>();
            cardImage.sprite = SpritesGlobal.MakeChipSprite(
                InternalPopupPalette.CardBackgroundColor,
                InternalPopupPalette.CardBorderColor,
                InternalPopupPalette.CardBorderThickness);
            cardImage.type = Image.Type.Sliced;
            cardImage.color = Color.white;
            cardImage.raycastTarget = true;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            int pad = Mathf.RoundToInt(InternalPopupPalette.CardPadding);
            cardLayout.padding = new RectOffset(pad, pad, pad, pad);
            cardLayout.spacing = InternalPopupPalette.CardSpacing;
            cardLayout.childAlignment = TextAnchor.UpperLeft;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var cardFitter = cardGo.AddComponent<ContentSizeFitter>();
            cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title (color supplied by the caller), then the content and footer subtrees beneath it.
            CreateTitle(cardGo.transform);

            this.ContentController = _contentBuilder.Build();
            this.ContentController.transform.SetParent(cardGo.transform, false);

            this.FooterController = _footerBuilder.Build();
            this.FooterController.transform.SetParent(cardGo.transform, false);

            // Starts closed: the root stays active, only the panel is hidden.
            panelGo.SetActive(false);

            return rootGo
                .AddComponent<InternalPopupController>()
                .Panel(panelGo);
        }

        private void CreateTitle(Transform card)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(card, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = _title;
            label.font = DefaultPalette.Font;
            label.fontSize = InternalPopupPalette.TitleFontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = _titleColor;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
        }
    }
}
