using System;
using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Footer of a popin: holds its buttons and the action bound to each. Runs the bound action when a
    /// button is clicked. Registration happens on Start and is released on OnDestroy.
    /// </summary>
    public class PopinButtonBarController : MonoBehaviour
    {
        private readonly List<ButtonController> _buttons = new List<ButtonController>();
        private readonly List<Action> _actions = new List<Action>();

        // OnClick handlers actually registered (kept so Start's registration can be undone in OnDestroy).
        private readonly List<EventVoid.OnEvent> _handlers = new List<EventVoid.OnEvent>();

        /// <summary>Register a footer button and the action to run when it is clicked.</summary>
        public PopinButtonBarController AddButton(ButtonController button, Action action)
        {
            _buttons.Add(button);
            _actions.Add(action);
            return this;
        }

        public void Start()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                // Wrap the Action into the EventVoid delegate once, and keep that exact reference: OnDestroy
                // must Remove the same instance it added (a fresh lambda would not match).
                Action action = _actions[i];
                EventVoid.OnEvent handler = () => action?.Invoke();
                _handlers.Add(handler);
                if (_buttons[i] != null) _buttons[i].OnClick.Add(handler);
            }
        }

        public void OnDestroy()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_buttons[i] != null) _buttons[i].OnClick.Remove(_handlers[i]);
            }
        }
    }
}
