using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.selectionring
{
    /// <summary>
    /// Builds the outline marking the selected row of a list: a sliced border overlaid on the whole
    /// row, out of layout and transparent to the pointer. Returns a <see cref="SelectionRingController"/>
    /// so the row controller can show and hide it from its selected state.
    ///
    /// A row usually already shows a state of its own (the active vessel, the controlling module, a
    /// missing target...) through its background and its left accent bar. Painting the selection with
    /// those same two channels makes it invisible on any row that already carries a state: both end up
    /// with the very same pixels. The ring is a channel of its own, laid over the state instead of
    /// competing with it — hence its neutral color, never the accent.
    ///
    /// Fluent builder in the style of <c>BadgeBuilder</c>: chain <c>WithXxx(...)</c> then call
    /// <c>Build()</c>. Build the ring once the row content is built: it has to be the last child of
    /// the row to be drawn above it.
    /// </summary>
    public class SelectionRingBuilder : IUGUIBuilder<SelectionRingController>
    {
        // ===========================================================
        // Builder parameters
        // ===========================================================

        private string _objectName = "SelectionRing";
        public SelectionRingBuilder WithObjectName(string objectName)
        {
            this._objectName = objectName;
            return this;
        }

        private Transform _parent;
        public SelectionRingBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private Color _color = SelectionRingPalette.RingColor;
        public SelectionRingBuilder WithColor(Color color)
        {
            this._color = color;
            return this;
        }

        private int _thickness = SelectionRingPalette.RingThickness;
        public SelectionRingBuilder WithThickness(int thickness)
        {
            this._thickness = thickness;
            return this;
        }

        private bool _visible = false;
        public SelectionRingBuilder WithVisible(bool visible)
        {
            this._visible = visible;
            return this;
        }

        // ===========================================================
        // Build the ring
        // ===========================================================

        public SelectionRingController Build()
        {
            var ringGo = new GameObject(_objectName, typeof(RectTransform));
            if (_parent != null)
            {
                ringGo.transform.SetParent(_parent, false);
                ringGo.transform.SetAsLastSibling();
            }

            var le = ringGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            var ring = ringGo.AddComponent<Image>();
            // White border over a transparent fill, tinted by the Image color: a single entry of the
            // sprite cache then serves every row of every list, whatever the requested color.
            ring.sprite = SpritesGlobal.Border(Color.clear, Color.white, _thickness);
            ring.type = Image.Type.Sliced;
            ring.color = _color;
            ring.raycastTarget = false;   // lets hover and click through to the row background

            // Covers the whole row (set after AddComponent, cf. the overwritten sizeDelta trap).
            var rect = ringGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var controller = ringGo.AddComponent<SelectionRingController>()
                .WithRingComponent(ring);
            controller.SetVisible(_visible);
            return controller;
        }
    }
}
