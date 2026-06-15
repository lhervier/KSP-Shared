using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;

namespace com.github.lhervier.ksp.shared.ugui.combo.itemcontent.label
{
    /// <summary>
    /// Default combo item content: a single text label. Mirrors the selection state by switching the
    /// text color between the normal and selected palette entries.
    /// </summary>
    public class LabelComboItemContentController : ComboItemContentController
    {
        private TextMeshProUGUI _label;
        public LabelComboItemContentController WithLabelComponent(TextMeshProUGUI label)
        {
            this._label = label;
            return this;
        }

        public override void SetSelected(bool selected)
        {
            if (_label == null) return;
            _label.color = selected ? ComboPalette.ItemSelectedColor : ComboPalette.ItemColor;
        }
    }
}
