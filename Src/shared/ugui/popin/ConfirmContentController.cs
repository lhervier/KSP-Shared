using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Holds the confirmation popin message label and exposes only its text, so the popin controller can
    /// (re)fill it after Build without gaining access to the label's other properties.
    /// </summary>
    public class ConfirmContentController : MonoBehaviour
    {
        private TextMeshProUGUI _message;
        public ConfirmContentController WithMessageComponent(TextMeshProUGUI message)
        {
            this._message = message;
            return this;
        }

        /// <summary>Set the displayed message text.</summary>
        public void SetMessage(string message)
        {
            if( _message != null ) _message.text = message ?? string.Empty;
        }
    }
}
