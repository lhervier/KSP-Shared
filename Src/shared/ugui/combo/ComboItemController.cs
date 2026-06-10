using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.combo
{
    public class ComboItemController : MonoBehaviour
    {
        public EventData<string> OnClick = new EventData<string>("ComboItem.OnClick");

        private string _id;
        public ComboItemController Id(string id)
        {
            this._id = id;
            return this;
        }

        private Button _button;
        public ComboItemController Button(Button button)
        {
            _button = button;
            return this;
        }

        public void Start()
        {
            if( _button != null )
            {
                _button.onClick.AddListener(_OnClick);
            }
        }

        public void OnDestroy()
        {
            if( _button != null )
            {
                _button.onClick.RemoveListener(_OnClick);
            }
        }

        private void _OnClick()
        {
            OnClick.Fire(this._id);
        }
    }
}