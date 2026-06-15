using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.combo.itemcontent
{
    /// <summary>
    /// Builds the visual content of a combo list item. Implementations decide freely what an option
    /// looks like in the dropdown (a plain label, an icon + text, anything). The combo handles the
    /// clickable row and the selection background itself; an implementation only produces the inner
    /// content and, optionally, styles it on selection via the returned ComboItemContentController.
    /// </summary>
    public abstract class BaseComboItemContentBuilder : IUGUIBuilder<ComboItemContentController>
    {
        private string _id;
        public BaseComboItemContentBuilder WithId(string id)
        {
            this._id = id;
            return this;
        }

        /// <summary>
        /// Builds the content for one option.
        /// The raw option value is accessible via the GetId() method.
        /// </summary>
        public abstract ComboItemContentController Build();

        public string GetId()
        {
            return this._id;
        }
    }
}
