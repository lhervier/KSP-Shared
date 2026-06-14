using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;

namespace com.github.lhervier.ksp.shared.ugui.combo.itemcontent.label
{
    /// <summary>
    /// Default BaseComboItemContentBuilder: renders an option as a plain text label showing its display
    /// text. This reproduces the historical combo item appearance and is used when the caller does not
    /// provide a custom content builder.
    /// </summary>
    public class LabelComboItemContentBuilder : BaseComboItemContentBuilder
    {
        // Label resolver owned by this builder (its own dependency, injected at construction), not a
        // parameter of the generic Build contract. Null → the raw id is shown.
        private Func<string, string> _labelFor;
        public LabelComboItemContentBuilder LabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        public override ComboItemContentController Build()
        {
            var labelGo = new GameObject("Label", typeof(RectTransform));
            var tmp = UGUILabels.AddLabel(labelGo);
            tmp.text = _labelFor != null ? _labelFor(GetId()) : GetId();
            tmp.fontSize = ComboPalette.ComboFontSize;
            tmp.alignment = TextAlignmentOptions.Left;

            // The default content carries the combo's standard single-line row height.
            var le = labelGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = ComboPalette.Height;

            return labelGo
                .AddComponent<LabelComboItemContentController>()
                .Label(tmp);
        }
    }
}
