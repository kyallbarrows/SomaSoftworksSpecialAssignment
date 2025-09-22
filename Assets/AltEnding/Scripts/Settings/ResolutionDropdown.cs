using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AltEnding.Settings
{
    public class ResolutionInfo
    {
        public int width;
        public int height;

        public ResolutionInfo()
		{
            width = 0;
            height = 0;
		}

        public ResolutionInfo(int width, int height)
		{
			this.width = width;
			this.height = height;
		}
	}

    public class ResolutionDropdown : MonoBehaviour
    {
        [SerializeField] protected AltEnding.GUI.GenericConfirmationModal confirmationModal;

        [SerializeField] protected TMP_Dropdown myDropdown;
        [SerializeField] protected List<TMP_Dropdown.OptionData> dropdownOptions;
        [SerializeField] protected List<ResolutionInfo> resolutionOptions;

#if UseNA
        [NaughtyAttributes.ReadOnly]
#endif
        [SerializeField] protected int currentResolutionIDX;
#if UseNA
        [NaughtyAttributes.ReadOnly]
#endif
        [SerializeField] protected int newResolutionIDX;

        [SerializeField] protected string confirmationTerm;

        private Coroutine initializationCoroutine;
        private bool delayedInitialization = false;

        bool isFullscreen 
        { 
            get 
            {
                if (SettingsManager.instance.initialized) return SettingsManager.instance.GetSettingAsBool(SettingType.FullScreen);
                else return true;
            } 
        }

        private void OnEnable()
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
			SettingsManager.settingsReset += SettingsManager_SettingsReset;
		}

		private void OnDisable()
		{
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            SettingsManager.settingsReset -= SettingsManager_SettingsReset;
        }

        private void SettingsManagerInstanceInitialized()
        {
            if (delayedInitialization)
            {
                delayedInitialization = false;
                SettingsManager.settingsManagerInstanceInitialized -= SettingsManagerInstanceInitialized;
            }
            Initialize();
        }

        private void SettingsManager_SettingsReset()
		{
            Initialize();
		}

        private void Initialize()
		{
            if (initializationCoroutine != null) StopCoroutine(initializationCoroutine);
            initializationCoroutine = StartCoroutine(InitializeCoroutine());
        }

        private IEnumerator InitializeCoroutine()
        {
            // Wait a frame to let SettingsManager initialize
            yield return null;

            Resolution[] resolutions = Screen.resolutions;
            if (resolutionOptions != null) resolutionOptions.Clear();
            resolutionOptions = new List<ResolutionInfo>();

            currentResolutionIDX = -1;

            //Get saved resolution
            int storedWidth = 0;
            int storedHeight = 0;
            Setting tempSetting = SettingsManager.instance.GetSetting(SettingType.ResolutionX);
            if (tempSetting != null && tempSetting.valueObject != null && tempSetting.myValueType == SettingValueType.Int)
            {
                storedWidth = tempSetting.intValue;
            }
            tempSetting = SettingsManager.instance.GetSetting(SettingType.ResolutionY);
            if (tempSetting != null && tempSetting.valueObject != null && tempSetting.myValueType == SettingValueType.Int)
            {
                storedHeight = tempSetting.intValue;
            }
            Debug.Log($"Saved resolution: {storedWidth}x{storedHeight}");

            //Set up dropdown options
            dropdownOptions = new List<TMP_Dropdown.OptionData>();
            TMP_Dropdown.OptionData newOption;
            ResolutionInfo newRes;
            for (int i = resolutions.Length - 1; i >= 0; i--)
            {
                newRes = new ResolutionInfo(resolutions[i].width, resolutions[i].height);
                if (!ResolutionExists(newRes))
                {
                    resolutionOptions.Add(newRes);

                    newOption = new TMP_Dropdown.OptionData(ResToString(newRes));
                    dropdownOptions.Add(newOption);
                    if (storedWidth == newRes.width && storedHeight == newRes.height)
                    {
                        currentResolutionIDX = dropdownOptions.Count - 1;
                    }
                }
            }

            //Populate Dropdown
            myDropdown.ClearOptions();
            myDropdown.AddOptions(dropdownOptions);
            if (currentResolutionIDX != -1)
            {
                myDropdown.SetValueWithoutNotify(currentResolutionIDX);
            }
            else
            {
                currentResolutionIDX = 0;
                myDropdown.SetValueWithoutNotify(0);
            }

            initializationCoroutine = null;
        }

        private bool ResolutionExists(ResolutionInfo checkRes)
        {
            return ResolutionExists(checkRes.width, checkRes.height);
        }

        private bool ResolutionExists(int width, int height)
        {
            bool exists = false;
            foreach (ResolutionInfo myRes in resolutionOptions)
            {
                if (myRes.width == width && myRes.height == height)
                {
                    exists = true;
                }
            }
            return exists;
        }

        private string ResToString(ResolutionInfo res)
        {
            return res.width + " x " + res.height;
        }

        public void DropdownOptionSelected(int selectedIDX)
		{
            newResolutionIDX = selectedIDX;
            ResolutionInfo resInfo = resolutionOptions[selectedIDX];
            Screen.SetResolution(resInfo.width, resInfo.height, isFullscreen);
            if (confirmationModal != null) confirmationModal.ShowConfirmationPrompt(confirmationTerm, ResolutionChangeConfirm);
        }

        public void ResolutionChangeConfirm(bool confirmed)
        {
            if (confirmed)
            {
                currentResolutionIDX = newResolutionIDX;
                SettingsManager.instance.ChangeSetting(SettingType.ResolutionX, resolutionOptions[currentResolutionIDX].width);
                SettingsManager.instance.ChangeSetting(SettingType.ResolutionY, resolutionOptions[currentResolutionIDX].height);
            }
            else
			{
                Screen.SetResolution(resolutionOptions[currentResolutionIDX].width, resolutionOptions[currentResolutionIDX].height, isFullscreen);
                myDropdown.SetValueWithoutNotify(currentResolutionIDX);
            }
        }
    }
}
