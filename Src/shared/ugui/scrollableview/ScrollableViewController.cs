using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.scrollableview
{
    /// <summary>
    /// A vertically scrollable view. The content (built by the builder's content builder) lives in a
    /// clipped viewport with a right-side scrollbar.
    ///
    /// The scroll offset is owned by this controller instead of being left to the ScrollRect alone, so
    /// it survives a rebuild of the content (children destroyed and recreated). Making it survive the
    /// view itself is up to the owner: memorize <see cref="OnScrollOffsetChanged"/> and hand the value
    /// back to the builder.
    /// </summary>
    public class ScrollableViewController : MonoBehaviour
    {
        /// <summary>
        /// Fired whenever the scroll offset changes, carrying the new one. Fires as often as once per
        /// frame while the user is scrolling: the handler must stay cheap (no I/O).
        /// </summary>
        public readonly EventData<float> OnScrollOffsetChanged = new EventData<float>("ScrollableViewController.OnScrollOffsetChanged");

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

        /// <summary>
        /// Wanted distance, in pixels, between the top of the content and the top of the viewport. This
        /// is the reference the view is brought back to; the position actually applied is this value
        /// clamped to what the content can currently honour.
        /// </summary>
        private float _offset;
        public ScrollableViewController WithInitialScrollOffset(float scrollOffset)
        {
            this._offset = scrollOffset;
            return this;
        }

        /// <summary>
        /// How far the view could be scrolled down on the previous frame. NaN until the first frame
        /// measures it (NaN never compares equal, so the first frame always counts as a change and
        /// applies the offset).
        /// </summary>
        private float _lastMaxOffset = float.NaN;

        public void LateUpdate()
        {
            RectTransform content = _scrollRect == null ? null : _scrollRect.content;
            if (content == null)
            {
                return;
            }

            // A changed travel means the content was rebuilt (or is still being laid out). The ScrollRect
            // clamps the position against the new bounds — from the top, which is what wipes the scroll
            // on every rebuild — so we re-assert our own offset instead of adopting the clamped one.
            float maxOffset = GetMaxOffset();
            if (maxOffset != _lastMaxOffset)
            {
                _lastMaxOffset = maxOffset;
                ApplyOffset();
                return;
            }

            // Settled view: the position can only have moved because the user scrolled, so it becomes the
            // new reference. Except when the content is too short to honour the offset and pins the
            // position at the end of its travel: that is the clamp talking, not the user. Keeping the
            // wanted offset there is what lets a filtered-down (or not yet laid out) list scroll back to
            // where it was once it grows again.
            if (content.anchoredPosition.y >= maxOffset && _offset > maxOffset)
            {
                return;
            }
            Memorize(content.anchoredPosition.y);
        }

        // ================================
        // Public API
        // ================================

        /// <summary>
        /// The current scroll offset, in pixels from the top of the content. Same value as the one
        /// carried by <see cref="OnScrollOffsetChanged"/>, for an owner that would rather read it than
        /// follow it.
        /// </summary>
        public float ScrollOffset => _offset;

        /// <summary>
        /// Return the content controller as a Monobehaviour, as Unity
        /// does not support Generic on Monobehaviour...
        /// </summary>
        /// <returns>The parent controller</returns>
        public MonoBehaviour GetContentController()
        {
            return this._contentController;
        }

        /// <summary>Scroll back to the top of the content, and stay there until the user scrolls away.</summary>
        public void ScrollToTop()
        {
            Memorize(0f);
            ApplyOffset();
        }

        // =======================================
        // Internal helpers
        // =======================================

        /// <summary>Take the given offset as the new reference, and notify.</summary>
        /// <param name="offset">The new wanted offset, in pixels from the top of the content</param>
        private void Memorize(float offset)
        {
            if (offset == _offset)
            {
                return;
            }
            _offset = offset;
            OnScrollOffsetChanged.Fire(offset);
        }

        /// <summary>Bring the view back to the wanted offset, as far as the current content allows.</summary>
        private void ApplyOffset()
        {
            RectTransform content = _scrollRect == null ? null : _scrollRect.content;
            if (content == null)
            {
                return;
            }
            // Clamp the applied position only : _offset keeps the wanted value, so a content that is
            // momentarily too short does not truncate it.
            float position = Mathf.Clamp(_offset, 0f, GetMaxOffset());
            if (content.anchoredPosition.y == position)
            {
                return;
            }
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, position);
            // Leftover inertia from a fling would drag the view away from the position we just set.
            _scrollRect.StopMovement();
        }

        /// <summary>How far the content can be scrolled down, in pixels (0 when it does not overflow).</summary>
        private float GetMaxOffset()
        {
            RectTransform content = _scrollRect == null ? null : _scrollRect.content;
            RectTransform viewport = _scrollRect == null ? null : _scrollRect.viewport;
            if (content == null || viewport == null)
            {
                return 0f;
            }
            return Mathf.Max(0f, content.rect.height - viewport.rect.height);
        }
    }
}
