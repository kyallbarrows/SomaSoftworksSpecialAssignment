#if UseNA
using NaughtyAttributes;
#endif
using AltEnding.GUI;
using AltEnding.SaveSystem;
using AltEnding.Settings;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using Unity.Services.Core.Environments;

namespace AltEnding.Analytics
{
    public class AnalyticsManager : PersistentSingleton<AnalyticsManager>, ISaveable
    {
		public static bool instance_Loaded
		{
			get
			{
				return (instance_Initialised && instance.globalData != null);
			}
		}
		private static event Action instanceLoadedAction;
		public static void WhenLoaded(Action executeWhenLoaded)
		{
			if (instance_Loaded)
			{
				executeWhenLoaded?.Invoke();
			}
			else
			{
				instanceLoadedAction += executeWhenLoaded;
			}
		}

#if UseNA
        [ReadOnly]
#endif
		[SerializeField] protected AnalyticsSaveData saveData;
#if UseNA
        [ReadOnly]
#endif
		[SerializeField] protected GlobalAnalyticsData globalData;

		private const string globalSavePlayerPrefsKey = "AnalyticsGlobalSaveData";

		private readonly List<string> backgroundsToIgnore = new List<string>() { };

        // Environment name for initializing Unity Services
        private const string ENVIRONMENT_NAME = "development";

		private void OnEnable()
		{
			ArticyFlowController.SpecialActionObjectReached += ArticyFlowController_SpecialActionObjectReached;
			ArticyFlowController.ArticyAnalyticEventTriggered += ArticyFlowController_ArticyAnalyticEventTriggered;
			SpeakerVisualsInstance.NewCharacter += SpeakerVisualsInstance_NewCharacter;
			SceneManagementSingleton.newLocationLoaded += SceneManagementSingleton_newLocationLoaded;
			SettingsManager.SettingChanged += SettingsManager_SettingChanged;
			SettingsManager.settingsLoaded += SettingsManager_SettingsLoaded;
		}

		private void OnDisable()
		{
			ArticyFlowController.SpecialActionObjectReached -= ArticyFlowController_SpecialActionObjectReached;
			ArticyFlowController.ArticyAnalyticEventTriggered -= ArticyFlowController_ArticyAnalyticEventTriggered;
			SpeakerVisualsInstance.NewCharacter -= SpeakerVisualsInstance_NewCharacter;
			SceneManagementSingleton.newLocationLoaded -= SceneManagementSingleton_newLocationLoaded;
			SettingsManager.SettingChanged -= SettingsManager_SettingChanged;
			SettingsManager.settingsLoaded -= SettingsManager_SettingsLoaded;
		}

		#region Listeners
		private void ArticyFlowController_SpecialActionObjectReached(string specialActionType)
		{
			Debug.Log($"[AM] ArticyFlowController_SpecialActionObjectReached called");

			switch (specialActionType)
			{
				case "End_Of_Game":
					AnalyticsEvent_EndEpisode endEpisodeEvent = new AnalyticsEvent_EndEpisode();
					SendAnalyticsEvent(endEpisodeEvent);

					AnalyticsEvent_StoryComplete storyCompleteEvent = new AnalyticsEvent_StoryComplete();
					storyCompleteEvent.FirstTimeOnDevice = !globalData.storyCompleted;
					SendAnalyticsEvent(storyCompleteEvent);

					globalData.storyCompleted = true;
					break;
				case "Enter_Episode_2":
					if (!globalData.episodeVisits.ContainsKey(specialActionType))
					{
						globalData.episodeVisits.Add(specialActionType, 1);
					}
					else globalData.episodeVisits[specialActionType]++;

					AnalyticsEvent_EnterEpisode2 ep2Event = new AnalyticsEvent_EnterEpisode2();
					ep2Event.FirstTimeOnDevice = globalData.episodeVisits[specialActionType] == 1;
					SendAnalyticsEvent(ep2Event);
					break;
                
				case "Enter_Episode_3":
					if (!globalData.episodeVisits.ContainsKey(specialActionType))
					{
						globalData.episodeVisits.Add(specialActionType, 1);
					}
					else globalData.episodeVisits[specialActionType]++;

					AnalyticsEvent_EnterEpisode3 ep3Event = new AnalyticsEvent_EnterEpisode3();
					ep3Event.FirstTimeOnDevice = globalData.episodeVisits[specialActionType] == 1;
					SendAnalyticsEvent(ep3Event);
					break;
				case "Enter_Episode_4":
					if (!globalData.episodeVisits.ContainsKey(specialActionType))
					{
						globalData.episodeVisits.Add(specialActionType, 1);
					}
					else globalData.episodeVisits[specialActionType]++;

					AnalyticsEvent_EnterEpisode4 ep4Event = new AnalyticsEvent_EnterEpisode4();
					ep4Event.FirstTimeOnDevice = globalData.episodeVisits[specialActionType] == 1;
					SendAnalyticsEvent(ep4Event);
					break;
				case "Enter_Episode_5":
					if (!globalData.episodeVisits.ContainsKey(specialActionType))
					{
						globalData.episodeVisits.Add(specialActionType, 1);
					}
					else globalData.episodeVisits[specialActionType]++;

					AnalyticsEvent_EnterEpisode5 ep5Event = new AnalyticsEvent_EnterEpisode5();
					ep5Event.FirstTimeOnDevice = globalData.episodeVisits[specialActionType] == 1;
					SendAnalyticsEvent(ep5Event);
					break;
				default:
					break;
			}
		}
        
