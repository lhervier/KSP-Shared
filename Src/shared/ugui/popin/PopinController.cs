using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.popin
{
    /// <summary>
    /// Drives a popin built by <see cref="PopinBuilder{TContent,TFooter}"/>: a modal
    /// card laid over a window. The root this controller lives on stays always active (so its lifecycle
    /// runs and the owner can drive it from the start); only the dimmed panel is shown/hidden.
    /// </summary>
    public class PopinController : MonoBehaviour
    {
        // The dimmed, modal panel holding the card. Toggled by Show()/Close(); the root stays active.
        private GameObject _panel;
        public PopinController WithPanel(GameObject panel)
        {
            this._panel = panel;
            return this;
        }

        /// <summary>Show the popin (its card and the modal dim layer behind it).</summary>
        public void Show()
        {
            if( _panel != null ) _panel.SetActive(true);
        }

        /// <summary>Close the popin (hide its card and the modal dim layer).</summary>
        public void Close()
        {
            if( _panel != null ) _panel.SetActive(false);
        }
    }
}
