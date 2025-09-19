using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Settings {
    public class FontSizeToggleManager : MonoBehaviour
    {
        [SerializeField] private Toggle smallToggle;
        [SerializeField] private Toggle defaultToggle;
        [SerializeField] private Toggle largeToggle;
#if UseNA
        [OnValueChanged(nameof(UpdateToggles))]
#endif
        [SerializeField, Range(0,2)] private int currentSizeInt;

        private void OnEnable()
        {
            SettingsManager.WhenLoaded(EventSubscriptions);
			SettingsManager.SettingChanged += SettingsManagerSettingChanged;
		}

        private void OnDisable()
        {
            SettingsManager.SettingChanged -= SettingsManagerSettingChanged;
        }

        private void EventSubscriptions()
        {
            if (!SettingsManager.instance_Initialised) return;

            if(SettingsManager.instance.GetIntSetting(SettingType.FontSize, ref currentSizeInt))
            {
                UpdateToggles(currentSizeInt);
            }
        }

        private void SettingsManagerSettingChanged(Setting changedSetting)
        {
            if (changedSetting == null || changedSetting.mySettingType != SettingType.FontSize || changedSetting.myValueType != SettingValueType.Int) return;

            UpdateToggles(changedSetting.intValue);
        }

        private void UpdateToggles()
        {
            UpdateToggles(currentSizeInt);
        }

        private void UpdateToggles(int settingValue)
        {
            switch (settingValue)
            {
                case 0:
                    smallToggle?.SetIsOnWithoutNotify(true);
                    return;
                case 1:
                    defaultToggle?.SetIsOnWithoutNotify(true);
                    return;
                case 2:
                    largeToggle?.SetIsOnWithoutNotify(true);
                    return;
            }
        }

        public void SetSmallSize(bool isTrue)
        {
            if (isTrue && SettingsManager.instance_Initialised) SettingsManager.instance.ChangeSetting(SettingType.FontSize, 0);
        }

        public void SetDefaultSize(bool isTrue)
        {
            if (isTrue && SettingsManager.instance_Initialised) SettingsManager.instance.ChangeSetting(SettingType.FontSize, 1);
        }

        public void SetLargeSize(bool isTrue)
        {
            if (isTrue && SettingsManager.instance_Initialised) SettingsManager.instance.ChangeSetting(SettingType.FontSize, 2);
        }
    }
}
