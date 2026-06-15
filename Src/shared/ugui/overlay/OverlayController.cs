using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.overlay
{
    /// <summary>
    /// Drives the click trap built by <see cref="OverlayBuilder"/>: a click on the overlay fires
    /// <see cref="OnClose"/>, which the owner subscribes to in order to dismiss its surface.
    /// </summary>
    public class OverlayController : MonoBehaviour
    {
        public readonly EventVoid OnClose = new EventVoid("Overlay.OnClose");

        private Button _overlay;
        public OverlayController WithButtonComponent(Button overlay)
        {
            this._overlay = overlay;
            return this;
        }

        public void Start()
        {
            if( _overlay != null )
            {
                _overlay.onClick.AddListener(OnClick);
            }
        }

        public void OnDestroy()
        {
            if( _overlay != null )
            {
                _overlay.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            this.OnClose.Fire();
        }
    }
}
