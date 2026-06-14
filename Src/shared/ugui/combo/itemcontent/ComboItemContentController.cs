using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.combo.itemcontent
{
    /// <summary>
    /// Base contract for the visual content displayed inside a combo list item (the part the user
    /// sees, as opposed to the clickable row chrome owned by ComboItemController). A content is built
    /// by a BaseComboItemContentBuilder and lives as a child of the item row. The row reflects its
    /// selection state on the content through SetSelected, leaving the content free to style itself.
    /// </summary>
    public abstract class ComboItemContentController : MonoBehaviour
    {
        /// <summary>
        /// Reflects the item's selection state on this content (e.g. recolor a label). Called by the
        /// owning row whenever the selection changes, including once at startup. Default: no-op, so a
        /// content that does not change appearance on selection has nothing to override.
        /// </summary>
        public virtual void SetSelected(bool selected) { }
    }
}
