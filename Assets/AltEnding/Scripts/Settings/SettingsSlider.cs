using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace AltEnding.Settings
{
    public class SettingsSlider : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Serializable] public class BaseEventDataEvent : UnityEvent<BaseEventData> { }

        [SerializeField] protected SettingType myType;
        [SerializeField] protected Slider mySlider;
        [SerializeField] protected TextMeshProUGUI myNumberLabel;
        [SerializeField] protected bool setLabelToTypeName;
        private bool delayedInitialization = false;

        [Header("Events")]
        [SerializeField] protected UnityEvent onPointerEnter;
        [SerializeField] protected BaseEventDataEvent onPointerUp;
        [SerializeField] protected UnityEvent onSelect;
        [SerializeField] protected BaseEventDataEvent onDeselect;
        [SerializeField] protected UnityEvent onSubmit;

        void OnEnable()
        {
            if (SettingsManager.instance_Initialised)
            {
                SettingsManagerInstanceInitialized();
            }
            else
            {
                delayedInitialization = true;
                SettingsManager.settingsManagerInstanceInitialized += SettingsManagerInstanceInitialized;
			}
			SettingsManager.settingsLoaded += SettingsLoaded;
			SettingsManager.settingsSaved += UpdateSliderFromSettingsManager;
			SettingsManager.settingsReset += UpdateSliderFromSettingsManager;
		}

        void OnDisable()
        {
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            SettingsManager.settingsLoaded -= SettingsLoaded;
            SettingsManager.settingsSaved -= UpdateSliderFromSettingsManager;
            SettingsManager.settingsReset -= UpdateSliderFromSettingsManager;
        }

        private void SettingsManagerInstanceInitialized()
        {
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            if (SettingsManager.instance.initialized)
            {
                SettingsLoaded(true);
            }
        }

        private void UpdateSliderFromSettingsManager()
        {
            if (!SettingsManager.instance_Initialised) return;
            int value = 0;
            if (SettingsManager.instance.GetIntSetting(myType, ref value))
            {
                UpdateSlider(value);
            }
        }

        void SettingsLoaded(bool loaded)
        {
            UpdateSliderFromSettingsManager();
        }

        public void SetToDefault()
        {
            //Need some way in the settings manager to get a specific setting's default value.
        }

        public void SliderChangedBy(float changeValue)
        {
            if (mySlider != null)
            {
                mySlider.value += changeValue;
            }
        }

        public void SliderChanged(float newValue)
        {
            if(SettingsManager.instance_Initialised) SettingsManager.instance.ChangeSetting(myType, (int)newValue);
            if (myNumberLabel != null) myNumberLabel.text = newValue.ToString();
        }

        protected void UpdateSlider(int newValue)
        {
            Debug.Log($"Update Slider: {newValue}");
            if (mySlider != null) mySlider.SetValueWithoutNotify(newValue);
            if (myNumberLabel != null) myNumberLabel.text = newValue.ToString();
        }

        public void UpdateSliderAndSetting(int newValue)
        {
            UpdateSlider(newValue);
            SliderChanged(newValue);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp.Invoke(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            onSelect.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselect.Invoke(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            onSubmit.Invoke();
        }
    }
}
