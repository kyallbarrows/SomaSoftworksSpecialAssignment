using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AltEnding.Settings
{
    public class SettingsToggle : MonoBehaviour
    {

        public SettingType myType;
        [SerializeField] protected Toggle myToggle;
        [SerializeField] protected TextMeshProUGUI myLabel;
        [SerializeField] protected bool setLabelToTypeName;
        private bool delayedInitialization = false;

        private void OnValidate()
        {
            if(setLabelToTypeName && myLabel != null)
            {
                myLabel.text = myType.ToString();
            }
        }

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
			SettingsManager.settingsSaved += SettingsManager_settingsSaved;
			SettingsManager.settingsReset += SettingsManager_SettingsReset;
		}

        void OnDisable()
        {
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            SettingsManager.settingsLoaded -= SettingsLoaded;
            SettingsManager.settingsSaved -= SettingsManager_settingsSaved;
            SettingsManager.settingsReset -= SettingsManager_SettingsReset;
        }

        private void SettingsManager_settingsSaved()
        {
            UpdateToggle(SettingsManager.instance.GetSettingAsBool(myType));
        }

        private void SettingsManagerInstanceInitialized()
		{
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            SettingsLoaded(true);
        }

        void SettingsLoaded(bool loaded)
        {
            UpdateToggle(SettingsManager.instance.GetSettingAsBool(myType));
        }

        private void SettingsManager_SettingsReset()
        {
            SettingsLoaded(true);
        }

        public virtual void ToggleChanged(bool newValue)
        {
            SettingsManager.instance.ChangeSetting(myType, newValue);
        }

        protected virtual void UpdateToggle(bool newValue)
        {
            if (myToggle != null) myToggle.isOn = newValue;
        }
    }
}
