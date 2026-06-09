using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.scrollableview
{
    /// <summary>
    /// Builds a vertically scrollable view: a clipped viewport with a right-side scrollbar, and a content
    /// container that grows to fit its children. The content is produced by the supplied content builder
    /// (see <see cref="ContentBuilder"/>). The view fills the rect it is given — callers own its placement.
    /// </summary>
    public class ScrollableViewBuilder<C> : IUGUIBuilder<ScrollableViewController> where C : MonoBehaviour
    {
        private string _objectName = "ScrollableView";
        private float _scrollbarWidth = ScrollableViewPalette.ScrollbarWidth;
        private float _scrollSensitivity = ScrollableViewPalette.ScrollbarSensitivity;
        private float _contentSpacing = 0f;
        private RectOffset _contentPadding = new RectOffset(0, 0, 0, 0);

        private Color _scrollbarBackgroundColor = ScrollableViewPalette.ScrollbarBackgroundColor;
        private Color _handleColor = ScrollableViewPalette.ScrollbarHandleColor;
        private Color _handleHoverColor = ScrollableViewPalette.ScrollbarHoverColor;

        private IUGUIBuilder<C> _contentBuilder;

        // ===========================================================
        // Builder parameters
        // ===========================================================

        /// <summary>The builder producing the scrolled content. Its result is mounted in the viewport.</summary>
        public ScrollableViewBuilder<C> ContentBuilder(IUGUIBuilder<C> contentBuilder)
        {
            this._contentBuilder = contentBuilder;
            return this;
        }

        public ScrollableViewBuilder<C> ObjectName(string objectName)
        {
            this._objectName = objectName;
            return this;
        }

        public ScrollableViewBuilder<C> ScrollbarWidth(float width)
        {
            this._scrollbarWidth = width;
            return this;
        }

        public ScrollableViewBuilder<C> ScrollSensitivity(float sensitivity)
        {
            this._scrollSensitivity = sensitivity;
            return this;
        }

        public ScrollableViewBuilder<C> ContentSpacing(float spacing)
        {
            this._contentSpacing = spacing;
            return this;
        }

        public ScrollableViewBuilder<C> ContentPadding(RectOffset padding)
        {
            this._contentPadding = padding;
            return this;
        }

        public ScrollableViewBuilder<C> ScrollbarBackgroundColor(Color color)
        {
            this._scrollbarBackgroundColor = color;
            return this;
        }

        public ScrollableViewBuilder<C> HandleColor(Color color)
        {
            this._handleColor = color;
            return this;
        }

        public ScrollableViewBuilder<C> HandleHoverColor(Color color)
        {
            this._handleHoverColor = color;
            return this;
        }

        // ===========================================================
        // Build
        // ===========================================================

        public ScrollableViewController Build()
        {
            var rootGo = new GameObject(_objectName, typeof(RectTransform));
            
            // ScrollRect drives the scrolling. It links viewport (clip) + content (scrolled) + scrollbar.
            var scrollRect = rootGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = _scrollSensitivity;

            // Viewport: full view minus the scrollbar column on the right. RectMask2D clips overflow.
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(rootGo.transform, false);

            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(-_scrollbarWidth, 0f);
            viewportGo.AddComponent<RectMask2D>();

            // Image with raycastTarget=true so mouse-wheel scroll has a target
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.sprite = SpritesGlobal.FillSprite;
            viewportImage.type = Image.Type.Simple;
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
            scrollRect.viewport = viewportRect;

            // Content: child of the viewport, anchored to its top edge. Height is auto-managed by the
            // ContentSizeFitter below, based on the sum of children's preferred heights.
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);

            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            scrollRect.content = contentRect;

            // VLG stacks the children top to bottom
            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = _contentPadding;
            contentLayout.spacing = _contentSpacing;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            // Auto-grow Content height to fit its children — drives the scrollbar's behavior
            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Mount the caller's content as the scrolled child. Its preferred height drives the scrollbar.
            C contentController = null;
            if (_contentBuilder != null)
            {
                contentController = _contentBuilder.Build();
                contentController.transform.SetParent(contentGo.transform, false);
            }

            // Scrollbar: vertical bar pinned to the right of the view, full height.
            var scrollbarGo = new GameObject("Scrollbar", typeof(RectTransform));
            scrollbarGo.transform.SetParent(rootGo.transform, false);

            var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(_scrollbarWidth, 0f);

            var scrollbarBg = scrollbarGo.AddComponent<Image>();
            scrollbarBg.sprite = SpritesGlobal.FillSprite;
            scrollbarBg.type = Image.Type.Simple;
            scrollbarBg.color = _scrollbarBackgroundColor;
            scrollbarBg.raycastTarget = true;

            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            // Sliding area: where the handle slides. Anchored to fill the scrollbar.
            var slidingAreaGo = new GameObject("Sliding Area", typeof(RectTransform));
            slidingAreaGo.transform.SetParent(scrollbarGo.transform, false);

            var slidingAreaRect = slidingAreaGo.GetComponent<RectTransform>();
            slidingAreaRect.anchorMin = Vector2.zero;
            slidingAreaRect.anchorMax = Vector2.one;
            slidingAreaRect.offsetMin = Vector2.zero;
            slidingAreaRect.offsetMax = Vector2.zero;

            // Handle: the draggable part.
            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(slidingAreaGo.transform, false);

            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;

            var handleImage = handleGo.AddComponent<Image>();
            handleImage.sprite = SpritesGlobal.FillSprite;
            handleImage.type = Image.Type.Simple;
            // White so the Scrollbar's ColorBlock controls the tint without multiplication
            handleImage.color = Color.white;
            handleImage.raycastTarget = true;

            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;

            // Visible handle on the scrollbar background, with subtle hover/press feedback
            var scrollbarColors = scrollbar.colors;
            scrollbarColors.normalColor = _handleColor;
            scrollbarColors.highlightedColor = _handleHoverColor;
            scrollbarColors.pressedColor = _handleHoverColor;
            scrollbarColors.selectedColor = _handleColor;
            scrollbarColors.disabledColor = _handleColor;
            scrollbarColors.colorMultiplier = 1f;
            scrollbarColors.fadeDuration = 0.1f;
            scrollbar.colors = scrollbarColors;

            scrollRect.verticalScrollbar = scrollbar;

            return rootGo
                .AddComponent<ScrollableViewController>()
                .ScrollRect(scrollRect)
                .ContentController(contentController);
        }
    }
}
