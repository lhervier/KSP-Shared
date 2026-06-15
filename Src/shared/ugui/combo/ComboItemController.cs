using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;
using com.github.lhervier.ksp.shared.ugui.styles;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.combo
{
    public class ComboItemController : MonoBehaviour
    {
        public EventData<string> OnClick = new EventData<string>("ComboItem.OnClick");

        private string _id;
        public ComboItemController WithId(string id)
        {
            this._id = id;
            return this;
        }

        private bool _selected = false;
        public ComboItemController WithSelectedState(bool selected)
        {
            this._selected = selected;
            return this;
        }

        private Button _button;
        public ComboItemController WithButtonComponent(Button button)
        {
            _button = button;
            return this;
        }

        private ComboItemContentController _content;
        public ComboItemController WithContentBuilder(ComboItemContentController content)
        {
            this._content = content;
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
            if( _button == null )
            {
                return;
            }

            _selected = select;

            // The row only owns its selection background; how the content reflects selection
            // (text color, icon swap...) is the content's own responsibility.
            var colors = _button.colors;
            colors.normalColor = _selected ? ComboPalette.ItemSelectedBgColor : Color.clear;
            colors.selectedColor = _selected ? ComboPalette.ItemSelectedBgColor : Color.clear;
            _button.colors = colors;

            if( _content != null )
            {
                _content.SetSelected(_selected);
            }
        }
    }
}