		private void ArticyFlowController_ArticyAnalyticEventTriggered(string aEvent)
		{
			Debug.Log($"[AM] ArticyFlowController_ArticyAnalyticEventTriggered called");
			switch (aEvent)
            {
				case "End_Ep":
					AnalyticsEvent_EndEpisode endEpEvent = new AnalyticsEvent_EndEpisode();
					if (ArticyFlowController.InstanceAssigned) endEpEvent.EpisodeNumber = (int)ArticyFlowController.Instance.currentEpisodeNumber;
					SendAnalyticsEvent(endEpEvent);
					break;
				case "Enter_Ep_1":
					AnalyticsEvent_EnterEpisode1 enterEp1Event = new AnalyticsEvent_EnterEpisode1();
					SendAnalyticsEvent(enterEp1Event);
					break;
				case "Epilogue":
					AnalyticsEvent_BeginEpilogue beginEpilogueEvent = new AnalyticsEvent_BeginEpilogue();
					if (ArticyFlowController.InstanceAssigned) beginEpilogueEvent.EpisodeNumber = (int)ArticyFlowController.Instance.currentEpisodeNumber;
					SendAnalyticsEvent(beginEpilogueEvent);
					break;
				case "Story_Complete":
					AnalyticsEvent_StoryComplete storyCompleteEvent = new AnalyticsEvent_StoryComplete();
					storyCompleteEvent.FirstTimeOnDevice = !globalData.storyCompleted;
					SendAnalyticsEvent(storyCompleteEvent);
					globalData.storyCompleted = true;
					break;
				default:
					break;
            }
		}

		private void SpeakerVisualsInstance_NewCharacter(SpeakerVisualsInstance sVI)
		{
			if (sVI == null || string.IsNullOrWhiteSpace(sVI.currentDPPArticyHexID)) return;

			if (!globalData.charactersMet.TryAdd(sVI.currentDPPArticyHexID, 1))
			{
				globalData.charactersMet[sVI.currentDPPArticyHexID]++;
			}

			AnalyticsEvent_CharacterMet aEvent = new AnalyticsEvent_CharacterMet();
			aEvent.FirstTimeOnDevice = globalData.charactersMet[sVI.currentDPPArticyHexID] == 1;
			if (ArticyFlowController.InstanceAssigned) aEvent.EpisodeNumber = (int)ArticyFlowController.Instance.currentEpisodeNumber;
			aEvent.CharacterName = sVI.currentDisplayName;
			aEvent.TriggerSource = sVI.loadingSource;
			SendAnalyticsEvent(aEvent);
		}

		private void SceneManagementSingleton_newLocationLoaded(string locName)
		{
			if (string.IsNullOrWhiteSpace(locName)) return;
			if (backgroundsToIgnore.Contains(locName)) return;

			if (!globalData.locVisits.TryAdd(locName, 1))
			{
				globalData.locVisits[locName]++;
			}

			if (!saveData.locVisits.TryAdd(locName, 1))
			{
				saveData.locVisits[locName]++;
			}

			AnalyticsEvent_LocationEntered aEvent = new AnalyticsEvent_LocationEntered();
			aEvent.LocationName = locName;
			aEvent.FirstTimeOnDevice = globalData.locVisits[locName] == 1;
			aEvent.NumTimesOnDevice = globalData.locVisits[locName];
			aEvent.FirstTimeInPlaythrough = saveData.locVisits[locName] == 1;
			aEvent.NumTimesInPlaythrough = saveData.locVisits[locName];
			aEvent.EpisodeNumber = (int)ArticyFlowController.Instance.currentEpisodeNumber;
			SendAnalyticsEvent(aEvent);
		}

		private void SettingsManager_SettingChanged(Setting setting)
		{
			if (setting != null && setting.mySettingType == SettingType.DataTrack)
			{
				globalData.optedIn = setting.boolValue;
				EnableDisableDataTracking();
			}
		}

