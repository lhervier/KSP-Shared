using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.overlay;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent;
using com.github.lhervier.ksp.shared.ugui.combo.itemcontent.label;

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
        public ComboController WithLabelComponent(TextMeshProUGUI value)
        {
            _value = value;
            return this;
        }

        private RectTransform _headerRect;
        public ComboController WithHeaderRect(RectTransform headerRect)
        {
            _headerRect = headerRect;
            return this;
        }

        private GameObject _dropdown;
        private RectTransform _dropdownRect;
        private RectTransform _content;

        // Parents the dropdown/overlay are reparented away from on Open and restored to on Collapse,
        // so they float above everything while open yet stay owned by (and die with) this combo.
        private Transform _dropdownHome;
        private Transform _overlayHome;
        public ComboController WithDropDown(GameObject dropDown, RectTransform content)
        {
            this._dropdown = dropDown;
            this._dropdownRect = _dropdown.GetComponent<RectTransform>();
            this._content = content;
            return this;
        }

        private Button _button;
        public ComboController WithButtonComponent(Button button)
        {
            _button = button;
            return this;
        }

        private OverlayController _overlayController;
        public ComboController WithOverlayController(OverlayController overlay)
        {
            _overlayController = overlay;
            return this;
        }

        
        private Func<string, string> _labelFor;
        public ComboController WithLabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        private BaseComboItemContentBuilder _itemContentBuilder;
        public ComboController WithItemContentBuilder(BaseComboItemContentBuilder itemContentBuilder)
        {
            this._itemContentBuilder = itemContentBuilder;
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
            // While open these live under the root canvas, not under us: Unity won't cascade-destroy
            // them with the combo, so we destroy them explicitly to avoid orphaning them on screen.
            if (_dropdown != null) Destroy(_dropdown);
            if (_overlayController != null) Destroy(_overlayController.gameObject);
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

            // Resolving the default list rendering is the combo's job: a plain label fed by our own
            // LabelFor. A caller-supplied builder takes over entirely (and brings its own dependencies).
            if( _itemContentBuilder == null )
            {
                LabelComboItemContentBuilder labelBuilder = new LabelComboItemContentBuilder()
                    .WithLabelFor(Label);
                _itemContentBuilder = labelBuilder;
            }

            foreach (string opt in options)
            {
                ComboItemController ctrl = new ComboItemBuilder()
                    .WithParent(_content)
                    .WithId(opt)
                    .WithContentBuilder(_itemContentBuilder)
                    .WithSelected(opt == current)
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

            // Float above EVERYTHING by reparenting the trap then the dropdown onto the host canvas
            // and making them the last siblings: render order within one canvas follows the
            // hierarchy, so this guarantees foreground without depending on KSP's canvas sorting
            // orders. Home parents are kept so Collapse can restore them (and they die with us).
            // worldPositionStays:false keeps the trap centred (anchoredPosition 0) and harmless for
            // the dropdown, which is repositioned by world corners just below.
            Transform top = ResolveTopCanvas();

            if (_overlayController != null) {
                _overlayHome = _overlayController.transform.parent;
                _overlayController.gameObject.SetActive(true);
                if (top != null) _overlayController.transform.SetParent(top, false);
                _overlayController.transform.SetAsLastSibling();
            }
            _dropdownHome = _dropdown.transform.parent;
            _dropdown.SetActive(true);
            if (top != null) _dropdown.transform.SetParent(top, false);
            _dropdown.transform.SetAsLastSibling();

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
            // Restore the home parents we moved them away from in Open (keep local layout: false).
            if (_dropdown != null) {
                _dropdown.SetActive(false);
                if (_dropdownHome != null) { _dropdown.transform.SetParent(_dropdownHome, false); _dropdownHome = null; }
            }
            if (_overlayController != null) {
                _overlayController.gameObject.SetActive(false);
                if (_overlayHome != null) { _overlayController.transform.SetParent(_overlayHome, false); _overlayHome = null; }
            }
        }

        // The NEAREST enclosing canvas' transform — i.e. the very surface the host window renders on.
        // Reparenting here + SetAsLastSibling puts the dropdown above the window (a sibling drawn
        // earlier) without leaving that canvas. NOT rootCanvas: a KSP popup lives on a high-order
        // overrideSorting sub-canvas, so climbing to the top canvas would sink the dropdown behind
        // the window (and into a different CanvasScaler, breaking its width).
        private Transform ResolveTopCanvas()
        {
            Canvas canvas = _dropdown.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.transform : _dropdown.transform.root;
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
