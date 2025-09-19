using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;

namespace AltEnding.Settings
{
    public enum SettingType
	{
        None = 0,
		//Audio: 01-99
		MasterVolume = 1,
		MusicVolume = 2,
		SFXVolume = 3,

		//Graphics: 100-199
		FullScreen = 100,
		ResolutionX = 101,
		ResolutionY = 102,
		Vsync = 103,

		//Gameplay: 200-299
		FontSize = 200,
		[InspectorName("Opt-In to Data Tracking")]
		DataTrack = 201,

		//Controls: 400-499

		//Debug/Dev Settings: 1000+
	}

	public enum SettingValueType
    {
		Undefined = 0,
		Int = 1,
		Bool = 2,
		String = 3,
    }
    
	[Serializable]
	public class Setting
	{
		public SettingType mySettingType;
		public SettingValueType myValueType;
		public object valueObject;

		public Setting(SettingType type, SettingValueType settingValueType, object valueReference)
		{
			mySettingType = type;
			myValueType = settingValueType;
			valueObject = valueReference;
		}

		public Setting(SettingType type, object valueReference)
		{
			mySettingType = type;
			myValueType = SettingsManager.GetSettingValueType(valueReference.GetType());
			valueObject = valueReference;
		}

		public Setting(string inputJSONString)
		{
			if (string.IsNullOrEmpty(inputJSONString)) return;
			string[] strings = inputJSONString.Split("__");
			if (strings.Length == 3)
			{
				if(!Enum.TryParse(strings[0], out mySettingType))
				{
					return;
				}

				myValueType = SettingValueType.Undefined;

				if (strings[1].StartsWith("svt")) {
					int svtInt;
					if(int.TryParse(strings[1].Remove(0,3), out svtInt))
                    {
						myValueType = (SettingValueType)svtInt;
                    }
				}

				if (myValueType == SettingValueType.Undefined)
				{
					Type valueType = Type.GetType(strings[1], false);
					myValueType = SettingsManager.GetSettingValueType(valueType);
				}

                switch (myValueType)
                {
					case SettingValueType.Int:
						int newint;
						if (int.TryParse(strings[2], out newint))
						{
							valueObject = newint;
							return;
						}
						break;
					case SettingValueType.Bool:
						bool newBool;
						if (bool.TryParse(strings[2], out newBool))
						{
							valueObject = newBool;
							return;
						}
						break;
					case SettingValueType.String:
						valueObject = strings[2];
						return;
					default:
						//Last Ditch attempt to parse into a float
						float newfloat;
						if (float.TryParse(strings[2], out newfloat))
						{
							myValueType = SettingValueType.Undefined;
							valueObject = newfloat;
							return;
						}
						break;
				}

				//Somehow got past the switch statement, so save it as an undefined string?
				myValueType = SettingValueType.Undefined;
				valueObject = strings[2];
			}
		}

		public string ToJSONString ()
		{
			return new string($"{mySettingType}__svt{((int)myValueType).ToString()}__{valueObject}");
		}

        #region Accessors
		public int intValue
        {
            get
            {
				if (myValueType == SettingValueType.Int) return (int)valueObject;
				else return 0;
            }
		}
		public string stringValue
        {
            get
            {
				if (myValueType == SettingValueType.String) return (string)valueObject;
				else return "";
			}
        }
		public bool boolValue
		{
			get
			{
				if (myValueType == SettingValueType.Bool) return (bool)valueObject;
				else return false;
			}
		}
		#endregion
	}

    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
		public static event SingletonInstanceInitialized settingsManagerInstanceInitialized;
		private static event Action SettingsLoadedAction;
		public static Action<Setting> SettingChanged;
		public static Action settingsSaving;
		public static Action settingsSaved;
		public static Action settingsLoading;
		public static Action<bool> settingsLoaded;
		public static Action settingsReset;

		public static void WhenLoaded(Action executeWhenLoaded)
		{
			if (instance_Initialised && instance.initialized)
			{
				executeWhenLoaded?.Invoke();
			}
			else
			{
				SettingsLoadedAction += executeWhenLoaded;
			}
		}

		[SerializeField, UnityEngine.Serialization.FormerlySerializedAs("debug")] private bool doDebugs;
		public bool initialized { get; private set; }

		public UnityEngine.Audio.AudioMixer myAudioMixer;

		[SerializeField] protected SerializableDictionary<SettingType, Setting> currentSettings;

		Coroutine savingAndLoadingCoroutine;
        
		void Start()
		{
			ValidateCurrentSettingsDictionary(true);
			initialized = true;
			SettingsLoadedAction?.Invoke();
			SettingsLoadedAction = null;
			settingsManagerInstanceInitialized?.Invoke();
		}