		private void SettingsManager_SettingsLoaded(bool success)
		{
			if (success)
			{
				globalData.optedIn = SettingsManager.instance.GetSettingAsBool(SettingType.DataTrack);
				EnableDisableDataTracking();
			}
		}
		#endregion
        
		void Start()
        {
			LoadGlobalData();
			Initialize();
        }

		public override void OnDestroy()
		{
			base.OnDestroy();
			SaveGlobalData();
		}

		private async void Initialize()
		{
			var options = new InitializationOptions();
			options.SetEnvironmentName(ENVIRONMENT_NAME);
			await UnityServices.InitializeAsync(options);

			EnableDisableDataTracking();
		}

		protected void SendAnalyticsEvent(Unity.Services.Analytics.Event analyticsEvent)
		{
#if UNITY_EDITOR //|| True
			StringBuilder sb = new StringBuilder($"[AM] Pretend send Unity Analytics Event: {analyticsEvent.GetType().Name}");
			Debug.Log(sb, this);
#else
			AnalyticsService.Instance.RecordEvent(analyticsEvent);
#endif
		}

		public void ShowFirstOptInModal()
        {
			if (globalData == null)
			{
				WhenLoaded(ShowFirstOptInModal);
				return;
			}

			if (globalData.shownOptInModal) return;

			if(!SettingsManager.instance_Initialised || !SettingsManager.instance.initialized)
            {
				SettingsManager.WhenLoaded(ShowFirstOptInModal);
				return;
            }

            if (!GlobalConfirmationModal.instance_Initialised)
            {
				GlobalConfirmationModal.WhenInitialized(ShowFirstOptInModal);
				return;
            }

            const string message =
                "This game reports annoymous device\nand gameplay information to our servers\nin order to improve gameplay, fix bugs, and\nimprove the player experience. We never\ncollect anything that tracks back to you.\n\nYou can learn more on our Privacy Policy page.\n\nYou can change your choice in the settings.";
            const string header = "Allow Anonymous Data Collection?";
            const string confirm = "Allow";
            const string cancel = "Don't Allow";
			GlobalConfirmationModal.instance.ShowConfirmationPrompt(message, header, confirm, cancel, OptInModalReturn);
        }

		private void OptInModalReturn(bool optIn)
        {
			if (!SettingsManager.instance_Initialised) return;
			globalData.optedIn = optIn;
			SettingsManager.instance.ChangeSetting(SettingType.DataTrack, optIn);
			globalData.shownOptInModal = true;
			EnableDisableDataTracking();
        }

		private void EnableDisableDataTracking()
		{
			if (globalData != null && globalData.optedIn)
			{
				Debug.Log("[AM] Enabling Data Tracking");
				AnalyticsService.Instance.StartDataCollection();
			}
			else
			{
				Debug.Log("[AM] Disabling Data Tracking");
				AnalyticsService.Instance.StopDataCollection();
			}
		}

		#region ISaveable Implementation
		public void ResetData()
		{
			saveData.locVisits.Clear();
		}

		public void SaveData(SaveData data)
		{
			data.analyticsSaveData.locVisits.Clear();
			foreach (KeyValuePair<string, int> kvp in saveData.locVisits)
			{
				data.analyticsSaveData.locVisits[kvp.Key] = kvp.Value;
			}
		}

		public void LoadData(SaveData data)
		{
			saveData.locVisits.Clear();
			foreach (KeyValuePair<string, int> kvp in data.analyticsSaveData.locVisits)
			{
				saveData.locVisits[kvp.Key] = kvp.Value;
			}
		}
		#endregion

		#region Global Saving & Loading
		private void SaveGlobalData()
		{
			PlayerPrefs.SetString(globalSavePlayerPrefsKey, JsonUtility.ToJson(globalData, true));
		}

		private void LoadGlobalData()
		{
			if (PlayerPrefs.HasKey(globalSavePlayerPrefsKey))
			{
				globalData = JsonUtility.FromJson<GlobalAnalyticsData>(PlayerPrefs.GetString(globalSavePlayerPrefsKey));
			}
			else globalData = new GlobalAnalyticsData();
			instanceLoadedAction?.Invoke();
			instanceLoadedAction = null;
		}

		[Serializable]
		protected class GlobalAnalyticsData
		{
            public SerializableDictionary<string, int> episodeVisits;
			public SerializableDictionary<string, int> charactersMet;
			public SerializableDictionary<string, int> locVisits;
			public bool storyCompleted;
			public bool optedIn;
			public bool shownOptInModal;

