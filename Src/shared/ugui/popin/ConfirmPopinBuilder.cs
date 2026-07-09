using System;
using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// A confirmation popin: a message body and a Cancel/OK footer. Specializes
    /// <see cref="ButtonBarPopinBuilder{TContent}"/> by owning both the message content and the two buttons.
    /// The returned <see cref="ConfirmPopinController"/> shows/closes it (it starts closed) and lets the owner
    /// (re)set the message and the OK action after Build. Cancel always just closes.
    /// </summary>
    public class ConfirmPopinBuilder : IUGUIBuilder<ConfirmPopinController>
    {
        private Transform _parent;
        public ConfirmPopinBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _title = string.Empty;
        public ConfirmPopinBuilder WithTitle(string title)
        {
            this._title = title;
            return this;
        }

        private Color _titleColor = Color.white;
        public ConfirmPopinBuilder WithTitleColor(Color titleColor)
        {
            this._titleColor = titleColor;
            return this;
        }

        private string _message = string.Empty;
        public ConfirmPopinBuilder WithMessage(string message)
        {
            this._message = message;
            return this;
        }

        // null = not set: fall back to the shared "commonOk" key at Build time.
        private string _okLabel = null;
        public ConfirmPopinBuilder WithOkLabel(string okLabel)
        {
            this._okLabel = okLabel;
            return this;
        }

        private PopinButtonStyle _okStyle = PopinButtonStyle.Confirm;
        public ConfirmPopinBuilder WithOkStyle(PopinButtonStyle okStyle)
        {
            this._okStyle = okStyle;
            return this;
        }

        private Action _okAction;
        public ConfirmPopinBuilder WithOkAction(Action okAction)
        {
            this._okAction = okAction;
            return this;
        }

        // null = not set: fall back to the shared "commonCancel" key at Build time.
        private string _cancelLabel = null;
        public ConfirmPopinBuilder WithCancelLabel(string cancelLabel)
        {
            this._cancelLabel = cancelLabel;
            return this;
        }

        public ConfirmPopinController Build()
        {
            // The Cancel/OK buttons must reach the controller's *current* action, which only exists once the
            // popin root is built. The buttons capture this (still null) reference; they bind their handlers
            // at Start, by which point Build has returned and the field below is assigned.
            ConfirmPopinController controller = null;

            // Unset labels fall back to the shared common keys, resolved for the current mod
            // (ModLocalization prefixes with #LOC_<ModName>_).
            string okLabel = _okLabel ?? ModLocalization.GetString("commonOk");
            string cancelLabel = _cancelLabel ?? ModLocalization.GetString("commonCancel");

            var popinBuilder = new ButtonBarPopinBuilder<ConfirmContentController>()
                .WithParent(_parent)
                .WithTitle(_title)
                .WithTitleColor(_titleColor)
                .WithContentBuilder(new ConfirmContentBuilder())
                .WithButton(cancelLabel, () => controller.Cancel())
                .WithButton(okLabel, () => controller.Confirm(), _okStyle);

            PopinController popin = popinBuilder.Build();
            ConfirmContentController content = popinBuilder.ContentController;

            // The controller lives on the popup's always-active root.
            controller = popin.gameObject
                .AddComponent<ConfirmPopinController>()
                .WithPopinController(popin)
                .WithContent(content);
            controller.SetMessage(_message);
            controller.SetOkAction(_okAction);
            return controller;
        }
    }
}
