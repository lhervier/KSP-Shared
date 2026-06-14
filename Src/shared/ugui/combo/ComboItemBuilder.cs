using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;

namespace com.github.lhervier.ksp.shared.ugui.combo
{
    public class ComboItemBuilder : IUGUIBuilder<ComboItemController>
    {
        private Transform _parent;
        public ComboItemBuilder Parent(Transform parent)
        {
            _parent = parent;
            return this;
        }

        private string _id;
        public ComboItemBuilder Id(string id)
        {
            this._id = id;
            return this;
        }

        private bool _selected = false;
        public ComboItemBuilder Selected(bool selected)
        {
            _selected = selected;
            return this;
        }

        private BaseComboItemContentBuilder _contentBuilder;
        public ComboItemBuilder Content(BaseComboItemContentBuilder contentBuilder)
        {
            _contentBuilder = contentBuilder;
            return this;
        }

        public ComboItemController Build()
        {
            var itemGo = new GameObject("Item", typeof(RectTransform));
            itemGo.transform.SetParent(_parent, false);
            // Row height follows the content's preferred size (so a multi-line content fits); Height is
            // only a floor. The standard single-line height is carried by the content itself (e.g. the
            // default label), keeping the row as tall as whatever it is asked to display.
            var le = itemGo.AddComponent<LayoutElement>();
            le.minHeight = ComboPalette.Height;

            var image = itemGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = itemGo.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = _selected ? ComboPalette.ItemSelectedBgColor : Color.clear;
            colors.highlightedColor = ComboPalette.ItemHoverColor;
            colors.pressedColor = ComboPalette.ItemHoverColor;
            colors.selectedColor = _selected ? ComboPalette.ItemSelectedBgColor : Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            var layout = itemGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(ComboPalette.PaddingH), Mathf.RoundToInt(ComboPalette.PaddingH), 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // The row owns the click and the selection background; what is shown inside it is delegated
            // to the content builder so callers can render options freely. The builder is mandatory:
            // resolving the default (a plain label) is the combo's job, not this row's.
            ComboItemContentController content = _contentBuilder
                .Id(_id)
                .Build();
            content.transform.SetParent(itemGo.transform, false);

            return itemGo
                .AddComponent<ComboItemController>()
                .Id(_id)
                .Button(button)
                .Content(content)
                .Selected(_selected);
        }
    }
}