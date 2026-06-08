using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.checkbox
{
    public class CheckboxController : MonoBehaviour
    {
        private GameObject _checkMark;
        private Button _button;

        public EventData<bool> OnToggled = new EventData<bool>("KSPShared.UGUI.Checkbox.OnToggled");

        public void BindButton(Button button)
        {
            this._button = button;
        }

        public void BindCheckmark(GameObject checkmark)
        {
            _checkMark = checkmark;
        }

        public void Start()
        {
            if( _button != null )
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        public void OnDestroy()
        {
            if( _button != null )
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            // checkMark.activeSelf is the current (pre-click) visual state, so the new desired
            // state is its negation. The callee decides whether to apply it (e.g., by reading
            // the model's state, calling a toggle method, etc.).
            SetChecked(!_checkMark.activeSelf);
        }

        public bool IsChecked()
        {
            if( _checkMark == null ) return false;
            return _checkMark.activeInHierarchy;
        }

        public void SetChecked(bool isChecked)
        {
            if( _checkMark.activeSelf == isChecked ) return;
            _checkMark.SetActive(isChecked);
            OnToggled.Fire(isChecked);
        }
    }
}
