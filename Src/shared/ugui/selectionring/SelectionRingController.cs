using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.selectionring
{
    /// <summary>
    /// Runtime handle over a selection ring built by <see cref="SelectionRingBuilder"/>. Lets the row
    /// controller show or hide it as the row becomes the selected one, and recolor it.
    /// </summary>
    public class SelectionRingController : MonoBehaviour
    {
        // =====================================================
        // Wiring (set by the builder)
        // =====================================================

        private Image _ring;
        public SelectionRingController WithRingComponent(Image ring)
        {
            this._ring = ring;
            return this;
        }

        // =====================================================
        // Public API
        // =====================================================

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetColor(Color color)
        {
            if (_ring != null) _ring.color = color;
        }
    }
}