			public GlobalAnalyticsData()
			{
                episodeVisits = new SerializableDictionary<string, int>()
                {
                    { "Enter_Episode_2", 0 },
                    { "Enter_Episode_3", 0 },
                    { "Enter_Episode_4", 0 },
                    { "Enter_Episode_5", 0 }
                };
				charactersMet = new SerializableDictionary<string, int>();
				locVisits = new SerializableDictionary<string, int>();
				storyCompleted = false;
				optedIn = false;
				shownOptInModal = false;
			}
		}
		#endregion

		#region Event Paramater Names
		const string episodeNumber_Name = "episodeNumber";
		const string firstTimeOnDevice_Name = "firstTimeOnDevice";
		const string sceneName_Name = "sceneName";
		const string triggerSource_Name = "triggerSource";
		#endregion

		#region Event Subclasses
		public class AnalyticsEvent_StoryComplete : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_StoryComplete() : base("storyComplete")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_BeginEpilogue : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_BeginEpilogue() : base("enterEpilogue")
			{
				EpisodeNumber = 0;
			}
            
			public int EpisodeNumber { set { SetParameter(episodeNumber_Name, value); } }
		}

		public class AnalyticsEvent_EndEpisode : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EndEpisode() : base("endEpisode")
			{
				EpisodeNumber = 0;
			}
            
			public int EpisodeNumber { set { SetParameter(episodeNumber_Name, value); } }
		}

		public class AnalyticsEvent_EnterEpisode1 : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EnterEpisode1() : base("enterEpisode1")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_EnterEpisode2 : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EnterEpisode2() : base("enterEpisode2")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_EnterEpisode3 : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EnterEpisode3() : base("enterEpisode3")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_EnterEpisode4 : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EnterEpisode4() : base("enterEpisode4")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_EnterEpisode5 : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_EnterEpisode5() : base("enterEpisode5")
			{
				FirstTimeOnDevice = false;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
		}

		public class AnalyticsEvent_LocationEntered : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_LocationEntered() : base("locationEntered")
			{
				FirstTimeOnDevice = false;
				NumTimesOnDevice = -1;
				FirstTimeInPlaythrough = false;
				NumTimesInPlaythrough = -1;
				LocationName = "";
				EpisodeNumber = -1;
			}

			public bool FirstTimeOnDevice { set { SetParameter("firstTimeOnDevice", value); } }
			public int NumTimesOnDevice { set { SetParameter("numTimesOnDevice", value); } }
			public bool FirstTimeInPlaythrough { set { SetParameter("firstTimeInPlaythrough", value); } }
			public int NumTimesInPlaythrough { set { SetParameter("numTimesInPlaythrough", value); } }
			public string LocationName { set { SetParameter("locationName", value); } }
			public int EpisodeNumber { set { SetParameter(episodeNumber_Name, value); } }
		}

		public class AnalyticsEvent_CharacterMet : Unity.Services.Analytics.Event
		{
			public AnalyticsEvent_CharacterMet() : base("characterMet")
			{
				CharacterName = "";
				EpisodeNumber = 0;
				FirstTimeOnDevice = false;
				TriggerSource = "";
			}

			public string CharacterName { set { SetParameter("characterName", !string.IsNullOrWhiteSpace(value) ? value : "Null"); } }
			public int EpisodeNumber { set { SetParameter(episodeNumber_Name, value); } }
			public bool FirstTimeOnDevice { set { SetParameter(firstTimeOnDevice_Name, value); } }
			public string TriggerSource { set { SetParameter(triggerSource_Name, !string.IsNullOrWhiteSpace(value) ? value : "Null"); } }
		}
		#endregion

		#region Editor Functionality
#if UNITY_EDITOR
#if UseNA
		[Button("Reset All Global Data")]
#endif
		private void ResetAllGlobalData()
		{
			ResetEpisodeVisits();
			ResetCharactersMet();
			ResetLOCVisits();
			ResetStoryCompleted();
		}

#if UseNA
		[Button("Reset Episode Visits")]
#endif
		private void ResetEpisodeVisits()
		{
			if (globalData != null) globalData.episodeVisits = new SerializableDictionary<string, int>();
		}
        
#if UseNA
		[Button("Reset Characters Met")]
#endif
		private void ResetCharactersMet()
        {
			if (globalData != null) globalData.charactersMet = new SerializableDictionary<string, int>();
        }
        
#if UseNA
		[Button("Reset Locations Visited")]
#endif
		private void ResetLOCVisits()
		{
			if (globalData != null) globalData.locVisits = new SerializableDictionary<string, int>();
		}
        
#if UseNA
		[Button("Reset Story Completed")]
#endif
		private void ResetStoryCompleted()
		{
			if (globalData != null) globalData.storyCompleted = false;
		}
#endif
		#endregion
	}
}
