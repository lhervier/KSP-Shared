using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.shared.ugui.popup
{
    /// <summary>
    /// Pieces of a freshly spawned window instance, handed from the builder's spawn routine back to the
    /// controller that drives them. One per spawn: the controller re-requests one whenever it re-opens a
    /// window that KSP had destroyed.
    /// </summary>
    public class PopupSpawnResult
    {
        public PopupDialog PopupDialog;
        public CanvasGroup CanvasGroup;
        public ButtonController CloseButtonController;
    }
}
