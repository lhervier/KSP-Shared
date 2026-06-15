using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.shared.ugui.slider
{
    /// <summary>
    /// Drives a shared slider. Exposes the value through the OnValueChanged event and the Get/Set API.
    /// SetValue syncs from the model without re-firing the event (no feedback loop).
    /// </summary>
    public class SliderController : MonoBehaviour
    {
        /// <summary>Fired whenever the value changes because of the user (handle drag).</summary>
        public EventData<float> OnValueChanged = new EventData<float>("KSPShared.UGUI.Slider.OnValueChanged");

        // True during SetValue/SetRange: neutralizes the Unity event triggered by a programmatic
        // assignment, so only user-driven changes are propagated.
        private bool _suppressNotify = false;

        // ============================================
        // Life cycle
        // ============================================

        private Slider _slider;
        public SliderController Slider(Slider slider)
        {
            this._slider = slider;
            return this;
        }

        public void Start()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        public void OnDestroy()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnSliderValueChanged(float value)
        {
            if (_suppressNotify) return;
            OnValueChanged.Fire(value);
        }

        // ============================================
        // Public API
        // ============================================

        public float GetValue()
        {
            return _slider != null ? _slider.value : 0f;
        }

        /// <summary>Sets the value without firing OnValueChanged.</summary>
        public void SetValue(float value)
        {
            if (_slider == null) return;
            _suppressNotify = true;
            try
            {
                _slider.value = value;
            }
            finally
            {
                _suppressNotify = false;
            }
        }

        /// <summary>Redefines the bounds (clamping the value into the new range) without firing the event.</summary>
        public void SetRange(float min, float max)
        {
            if (_slider == null) return;
            _suppressNotify = true;
            try
            {
                _slider.minValue = min;
                _slider.maxValue = max;
            }
            finally
            {
                _suppressNotify = false;
            }
        }
    }
}
