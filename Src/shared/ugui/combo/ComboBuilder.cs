using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.overlay;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.shared.ugui.combo
{
    /// <summary>
    /// Combo réutilisable : une ligne [libellé][valeur ▼] et un dropdown qui FLOTTE par-dessus le
    /// contenu en dessous (positionné en absolu sous l'en-tête, passé au premier plan via
    /// SetAsLastSibling, hauteur plafonnée + ScrollRect si la liste est longue). Combo « bête » :
    /// l'appelant fournit les options via SetOptions(...) et reçoit le choix via OnSelect.
    /// </summary>
    public class ComboBuilder : IUGUIBuilder<ComboController>
    {
        // Preferred caret with degraded alternatives, picked at build time depending on what the
        // font (base + fallbacks) can actually render. "↓" is known to be available.
        private static string CaretGlyph => DefaultPalette.PickGlyph("▼", "▾", "↓");

        // ====================================
        // Builder parameters
        // ====================================

        private Transform _parent;
        public ComboBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _label;
        public ComboBuilder Label(string label)
        {
            _label = label;
            return this;
        }

        private Func<string, string> _labelFor;
        public ComboBuilder LabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        public ComboController Build()
        {
            var rootGo = new GameObject("Combo", typeof(RectTransform));
            rootGo.transform.SetParent(_parent, false);
            
            var rowLayout = rootGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 0, 0);
            rowLayout.spacing = ComboPalette.Spacing;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rootGo.transform, false);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.minWidth = labelLe.preferredWidth = 46f;
            var label = UGUILabels.AddLabel(labelGo);
            label.text = _label;
            label.fontSize = ComboPalette.LabelFontSize;
            label.color = ComboPalette.LabelColor;
            label.alignment = TextAlignmentOptions.Left;

            // En-tête : bouton plein largeur [valeur (flexible)] [caret]
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(rootGo.transform, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.flexibleWidth = 1f;
            headerLe.minHeight = headerLe.preferredHeight = ComboPalette.Height;
            var headerRect = headerGo.GetComponent<RectTransform>();

            var headerImage = headerGo.AddComponent<Image>();
            headerImage.sprite = SpritesGlobal.Border(ComboPalette.BackgroundColor, ComboPalette.BorderColor, 1);
            headerImage.type = Image.Type.Sliced;
            headerImage.color = Color.white;
            headerImage.raycastTarget = true;

            var headerBtn = headerGo.AddComponent<Button>();
            headerBtn.targetGraphic = headerImage;
            var hColors = headerBtn.colors;
            hColors.normalColor = ComboPalette.BackgroundColor;
            hColors.highlightedColor = ComboPalette.HoverColor;
            hColors.pressedColor = ComboPalette.HoverColor;
            hColors.selectedColor = ComboPalette.BackgroundColor;
            hColors.colorMultiplier = 1f;
            hColors.fadeDuration = 0.1f;
            headerBtn.colors = hColors;

            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(Mathf.RoundToInt(ComboPalette.PaddingH), Mathf.RoundToInt(ComboPalette.PaddingH), 0, 0);
            headerLayout.spacing = ComboPalette.Spacing;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var valueGo = new GameObject("Value", typeof(RectTransform));
            valueGo.transform.SetParent(headerGo.transform, false);
            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.flexibleWidth = 1f;
            var value = UGUILabels.AddLabel(valueGo);
            value.fontSize = ComboPalette.ComboFontSize;
            value.color = ComboPalette.ComboTextColor;
            value.alignment = TextAlignmentOptions.Left;

            var caretGo = new GameObject("Caret", typeof(RectTransform));
            caretGo.transform.SetParent(headerGo.transform, false);
            var caret = UGUILabels.AddLabel(caretGo);
            caret.text = CaretGlyph;
            caret.fontSize = ComboPalette.CaretFontSize;
            caret.color = ComboPalette.CaretColor;
            caret.alignment = TextAlignmentOptions.Center;

            // Dropdown flottant : enfant du MÊME parent (le panneau du menu), positionné en absolu
            // sous l'en-tête à l'ouverture. ignoreLayout pour ne pas occuper de place dans le menu.
            GameObject dropdown = BuildDropdown(out RectTransform content);

            // Piège à clic propre au combo : referme le dropdown dès qu'on clique en dehors.
            // Composant générique partagé ; on le parente au même panneau et on le masque jusqu'à l'ouverture.
            OverlayController overlay = new OverlayBuilder().Build();
            overlay.transform.SetParent(_parent, false);
            overlay.gameObject.SetActive(false);

            return rootGo
                .AddComponent<ComboController>()
                .Value(value)
                .HeaderRect(headerRect)
                .DropDown(dropdown, content)
                .Button(headerBtn)
                .OverlayController(overlay)
                .LabelFor(_labelFor);
        }

        // Panneau scrollable du dropdown (construit détaché et masqué ; positionné/affiché à l'ouverture).
        private GameObject BuildDropdown(out RectTransform content)
        {
            var dropdownGo = new GameObject("ComboDropdown", typeof(RectTransform));
            dropdownGo.transform.SetParent(_parent, false);

            var le = dropdownGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            var image = dropdownGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.Border(ComboPalette.ComboBgColor, ComboPalette.BorderColor, 1);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var scrollRect = dropdownGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 18f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(dropdownGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportGo.AddComponent<RectMask2D>();
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.sprite = SpritesGlobal.FillSprite;
            viewportImage.type = Image.Type.Simple;
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
            scrollRect.viewport = viewportRect;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            scrollRect.content = content;

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 3, 3);
            layout.spacing = 0f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            dropdownGo.SetActive(false);
            return dropdownGo;
        }
    }
}
