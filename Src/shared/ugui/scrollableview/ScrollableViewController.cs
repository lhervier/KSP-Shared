using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.scrollableview
{
    /// <summary>
    /// A vertically scrollable view. The content (built by the builder's content builder) lives in a
    /// clipped viewport with a right-side scrollbar.
    /// </summary>
    public class ScrollableViewController : MonoBehaviour
    {
        // ===========================================
        // Life cycle
        // ===========================================

        private MonoBehaviour _contentController;
        public ScrollableViewController WithContentController(MonoBehaviour contentController)
        {
            this._contentController = contentController;
            return this;
        }

        private ScrollRect _scrollRect;
        public ScrollableViewController WithScrollRect(ScrollRect scrollRect)
        {
            this._scrollRect = scrollRect;
            return this;
        }

        // ================================
        // Public API
        // ================================

        /// <summary>
        /// Return the content controller as a Monobehaviour, as Unity
        /// does not support Generic on Monobehaviour...
        /// </summary>
        /// <returns>The parent controller</returns>
        public MonoBehaviour GetContentController()
        {
            return this._contentController;
        }

        /// <summary>Scroll back to the top of the content.</summary>
        public void ScrollToTop()
        {
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
}
