using System.Reflection.Emit;
using com.github.lhervier.ksp.shared.ugui.styles;
using TMPro;
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

        private bool _selected = false;
        public ComboItemController Selected(bool selected)
        {
            this._selected = selected;
            return this;
        }

        private Button _button;
        public ComboItemController Button(Button button)
        {
            _button = button;
            return this;
        }

        private TextMeshProUGUI _label;
        public ComboItemController Label(TextMeshProUGUI label)
        {
            this._label = label;
            return this;
        }

        public void Start()
        {
            if( _button != null )
            {
                _button.onClick.AddListener(_OnClick);
            }
            SelectInternal(this._selected);
        }

        public void OnDestroy()
        {
            if( _button != null )
            {
                _button.onClick.RemoveListener(_OnClick);
            }
        }

        // ==========================================
        // Methods bound to events
        // ==========================================

        private void _OnClick()
        {
            OnClick.Fire(this._id);
        }

        // ====================================
        // Public API
        // ====================================

        public string GetId()
        {
            return this._id;
        }

        public bool IsSelected()
        {
            return this._selected;
        }

        public void Select(bool select)
        {
            if( select == _selected ) return;
            this.SelectInternal(select);
        }

        // =======================================================
        // Internal helpers
        // =======================================================
        
        private void SelectInternal(bool select)
        {
            if( _button == null || _label == null )
            {
                return;
            }

            _selected = select;

            var colors = _button.colors;
            if( _selected )
            {
                colors.normalColor = ComboPalette.ItemSelectedBgColor;
                colors.selectedColor = ComboPalette.ItemSelectedBgColor;
                _label.color = ComboPalette.ItemSelectedColor;
            }
            else
            {
                colors.normalColor = Color.clear;
                colors.selectedColor = Color.clear;
                _label.color = ComboPalette.ItemColor;
            }
            _button.colors = colors;
        }
    }
}