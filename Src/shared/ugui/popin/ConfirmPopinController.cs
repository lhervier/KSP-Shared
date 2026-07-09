using System;
using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Drives a confirmation popin built by <see cref="ConfirmPopinBuilder"/>: a modal card holding a
    /// message and a Cancel/OK button bar. The message and the OK action can be (re)set after Build, so a
    /// single popin instance can serve successive confirmations. Cancel just closes; OK closes then runs
    /// the current action. Lives on the popin's always-active root.
    /// </summary>
    public class ConfirmPopinController : MonoBehaviour
    {
        private PopinController _popin;
        public ConfirmPopinController WithPopinController(PopinController popin)
        {
            this._popin = popin;
            return this;
        }

        private ConfirmContentController _content;
        public ConfirmPopinController WithContent(ConfirmContentController content)
        {
            this._content = content;
            return this;
        }

        private Action _okAction;

        /// <summary>Set the message shown in the card body.</summary>
        public void SetMessage(string message)
        {
            _content?.SetMessage(message);
        }

        /// <summary>Set the action run when the user confirms (OK). Cancel never runs it.</summary>
        public void SetOkAction(Action okAction)
        {
            this._okAction = okAction;
        }

        /// <summary>Show the confirmation popin.</summary>
        public void Show()
        {
            _popin?.Show();
        }

        /// <summary>Close the confirmation popin without running the OK action.</summary>
        public void Close()
        {
            _popin?.Close();
        }

        /// <summary>Run the OK path (bound to the OK button by the builder).</summary>
        public void Confirm()
        {
            // Close first, so the popin is gone even if the action navigates away or throws; snapshot the
            // action beforehand so a re-entrant SetOkAction cannot swap what this click runs.
            Action action = _okAction;
            Close();
            action?.Invoke();
        }

        /// <summary>Run the Cancel path (bound to the Cancel button by the builder).</summary>
        public void Cancel()
        {
            Close();
        }
    }
}
