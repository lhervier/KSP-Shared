using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.overlay;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.combo
{
    public class ComboController : MonoBehaviour
    {
        private readonly List<ComboItemController> _items = new List<ComboItemController>();

        /// <summary>Callback invoqué quand l'utilisateur choisit une option (reçoit la VALEUR brute).</summary>
        public EventData<string> OnSelect = new EventData<string>("Combobox.OnSelect");

        // =======================================
        // Life cycle
        // =======================================

        private TextMeshProUGUI _value;
        public ComboController Value(TextMeshProUGUI value)
        {
            _value = value;
            return this;
        }

        private RectTransform _headerRect;
        public ComboController HeaderRect(RectTransform headerRect)
        {
            _headerRect = headerRect;
            return this;
        }

        private GameObject _dropdown;
        private RectTransform _dropdownRect;
        private RectTransform _content;
        public ComboController DropDown(GameObject dropDown, RectTransform content)
        {
            this._dropdown = dropDown;
            this._dropdownRect = _dropdown.GetComponent<RectTransform>();
            this._content = content;
            return this;
        }

        private Button _button;
        public ComboController Button(Button button)
        {
            _button = button;
            return this;
        }

        private OverlayController _overlayController;
        public ComboController OverlayController(OverlayController overlay)
        {
            _overlayController = overlay;
            return this;
        }

        
        private Func<string, string> _labelFor;
        public ComboController LabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        public void Start()
        {
            // A click on the overlay (anywhere outside the dropdown) collapses the combo.
            if (_overlayController != null) {
                _overlayController.OnClose.Add(Collapse);
            }

            if( _button != null )
            {
                _button.onClick.AddListener(Toggle);
            }
        }

        public void OnDestroy()
        {
            if( _button != null )
            {
                _button.onClick.RemoveListener(Toggle);
            }
            if (_overlayController != null) {
                _overlayController.OnClose.Remove(Collapse);
            }
            foreach (var item in _items) {
                item.OnClick.Remove(OnItemClicked);
            }
        }

        // =============================
        // Public API
        // =============================

        public void SetOptions(IReadOnlyList<string> options, string current)
        {
            if (_value != null) _value.text = Label(current);

            foreach (var item in _items) {
                item.OnClick.Remove(OnItemClicked);
                Destroy(item.gameObject);
            }
            _items.Clear();
            if (options == null) return;

            foreach (string opt in options)
            {
                ComboItemController ctrl = new ComboItemBuilder()
                    .Parent(_content)
                    .Id(opt)
                    .Label(Label(opt))
                    .Selected(opt == current)
                    .Build();
                ctrl.OnClick.Add(OnItemClicked);
                _items.Add(ctrl);
            }
        }

        public void Select(string current)
        {
            if (_value != null) _value.text = Label(current);

            foreach (var item in _items) {
                item.Select(item.GetId() == current);
            }
        }

        public void Toggle()
        {
            if (_dropdown == null) return;
            if (_dropdown.activeSelf) Collapse();
            else Open();
        }

        public void Open()
        {
            if (_dropdown == null) return;

            // Le piège passe au premier plan, puis le dropdown par-dessus le piège.
            if (_overlayController != null) { _overlayController.gameObject.SetActive(true); _overlayController.transform.SetAsLastSibling(); }
            _dropdown.SetActive(true);
            _dropdown.transform.SetAsLastSibling();   // au premier plan, par-dessus le contenu du menu

            // Hauteur = contenu, plafonnée (au-delà → scroll) ; largeur = celle de l'en-tête.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            float height = Mathf.Min(LayoutUtility.GetPreferredHeight(_content), ComboPalette.DropdownMaxHeight);

            var corners = new Vector3[4];
            _headerRect.GetWorldCorners(corners);   // 0 = bas-gauche
            _dropdownRect.anchorMin = new Vector2(0f, 1f);
            _dropdownRect.anchorMax = new Vector2(0f, 1f);
            _dropdownRect.pivot = new Vector2(0f, 1f);
            _dropdownRect.sizeDelta = new Vector2(_headerRect.rect.width, height);
            _dropdownRect.position = corners[0];
        }

        public void Collapse()
        {
            if (_dropdown != null) _dropdown.SetActive(false);
            if (_overlayController != null) _overlayController.gameObject.SetActive(false);
        }

        // ========================================
        // Methods bound to events
        // ========================================

        private void OnItemClicked(string id)
        {
            OnSelect.Fire(id); 
            Collapse();
        }

        // =============================================
        // Helpers
        // =============================================

        private string Label(string value)
        {
            return _labelFor != null ? _labelFor(value) : (value ?? string.Empty);
        }
    }
}
