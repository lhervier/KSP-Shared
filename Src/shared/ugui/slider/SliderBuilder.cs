using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.shared.ugui.slider
{
    /// <summary>
    /// Builds a reusable dark-themed slider (thin track + accent-green fill and handle), assembled on
    /// top of the Unity Slider component. Default colors/metrics: SliderPalette.
    /// </summary>
    public class SliderBuilder : IUGUIBuilder<SliderController>
    {
        // ===================================================
        // Builder parameters
        // ===================================================

        private Transform _parent;
        public SliderBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private float _min = 0f;
        public SliderBuilder Min(float min)
        {
            this._min = min;
            return this;
        }

        private float _max = 1f;
        public SliderBuilder Max(float max)
        {
            this._max = max;
            return this;
        }

        private float _value = 0f;
        public SliderBuilder Value(float value)
        {
            this._value = value;
            return this;
        }

        // Constrain to integer values (step of 1). Otherwise the slider is continuous.
        private bool _wholeNumbers = false;
        public SliderBuilder WholeNumbers(bool wholeNumbers)
        {
            this._wholeNumbers = wholeNumbers;
            return this;
        }

        // =======================================
        // Build
        // =======================================

        public SliderController Build()
        {
            // Root: holds the Slider component and reserves the height in the parent layout.
            var rootGo = new GameObject("Slider", typeof(RectTransform));
            if (_parent != null) rootGo.transform.SetParent(_parent, false);

            var le = rootGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = SliderPalette.Height;

            var slider = rootGo.AddComponent<Slider>();

            // Transparent full-area hit surface: makes click/drag work anywhere across the slider's
            // height, not only on the thin track.
            var hitImage = rootGo.AddComponent<Image>();
            hitImage.sprite = SpritesGlobal.FillSprite;
            hitImage.type = Image.Type.Simple;
            hitImage.color = Color.clear;
            hitImage.raycastTarget = true;

            // Background track: thin bar, vertically centered, full width.
            var bgGo = NewStretchChild(rootGo.transform, "Background", SliderPalette.TrackHeight);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.sprite = SpritesGlobal.FillSprite;
            bgImage.type = Image.Type.Simple;
            bgImage.color = SliderPalette.TrackColor;
            bgImage.raycastTarget = true;

            // Fill area: same footprint as the track; the child Fill follows the value.
            var fillAreaGo = NewStretchChild(rootGo.transform, "Fill Area", SliderPalette.TrackHeight);
            var fillAreaRect = fillAreaGo.GetComponent<RectTransform>();

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.sprite = SpritesGlobal.FillSprite;
            fillImage.type = Image.Type.Simple;
            fillImage.color = SliderPalette.FillColor;
            fillImage.raycastTarget = false;

            // Slide area + square bordered handle.
            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(rootGo.transform, false);
            var handleAreaRect = handleAreaGo.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            // Horizontal inset = half a handle, so the handle stays within the edges at the extremes.
            handleAreaRect.offsetMin = new Vector2(SliderPalette.HandleSize / 2f, 0f);
            handleAreaRect.offsetMax = new Vector2(-SliderPalette.HandleSize / 2f, 0f);

            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(SliderPalette.HandleSize, SliderPalette.HandleSize);
            var handleImage = handleGo.AddComponent<Image>();
            handleImage.sprite = SpritesGlobal.Border(SliderPalette.HandleColor, SliderPalette.HandleBorderColor, SliderPalette.HandleBorderThickness);
            handleImage.type = Image.Type.Sliced;
            handleImage.color = Color.white;
            handleImage.raycastTarget = true;

            // Wire up the Unity Slider.
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.transition = Selectable.Transition.None;
            slider.minValue = _min;
            slider.maxValue = _max;
            slider.wholeNumbers = _wholeNumbers;
            slider.value = Mathf.Clamp(_value, _min, _max);

            return rootGo
                .AddComponent<SliderController>()
                .Slider(slider);
        }

        // Child anchored to full width, fixed height, vertically centered.
        private static GameObject NewStretchChild(Transform parent, string objectName, float height)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
            return go;
        }
    }
}