		void ValidateCurrentSettingsDictionary(bool forceAnyways = false)
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Validate the list!"); return; }
			if (currentSettings == null || forceAnyways)
			{
				currentSettings = new SerializableDictionary<SettingType, Setting>();
				LoadSettings();
			}
		}

		#region Get Settings
		public Setting GetSetting(SettingType type)
		{
			ValidateCurrentSettingsDictionary();
			if (currentSettings.ContainsKey(type) && currentSettings[type].valueObject != null)
			{
				return currentSettings[type];
			}

			return null;
		}

		public bool GetSettingAsBool(SettingType type)
		{
			if (currentSettings.ContainsKey(type) && currentSettings[type].valueObject != null && currentSettings[type].myValueType == SettingValueType.Bool)
			{
				return (bool)currentSettings[type].valueObject;
			}
			else return false;
		}

		public bool GetBoolSetting(SettingType type, ref bool value)
		{
			if (!initialized || currentSettings == null || currentSettings.Count <= 0)
			{
				Debug.LogError($"Not initialized properly, could not get value for {type}", this);
				return false;
			}
			if (!currentSettings.ContainsKey(type))
			{
				Debug.LogError($"Settings list did not contain entry for {type}", this);
				return false;
			}
			if (currentSettings[type].myValueType != SettingValueType.Bool)
			{
				Debug.LogError($"{type} setting is set as {currentSettings[type].myValueType} instead of bool. Failure to return.", this);
				return false;
			}
			value = (bool)currentSettings[type].valueObject;
			return true;

		}

		public bool GetIntSetting(SettingType type, ref int value)
        {
			if (!initialized || currentSettings == null || currentSettings.Count <= 0)
			{
				Debug.LogError($"Not initialized properly, could not get value for {type}", this);
				return false;
			}
			if (!currentSettings.ContainsKey(type)) {
				Debug.LogError($"Settings list did not contain entry for {type}", this);
				return false;
			}
			if (currentSettings[type].myValueType != SettingValueType.Int) {
				Debug.LogError($"{type} setting is set as {currentSettings[type].myValueType} instead of int. Failure to return.", this);
				return false;
			}
			value = (int)currentSettings[type].valueObject;
			return true;

        }
		#endregion

		#region Change Settings
		public bool ChangeSetting(Setting setting)
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning($"Either Saving or Loading! Don't change {((setting != null) ? setting.mySettingType : "null setting")} in the list!"); return false; }
			EasyDebug("SettingsManager.ChangeSetting(" + setting.mySettingType.ToString() + ", " + setting.valueObject.ToString() + ")");
			ValidateCurrentSettingsDictionary();
			if (currentSettings.ContainsKey(setting.mySettingType))
			{
				currentSettings[setting.mySettingType].valueObject = setting.valueObject;

				if(currentSettings[setting.mySettingType].myValueType != setting.myValueType){ }

				SettingChanged?.Invoke(currentSettings[setting.mySettingType]);
				UpdateDependents();
				return true;
			}
			return false;
		}

		public bool ChangeSetting(SettingType type, int newValue)
		{
			return ChangeSetting(new Setting(type, SettingValueType.Int, newValue));
		}

		public bool ChangeSetting(SettingType type, bool newValue)
		{
			return ChangeSetting(new Setting(type, SettingValueType.Bool, newValue));
		}

		public bool ChangeSetting(SettingType type, string newValue)
		{
			return ChangeSetting(new Setting(type, SettingValueType.String, newValue));
		}
		#endregion

		void UpdateDependents()
		{
			//Update audio levels
			if (myAudioMixer != null)
			{
				myAudioMixer.SetFloat("MasterVolume", LinearVolumeToDeciblesVolume(currentSettings[SettingType.MasterVolume].intValue / 100f));
				myAudioMixer.SetFloat("MusicVolume", LinearVolumeToDeciblesVolume(currentSettings[SettingType.MusicVolume].intValue / 100f));
				myAudioMixer.SetFloat("SFXVolume", LinearVolumeToDeciblesVolume(currentSettings[SettingType.SFXVolume].intValue / 100f));
			}

			if (Screen.fullScreen != currentSettings[SettingType.FullScreen].boolValue)
			{
				Screen.fullScreen = currentSettings[SettingType.FullScreen].boolValue;
			}
		}

		#region Setting List generation
		private bool AddSetting(Setting newSetting)
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't add to the list!"); return false; }
			ValidateCurrentSettingsDictionary();
			if (!currentSettings.ContainsKey(newSetting.mySettingType))
			{
				currentSettings.Add(newSetting.mySettingType, newSetting);
				return true;
			}

			return false;
		}

		private void DefaultIndividualSetting(Setting setting)
        {
			if (!AddSetting(setting))
			{
				ChangeSetting(setting);
			}
		}

		/// <summary>
		/// Resets all settings to the default values. Invokes settingsReset when finished.
		/// </summary>
		[ContextMenu("Reset Settings")]
		public void ResetSettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Reset the list!"); return; }
			DefaultSettings();
			settingsReset?.Invoke();
		}

		public void DefaultSettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Default the list!"); return; }
			currentSettings.Clear();
			DefaultSoundSettings();
			DefaultGraphicsSettings();
			DefaultGameplaySettings();
			DefaultDevSettings();
		}

		public void DefaultSoundSettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Default the Audio list!"); return; }
			DefaultIndividualSetting(new Setting(SettingType.MasterVolume, 100));
			DefaultIndividualSetting(new Setting(SettingType.MusicVolume, 100));
			DefaultIndividualSetting(new Setting(SettingType.SFXVolume, 100));
		}

		public void DefaultGraphicsSettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Default the Graphics list!"); return; }
			Resolution[] resolutionList = Screen.resolutions;
			Resolution res;
			if (resolutionList != null && resolutionList.Length > 0)
			{
				res = Screen.resolutions.Last();
            }
            else
            {
				res = Screen.currentResolution;
            }
			DefaultIndividualSetting(new Setting(SettingType.ResolutionX, res.width));
			DefaultIndividualSetting(new Setting(SettingType.ResolutionY, res.height));
			DefaultIndividualSetting(new Setting(SettingType.FullScreen, true));
		}

		public void DefaultGameplaySettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Default the Gameplay list!"); return; }
			DefaultIndividualSetting(new Setting(SettingType.FontSize, 1));
			DefaultIndividualSetting(new Setting(SettingType.DataTrack, false));
		}

		public void DefaultDevSettings()
		{
			if (savingAndLoadingCoroutine != null) { Debug.LogWarning("Either Saving or Loading! Don't Default the Dev list!"); return; }
		}

		public void ExternalInvokeSettingsResetEvent()
		{
			settingsReset?.Invoke();
		}
		#endregion

		#region Saving & Loading
		/// <summary>
		/// Save the current settings to PlayerPrefs.  Invokes 'settingsSaving' event before starting, and send 'settingsSaved' event after.
		/// </summary>
		[ContextMenu("Save Settings")]
		public void SaveSettings()
		{
			EasyDebug("SettingsManager.SaveSettings()");
			if (savingAndLoadingCoroutine == null)
			{
				savingAndLoadingCoroutine = StartCoroutine(SaveSettingsCoroutine());
			}
		}

		IEnumerator SaveSettingsCoroutine()
		{
			settingsSaving?.Invoke();

			EasyDebug("Saving settings");

			//This initialises the xml doc, I guess
			XmlDocument xml = new XmlDocument();

			XmlElement root = xml.CreateElement("Settings");
			xml.AppendChild(root);

			//Save all the attributes
			foreach (SettingType type in currentSettings.Keys)
			{
				EasyDebug($"Saving setting '{type}' as {currentSettings[type].valueObject}");
				root.SetAttribute(type.ToString(), currentSettings[type].ToJSONString());
				yield return new WaitForEndOfFrame();
			}

			//This finishes, I think.
			StringWriter xmlStr = new StringWriter();
			xml.Save(xmlStr);

			//Save to playerprefs
			PlayerPrefs.SetString("Settings", xmlStr.ToString());
			PlayerPrefs.Save();

			settingsSaved?.Invoke();

			EasyDebug("Saved settings:\n" + xmlStr.ToString());
			savingAndLoadingCoroutine = null;
		}

		/// <summary>
		/// Load the settings saved to PlayerPrefs.  Invokes 'settingsLoading' event before starting, and send 'settingsLoaded' event with a bool denoting success after trying to load.
		/// </summary>
		[ContextMenu("Load Settings")]
		public void LoadSettings()
		{
			if (doDebugs) Debug.Log("SettingsManager.LoadSettings()");
			if (savingAndLoadingCoroutine == null)
			{
				DefaultSettings();
				//Keybinds Manager doesn't exist yet, uncomment this when it does.
				//if (KeybindsManager.instance != null) KeybindsManager.instance.LoadBindings();
				savingAndLoadingCoroutine = StartCoroutine(LoadSettingsCoroutine());
			}
		}

		IEnumerator LoadSettingsCoroutine()
		{
			settingsLoading?.Invoke();


			string strSave = PlayerPrefs.GetString("Settings", "");
			if (!string.IsNullOrEmpty(strSave))
			{
				EasyDebug("Loading settings:\n" + strSave);

				XmlDocument xml = new XmlDocument();
				xml.LoadXml(strSave);
				XmlElement save = (XmlElement)xml.SelectSingleNode("/Settings");

				string debugString = "Loading settings loop:";

				List<SettingType> sKeys = currentSettings.Keys.ToList();
				SettingType sType;
				//Load all the attributes
				for (int c = 0; c < sKeys.Count; c++)
				{
					sType = sKeys[c];
					if (save.HasAttribute(sType.ToString()))
					{
						currentSettings[sType] = new Setting(save.GetAttribute(sType.ToString()));
						if (doDebugs) debugString += $"\n'{sType}' as {currentSettings[sType].myValueType}";
					}
					yield return new WaitForEndOfFrame();
				}

				UpdateDependents();

                initialized = true;
				settingsLoaded?.Invoke(true);

				EasyDebug(debugString + "\n Loaded settings successfully");
			}
			else
			{
                initialized = true;
				settingsLoaded?.Invoke(false);
                
				EasyDebug("Failed to load settings; string was null or empty");
			}
			savingAndLoadingCoroutine = null;
		}
		#endregion

		#region Helpers & Utilities
		private void EasyDebug(string message)
        {
            if (doDebugs)
            {
				Debug.Log(message, this);
            }
        }

		protected float LinearVolumeToDeciblesVolume(float linearVolume)
		{
			if (linearVolume <= 0) return -80;
			else return Mathf.Log10(linearVolume) * 20;
		}

