using System;
using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// A popin whose content is a caller-supplied builder and whose footer is a declared set of buttons
    /// (label + action + alert flag). Specializes <see cref="PopinBuilder{TContent,TFooter}"/> by owning
    /// the footer itself (a <see cref="PopinButtonBarController"/>). The content controller is exposed as
    /// <see cref="ContentController"/> after Build() so the owner can wire it to its model.
    /// </summary>
    public class ButtonBarPopinBuilder<TContent> : IUGUIBuilder<PopinController>
        where TContent : MonoBehaviour
    {
        private readonly PopinButtonBarBuilder _footerBuilder = new PopinButtonBarBuilder();

        private Transform _parent;
        public ButtonBarPopinBuilder<TContent> WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _title = string.Empty;
        public ButtonBarPopinBuilder<TContent> WithTitle(string title)
        {
            this._title = title;
            return this;
        }

        private Color _titleColor = Color.white;
        public ButtonBarPopinBuilder<TContent> WithTitleColor(Color titleColor)
        {
            this._titleColor = titleColor;
            return this;
        }

        private IUGUIBuilder<TContent> _contentBuilder;
        public ButtonBarPopinBuilder<TContent> WithContentBuilder(IUGUIBuilder<TContent> contentBuilder)
        {
            this._contentBuilder = contentBuilder;
            return this;
        }

        /// <summary>Append a footer button styled according to <paramref name="style"/> (defaults to neutral).</summary>
        public ButtonBarPopinBuilder<TContent> WithButton(string label, Action action, PopinButtonStyle style = PopinButtonStyle.Normal)
        {
            _footerBuilder.WithButton(label, action, style);
            return this;
        }

        /// <summary>The content controller built from the content builder (set by Build).</summary>
        public TContent ContentController { get; private set; }

        public PopinController Build()
        {
            var popinBuilder = new PopinBuilder<TContent, PopinButtonBarController>()
                .WithParent(_parent)
                .WithTitle(_title)
                .WithTitleColor(_titleColor)
                .WithContentBuilder(_contentBuilder)
                .WithFooterBuilder(_footerBuilder);

            PopinController popin = popinBuilder.Build();
            this.ContentController = popinBuilder.ContentController;
            return popin;
        }
    }
}
