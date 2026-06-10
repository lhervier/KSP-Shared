using UnityEngine;

namespace com.github.lhervier.ksp.shared.ugui.internalpopup
{
    /// <summary>
    /// Drives an internal popup built by <see cref="InternalPopupBuilder{TContent,TFooter}"/>: a modal
    /// card laid over a window. The root this controller lives on stays always active (so its lifecycle
    /// runs and the owner can drive it from the start); only the dimmed panel is shown/hidden.
    /// </summary>
    public class InternalPopupController : MonoBehaviour
    {
        // The dimmed, modal panel holding the card. Toggled by Show()/Close(); the root stays active.
        private GameObject _panel;
        public InternalPopupController Panel(GameObject panel)
        {
            this._panel = panel;
            return this;
        }

        /// <summary>Show the internal popup (its card and the modal dim layer behind it).</summary>
        public void Show()
        {
            if( _panel != null ) _panel.SetActive(true);
        }

        /// <summary>Close the internal popup (hide its card and the modal dim layer).</summary>
        public void Close()
        {
            if( _panel != null ) _panel.SetActive(false);
        }
    }
}