#if UseNA
		[NaughtyAttributes.Button()]
#endif
		protected void PrintCurrentSettings()
		{
			System.Text.StringBuilder message = new System.Text.StringBuilder("Current Settings:\n");

			foreach (KeyValuePair<SettingType, Setting> kvp in currentSettings)
			{
				message.AppendLine($"\t{kvp.Key}: {kvp.Value.valueObject}");
			}

			Debug.Log(message);
		}

		public static SettingValueType GetSettingValueType(object valueObject)
        {
			if (valueObject == null) return SettingValueType.Undefined;

			return GetSettingValueType(valueObject.GetType());
        }

		public static SettingValueType GetSettingValueType(Type type)
		{

			if (type == typeof(int))
			{
				return SettingValueType.Int;
			}
			else if (type == typeof(bool))
			{
				return SettingValueType.Bool;
			}
			else if (type == typeof(string))
			{
				return SettingValueType.String;
			}

			return SettingValueType.Undefined;
		}

		/// <summary>
		/// Convert the integer that is stored as a setting into the float used to sample a curve.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>A float between 0-1, used to sample a curve which stores potential modifier values.</returns>
		public static float FontSizeFloat(int input)
        {
			return Mathf.InverseLerp(0f, 2f, input);
        }

		/// <summary>
		/// Automatically retrieve the current font size setting already converted into a sample point value.
		/// </summary>
		/// <returns>A float between 0-1, used to sample a curve which stores potential modifier values.</returns>
		public float FontSizeValue()
        {
			int result = 1;
			if(!GetIntSetting(SettingType.FontSize, ref result))
            {
				return 0.5f;
            }
            else return FontSizeFloat(result);
        }

		public static AnimationCurve DefaultSizeModifierCurve()
        {
			Keyframe[] keys = new Keyframe[3];
			keys[0] = new Keyframe(0f, 0.9f);
			keys[1] = new Keyframe(0.5f, 1f);
			keys[2] = new Keyframe(1f, 1.1f);
			AnimationCurve result = new AnimationCurve(keys);
#if UNITY_EDITOR
			UnityEditor.AnimationUtility.SetKeyLeftTangentMode(result, 0, UnityEditor.AnimationUtility.TangentMode.Linear);
			UnityEditor.AnimationUtility.SetKeyRightTangentMode(result, 0, UnityEditor.AnimationUtility.TangentMode.Linear);
			UnityEditor.AnimationUtility.SetKeyLeftTangentMode(result, 1, UnityEditor.AnimationUtility.TangentMode.Linear);
			UnityEditor.AnimationUtility.SetKeyRightTangentMode(result, 1, UnityEditor.AnimationUtility.TangentMode.Linear);
			UnityEditor.AnimationUtility.SetKeyLeftTangentMode(result, 2, UnityEditor.AnimationUtility.TangentMode.Linear);
			UnityEditor.AnimationUtility.SetKeyRightTangentMode(result, 2, UnityEditor.AnimationUtility.TangentMode.Linear);
#endif
			return result;
        }
		#endregion
	}
}
