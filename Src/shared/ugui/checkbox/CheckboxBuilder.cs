using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.checkbox
{
    public class CheckboxBuilder : IUGUIBuilder<CheckboxController>
    {
        private bool _isChecked = false;
        private string _label = string.Empty;
        private bool _greedy = false;

        // ==============================================
        // Build parameters
        // ==============================================

        public CheckboxBuilder Checked(bool isChecked)
        {
            this._isChecked = isChecked;
            return this;
        }

        /// <summary>Optional label displayed next to the box. Empty => box only (zero-width label).</summary>
        public CheckboxBuilder Label(string label)
        {
            this._label = label ?? string.Empty;
            return this;
        }

        /// <summary>
        /// When true, the clickable area fills the available width (the whole row toggles).
        /// When false (default), it hugs box+label and stays left-aligned, so clicks past the
        /// label do nothing — even if a parent layout stretches the row.
        /// </summary>
        public CheckboxBuilder Greedy(bool greedy)
        {
            this._greedy = greedy;
            return this;
        }

        // ================================================
        // Builder
        // ================================================

        public CheckboxController Build()
        {
            // Root: hosts the clickable wrapper, left-aligned. It may be stretched by the parent
            // layout, but never force-expands its child, so empty space on its right is NOT clickable.
            var rootGo = new GameObject("Checkbox", typeof(RectTransform));
            
            var rootLayout = rootGo.AddComponent<HorizontalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 0);
            rootLayout.spacing = 0f;
            rootLayout.childAlignment = TextAnchor.MiddleLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = false;

            // In greedy mode, the root grabs the leftover width of its own parent layout.
            var rootLayoutElement = rootGo.AddComponent<LayoutElement>();
            rootLayoutElement.flexibleWidth = _greedy ? 1f : 0f;

            // Clickable wrapper: the single raycast surface (box + label both bubble up here).
            // hug => flexibleWidth 0 (sticks to its content) ; greedy => 1 (fills the root).
            var clickableGo = new GameObject("Clickable", typeof(RectTransform));
            clickableGo.transform.SetParent(rootGo.transform, false);

            var clickableImage = clickableGo.AddComponent<Image>();
            clickableImage.sprite = SpritesGlobal.FillSprite;
            clickableImage.type = Image.Type.Simple;
            clickableImage.color = Color.clear;
            clickableImage.raycastTarget = true;

            var clickHandler = clickableGo.AddComponent<PointerHandler>();
            
            var clickableLayout = clickableGo.AddComponent<HorizontalLayoutGroup>();
            clickableLayout.padding = new RectOffset(0, 0, 0, 0);
            clickableLayout.spacing = DefaultPalette.Spacing;
            clickableLayout.childAlignment = TextAnchor.MiddleLeft;
            clickableLayout.childControlWidth = true;
            clickableLayout.childControlHeight = true;
            clickableLayout.childForceExpandWidth = false;
            clickableLayout.childForceExpandHeight = false;

            var clickableLayoutElement = clickableGo.AddComponent<LayoutElement>();
            clickableLayoutElement.flexibleWidth = _greedy ? 1f : 0f;

            // Box: fixed 12x12 square with a dark background. raycastTarget = false so clicks
            // bubble up to the wrapper's PointerHandler (single click entry point).
            var boxGo = new GameObject("Box", typeof(RectTransform));
            boxGo.transform.SetParent(clickableGo.transform, false);

            var boxLayoutElement = boxGo.AddComponent<LayoutElement>();
            boxLayoutElement.preferredWidth = CheckboxPalette.CheckboxSize;
            boxLayoutElement.preferredHeight = CheckboxPalette.CheckboxSize;
            boxLayoutElement.minWidth = CheckboxPalette.CheckboxSize;
            boxLayoutElement.minHeight = CheckboxPalette.CheckboxSize;

            var bgImage = boxGo.AddComponent<Image>();
            bgImage.sprite = SpritesGlobal.FillSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.color = DefaultPalette.FieldBackgroundColor;
            bgImage.raycastTarget = false;

            // Green inner fill that represents the "checked" state
            var checkmarkGo = new GameObject("Checkmark", typeof(RectTransform));
            checkmarkGo.transform.SetParent(boxGo.transform, false);
            
            var checkmarkRect = checkmarkGo.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = new Vector2(CheckboxPalette.CheckmarkInset, CheckboxPalette.CheckmarkInset);
            checkmarkRect.offsetMax = new Vector2(-CheckboxPalette.CheckmarkInset, -CheckboxPalette.CheckmarkInset);

            var checkmarkImage = checkmarkGo.AddComponent<Image>();
            checkmarkImage.sprite = SpritesGlobal.FillSprite;
            checkmarkImage.type = Image.Type.Simple;
            checkmarkImage.color = DefaultPalette.AccentColor;
            checkmarkImage.raycastTarget = false;

            // Initial visibility from the parameter; checkmarkGo.activeSelf is the source of truth
            // (so external updates via SetActive stay consistent with what the click handler reads).
            checkmarkGo.SetActive(_isChecked);

            // Label: optional, raycastTarget = false (clicks pass through to the wrapper).
            // Empty text => zero width => only the box is visible.
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(clickableGo.transform, false);

            var labelLayoutElement = labelGo.AddComponent<LayoutElement>();
            labelLayoutElement.flexibleWidth = _greedy ? 1f : 0f;

            var label = labelGo.AddComponent<Text>();
            label.text = _label;
            label.font = HighLogic.UISkin.font;
            label.fontSize = CheckboxPalette.CheckboxFontSize;
            label.color = DefaultPalette.LabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            
            return rootGo
                .AddComponent<CheckboxController>()
                .ClickHandler(clickHandler)
                .Checkmark(checkmarkGo)
                .Label(label);
        }
    }
}
