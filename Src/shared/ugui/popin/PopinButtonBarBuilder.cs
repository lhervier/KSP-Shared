using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Builds the footer of a popin: a right-aligned horizontal row of buttons, each declared by a label,
    /// a click action and a <see cref="PopinButtonStyle"/>. Button look and metrics are shared
    /// (see <see cref="PopinPalette"/>); mods only declare label/action/style.
    /// </summary>
    public class PopinButtonBarBuilder : IUGUIBuilder<PopinButtonBarController>
    {
        private struct ButtonSpec
        {
            public string Label;
            public Action Action;
            public PopinButtonStyle Style;
        }

        private readonly List<ButtonSpec> _buttons = new List<ButtonSpec>();

        /// <summary>Append a button styled according to <paramref name="style"/> (defaults to neutral).</summary>
        public PopinButtonBarBuilder WithButton(string label, Action action, PopinButtonStyle style = PopinButtonStyle.Normal)
        {
            _buttons.Add(new ButtonSpec { Label = label, Action = action, Style = style });
            return this;
        }

        public PopinButtonBarController Build()
        {
            var rootGo = new GameObject("PopinButtonBar", typeof(RectTransform));

            var layout = rootGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = PopinPalette.FooterSpacing;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var controller = rootGo.AddComponent<PopinButtonBarController>();

            foreach (ButtonSpec spec in _buttons)
            {
                ButtonController button = BuildButton(spec);
                button.transform.SetParent(rootGo.transform, false);
                controller.AddButton(button, spec.Action);
            }

            return controller;
        }

        private ButtonController BuildButton(ButtonSpec spec)
        {
            var builder = new ButtonBuilder()
                .WithObjectName(ObjectName(spec.Style))
                .WithLabel(spec.Label)
                .WithAutoWidth(PopinPalette.ButtonPaddingH)
                .WithSize(PopinPalette.ButtonHeight)
                .WithFontSize(PopinPalette.ButtonFontSize);

            switch (spec.Style)
            {
                case PopinButtonStyle.Confirm:
                    builder
                        .WithBackgroundColor(PopinPalette.ButtonConfirmBgColor)
                        .WithHoverColor(PopinPalette.ButtonConfirmHoverColor)
                        .WithTextColor(PopinPalette.ButtonConfirmTextColor);
                    break;
                case PopinButtonStyle.Alert:
                    builder
                        .WithBackgroundColor(PopinPalette.ButtonAlertBgColor)
                        .WithHoverColor(PopinPalette.ButtonAlertHoverColor)
                        .WithTextColor(PopinPalette.ButtonAlertTextColor);
                    break;
                default:
                    builder
                        .WithBackgroundColor(PopinPalette.ButtonBgColor)
                        .WithHoverColor(PopinPalette.ButtonHoverColor)
                        .WithTextColor(PopinPalette.ButtonTextColor);
                    break;
            }

            return builder.Build();
        }

        private static string ObjectName(PopinButtonStyle style)
        {
            switch (style)
            {
                case PopinButtonStyle.Confirm: return "ConfirmButton";
                case PopinButtonStyle.Alert: return "AlertButton";
                default: return "Button";
            }
        }
    }
}
