using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Builds the content of a confirmation popin: a single wrapped message label (filled by the popin
    /// controller). Styled from the shared <see cref="PopinPalette"/>.
    /// </summary>
    public class ConfirmContentBuilder : IUGUIBuilder<ConfirmContentController>
    {
        public ConfirmContentController Build()
        {
            var go = new GameObject("ConfirmContent", typeof(RectTransform));

            var message = UGUILabels.AddLabel(go);
            message.text = string.Empty;
            message.fontSize = PopinPalette.MessageFontSize;
            message.color = PopinPalette.MessageColor;
            message.alignment = TextAlignmentOptions.TopLeft;
            message.enableWordWrapping = true;

            return go
                .AddComponent<ConfirmContentController>()
                .WithMessageComponent(message);
        }
    }
}
