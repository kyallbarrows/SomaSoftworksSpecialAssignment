using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AltEnding.SaveSystem;

namespace AltEnding
{
	[RequireComponent(typeof(ArticyFlowPlayer))]
	public class ArticyFlowController : MonoBehaviour, IArticyFlowPlayerCallbacks, ISaveable
	{
		public static Action<IFlowObject> NewFlowObject;
		public static Action<IList<Branch>> NewChoices;
		public static Action<string> SpecialActionObjectReached;
		public static Action<string> ArticyAnalyticEventTriggered;
		public static Action FlowStarting;
		public static Action FlowStarted;
		public static Action FlowEnded;
		public static Action EndOfOwnedContentReached;

		[SerializeField] protected ArticyRef startingArticyObject;

		[ContextMenuItem("Test Episode Function", "TestEpisodeNumberWithSkipNode")]
		[SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef skipToNode;
        
#if UseNA
        [NaughtyAttributes.Foldout("Episode References")]
#endif
        [SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef episode1Ref;
#if UseNA
        [NaughtyAttributes.Foldout("Episode References")]
#endif
        [SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef episode2Ref;
#if UseNA
        [NaughtyAttributes.Foldout("Episode References")]
#endif
        [SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef episode3Ref;
#if UseNA
        [NaughtyAttributes.Foldout("Episode References")]
#endif
        [SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef episode4Ref;
#if UseNA
        [NaughtyAttributes.Foldout("Episode References")]
#endif
        [SerializeField, ArticyTypeConstraint(typeof(IFlowObject))] protected ArticyRef episode5Ref;
		protected ArticyHierarchyManager hierarchyManager;

		// the flow player component found on this game object
		private ArticyFlowPlayer flowPlayer;

		IArticyObject lastArticyObject;
		public IArticyObject currentArticyObject { get; protected set; }

		[field: SerializeField
#if UseNA
        ,NaughtyAttributes.ReadOnly
#endif
        ]
		public EpisodeNumber currentEpisodeNumber { get; protected set; }

		bool saveCheckpointOnNextFlowPause = false;

		#region Instance Management
		public static ArticyFlowController Instance { get; private set; }
		public static bool InstanceAssigned { get { return Instance != null; } }

		private static event Action instanceInitializedAction;

		public static void WhenInitialized(Action executeWhenInitialized)
		{
			if (InstanceAssigned)
			{
				executeWhenInitialized?.Invoke();
			}
			else
			{
				instanceInitializedAction += executeWhenInitialized;
			}
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Debug.Log("Found more than one Articy Flow Controller in the scene. Destroying the newest one.");
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
		}
		#endregion

		[SerializeField] private bool doDebugs;

        private void OnEnable()
        {
			SaveManager.loaded += SetEpisodeNumberFromCurrentNode;
        }

        private void OnDisable()
        {
			SaveManager.loaded -= SetEpisodeNumberFromCurrentNode;
        }
        
        // Used to initialize our debug flow player handler.
        private void Start()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (flowPlayer == null)
			{
				// You could assign this via the inspector but this works equally well for our purpose.
				flowPlayer = GetComponent<ArticyFlowPlayer>();
				Debug.Assert(flowPlayer != null, "ArticyDebugFlowPlayer needs the ArticyFlowPlayer component!");
			}
			if (hierarchyManager != ArticyDatabase.ProjectHierarchy) hierarchyManager = ArticyDatabase.ProjectHierarchy;
		}

		// This is called every time the flow player reaches an object of interest.
        public void OnFlowPlayerPaused(IFlowObject aObject)
        {
            Debug.Log($"[AFC] OnFlowPlayerPause()\nFlow Object: {(aObject != null ? aObject : "null")}");

            if (aObject == null)
            {
                FlowEnded?.Invoke();
                return;
            }

            var analyticsEvents = ArticyStoryHelper.Instance.ParseArticyAnalyticsEvents(aObject);
            foreach (var analyticsEvent in analyticsEvents)
                SendEvent(analyticsEvent);

            var specialAction = ArticyStoryHelper.Instance.GetSpecialActionType(aObject);
            if (!string.IsNullOrWhiteSpace(specialAction))
            {
                SpecialActionObjectReached?.Invoke(specialAction);
                PerformSpecialAction(specialAction);
            }
            else if (ArticyStoryHelper.Instance.ShouldSkipFlow(aObject))
            {
                PlayFirstBranch();
            }

            NewFlowObject?.Invoke(aObject);

            if (aObject is IArticyObject articyObj)
            {
                lastArticyObject = currentArticyObject;
                currentArticyObject = articyObj;
            }

            ConditionallySave();
        }

        private void ConditionallySave()
        {
            //Save if we need to after everything has processed
            if (saveCheckpointOnNextFlowPause)
            {
                saveCheckpointOnNextFlowPause = false;
                if (currentArticyObject != null) SaveManager.Instance.SaveGame(currentArticyObject.Id.ToHex());
            }

            //The skipping behavior could potentially be improved later by having the managers for the systems that should skip it notify the AFC to skip
            bool skipCurrentSave = false;
            if (!skipCurrentSave && SaveManager.Initialized)
                SaveManager.Instance.SaveGame();
        }

        protected void SendEvent(string aEvent)
        {
			if (aEvent != "None") ArticyAnalyticEventTriggered?.Invoke(aEvent);
		}

		// Called every time the flow player encounters multiple branches, or paused on a node and wants to tell us how to continue.
		public void OnBranchesUpdated(IList<Branch> aBranches)
		{
			NewChoices?.Invoke(aBranches);
		}

		public void PlayBranch(Branch branch)
		{
			flowPlayer.Play(branch);
		}

		public bool PlayFirstBranch()
		{
			if (flowPlayer.AvailableBranches.Count > 0)
			{
				flowPlayer.Play(flowPlayer.AvailableBranches[0]);
				return true;
			}
			return false;
		}

		public void PlayID(string hexID)
		{
			if (doDebugs) Debug.Log($"[AFC] ArticyFlowController was told to play object with hex ID: {hexID}");
			PlayID(ArticyUtility.FromHex(hexID));
		}
        
		public void PlayID(ulong objectID)
		{
			if (doDebugs) Debug.Log($"[AFC] ArticyFlowController was told to play object with ulong ID: {objectID}");
			IArticyObject aObj = ArticyDatabase.GetObject(objectID);
			if (aObj != null)
			{
				PlayArticyObject(aObj);
			}
			else Debug.LogWarning($"[AFC] Articy database did not find object with ulong ID: {objectID}.  PlayID call was ignored.");
		}

		public void PlayArticyRef(ArticyRef articyRef)
		{
			PlayArticyObject(articyRef.GetObject());
		}

		public void PlayArticyObject(IArticyObject articyObject)
		{
			FlowStarting?.Invoke();
			flowPlayer.StartOn = articyObject;
			FlowStarted?.Invoke();
		}

#if UseNA
        [NaughtyAttributes.Button()]
#endif
		private void PlaySkipToNode()
        {
			if (skipToNode != null)
			{
				PlayArticyRef(skipToNode);
			}
        }

		public void GoBack()
		{
			//Not calling PlayArticyObject because we don't want to call the FlowStarted event (currently)
			flowPlayer.StartOn = lastArticyObject;
		}

		[ContextMenu("Reset Story")]
		void ResetStory()
		{
			flowPlayer.GlobalVariables.ResetVariables();
			PlayArticyRef(startingArticyObject);
		}

		public void CopyTargetLabel(BaseEventData aData)
		{
			var pointerData = aData as PointerEventData;
			if (pointerData != null)
				GUIUtility.systemCopyBuffer = pointerData.pointerPress.GetComponent<Text>().text;
			Debug.LogFormat("[AFC] Copied text \"{0}\" into clipboard!", GUIUtility.systemCopyBuffer);
		}

		public void SaveCheckpointOnNextFlowPause()
		{
			saveCheckpointOnNextFlowPause = true;
		}

		private void PerformSpecialAction(string specialActionType)
		{
			Debug.Log($"[AFC] Performing special action of type '{specialActionType}'");
			switch (specialActionType)
			{
				case "End_Of_Demo":
				case "End_Of_Game":
					SceneManagementSingleton.instance.LoadScene("End of Demo", UnityEngine.SceneManagement.LoadSceneMode.Single);
					FlowEnded?.Invoke();
					break;
				case "Enter_Episode_2":
                    PlayFirstBranch();
                    SafeSetCurrentEpisode(EpisodeNumber.Episode2);
					break;
				case "Enter_Episode_3":
                    PlayFirstBranch();
                    SafeSetCurrentEpisode(EpisodeNumber.Episode3);
					break;
				case "Enter_Episode_4":
                    PlayFirstBranch();
                    SafeSetCurrentEpisode(EpisodeNumber.Episode4);
					break;
				case "Enter_Episode_5":
                    PlayFirstBranch();
                    SafeSetCurrentEpisode(EpisodeNumber.Episode5);
					break;
			}
		}

        #region External Helper Functions
        #region Articy Variables
		public void AddArticyVariableChangedListener(string aVariable, Action<string, object> aNotificationFunc)
        {
			if (flowPlayer == null || flowPlayer.GlobalVariables == null || flowPlayer.GlobalVariables.Notifications == null) return;

			flowPlayer.GlobalVariables.Notifications.AddListener(aVariable, aNotificationFunc);
        }

		public void RemoveArticyVariableChangedListener(string aVariable, Action<string, object> aNotificationFunc)
		{
			if (flowPlayer == null || flowPlayer.GlobalVariables == null || flowPlayer.GlobalVariables.Notifications == null) return;

			flowPlayer.GlobalVariables.Notifications.RemoveListener(aVariable, aNotificationFunc);
		}

		public static bool GetArticyBoolean(string variableName)
        {
			if (string.IsNullOrWhiteSpace(variableName)
                || !InstanceAssigned
                || Instance.flowPlayer == null
                || Instance.flowPlayer.GlobalVariables == null
                || !Instance.flowPlayer.GlobalVariables.IsInitialized) return false;

			return Instance.flowPlayer.GlobalVariables.GetVariableByString<bool>(variableName);
        }

		private static List<string> _articyVariableNamesList;
		private static List<string> _articyBooleanNamesList;

		public static List<string> ArticyVariableNamesList()
        {
			_articyVariableNamesList = ArticyStoryHelper.Instance.GenerateArticyVariableNamesLists(_articyVariableNamesList);
			return _articyVariableNamesList;
        }

		public static List<string> ArticyBooleanNamesList()
        {
            _articyBooleanNamesList = ArticyStoryHelper.Instance.GenerateArticyBooleanNamesList(_articyVariableNamesList, _articyBooleanNamesList);
			return _articyBooleanNamesList;
        }
		#endregion

		public static string ValidateArticyObjectID(string objectID)
		{
			if (string.IsNullOrWhiteSpace(objectID)) return "";
			if (objectID.StartsWith("0x0")) return objectID.Replace("0x0", "0x");
			return objectID;
		}

		public static bool CheckCurrentEpisodeNumber(EpisodeNumber numberToCheck)
        {
			if (!InstanceAssigned)
			{
				Debug.LogWarning("No flow controller to compare episode number against.");
				return true;
			}
			if (Instance.currentEpisodeNumber == EpisodeNumber.None)
			{
				Debug.LogWarning("Current episode number is unassigned, therefore validation is unavailable.");
				return true;
			}
			return Instance.currentEpisodeNumber == numberToCheck;
		}

		public static bool CheckCurrentEpisodeNumber(EpisodeNumberFlags flagToCheck)
		{
			if (!InstanceAssigned)
			{
				Debug.LogWarning("No flow controller to compare episode number against.");
				return true;
			}
			if (Instance.currentEpisodeNumber == EpisodeNumber.None)
			{
				Debug.LogWarning("Current episode number is unassigned, therefore validation is unavailable.");
				return true;
			}
			return  flagToCheck.IsEpisodeFlagSet(Instance.currentEpisodeNumber);
		}

		public int GetEpisodeIntOfCurrentNode()
		{
				return (int)GetEpisodeNumberOfCurrentNode();
		}

		public EpisodeNumber GetEpisodeNumberOfCurrentNode()
		{
			if (currentArticyObject != null)
			{
				EpisodeNumber eNumber = GetEpisodeNumberForNodeByID(currentArticyObject.Id);
				SafeSetCurrentEpisode(eNumber);
				return eNumber;
			}
			else
			{
				EasyDebug("No current node");
				return EpisodeNumber.None;
			}
		}

		public EpisodeNumber GetCurrentEpisodeNumberFromNode()
		{
			SetEpisodeNumberFromCurrentNode();
			return currentEpisodeNumber;
		}

		private void SetEpisodeNumberFromCurrentNode()
		{
			if (currentArticyObject != null)
			{
				EpisodeNumber eNumber = GetEpisodeNumberForNodeByID(currentArticyObject.Id);
				SafeSetCurrentEpisode(eNumber);
				if(eNumber == EpisodeNumber.None)
				{
					Debug.LogWarning("Current node wasn't node but we still wewn't able to determine the episode number.", this);
                }
                else
                {
					EasyDebug($"Current Episode number: {currentEpisodeNumber}");
                }
			}
			else
			{
				Debug.LogWarning("No current node to set episode number by", this);
			}
		}

		private bool SetCurrentEpisodeNumberByObject(IArticyObject articyObject)
        {
			if (articyObject == null) return false;

			return SetCurrentEpisodeNumberByID(articyObject.Id);
		}

        private bool SetCurrentEpisodeNumberByID(ulong articyObjectID)
        {
            EpisodeNumber result = GetEpisodeNumberForNodeByID(articyObjectID);
            if (result == EpisodeNumber.None)
                return false;

            SafeSetCurrentEpisode(result);
            return true;
        }

        private void SafeSetCurrentEpisode(EpisodeNumber newEpisodeNumber)
        {
            // If you don't want the episode set under certain conditions, apply those conditions here
            currentEpisodeNumber = newEpisodeNumber;
        }

		public EpisodeNumber GetEpisodeNumberForObject(IArticyObject articyObject)
        {
			if (articyObject == null) return EpisodeNumber.None;
			return GetEpisodeNumberForNodeByID(articyObject.Id);
        }

		public EpisodeNumber GetEpisodeNumberForNodeByID(ulong articyObjectID)
		{
			string debugString = $"Testing for episode by heirarchy of {articyObjectID}";
			if (hierarchyManager == null) hierarchyManager = ArticyDatabase.ProjectHierarchy;
			ArticyHierarchyNode nodeToTest = hierarchyManager.GetHierarchyInfo(articyObjectID);
			while (nodeToTest != null)
			{
				debugString += $"\nChecking node id: {nodeToTest.Id}...";
				if (nodeToTest.Id == hierarchyManager.ProjectNode.Id)
                {
					EasyDebug(debugString + $"\nFailed; Ended with project node: {hierarchyManager.ProjectNode.Id}");
					return EpisodeNumber.None;
                }else if(episode1Ref.HasReference && nodeToTest.Id == episode1Ref.id)
				{
					EasyDebug(debugString + $"\nSuccessfully Ended with Episode 1 node.");
					return EpisodeNumber.Episode1;
                }else if(episode2Ref.HasReference && nodeToTest.Id == episode2Ref.id)
				{
					EasyDebug(debugString + $"\nSuccessfully Ended with Episode 2 node.");
					return EpisodeNumber.Episode2;
                }else if (episode3Ref.HasReference && nodeToTest.Id == episode3Ref.id)
				{
					EasyDebug(debugString + $"\nSuccessfully Ended with Episode 3 node.");
					return EpisodeNumber.Episode3;
				}else if (episode4Ref.HasReference && nodeToTest.Id == episode4Ref.id)
				{
					EasyDebug(debugString + $"\nSuccessfully Ended with Episode 4 node.");
					return EpisodeNumber.Episode4;
				}else if (episode5Ref.HasReference && nodeToTest.Id == episode5Ref.id)
				{
					EasyDebug(debugString + $"\nSuccessfully Ended with Episode 5 node.");
					return EpisodeNumber.Episode5;
                }else
                {
					nodeToTest = hierarchyManager.GetHierarchyInfo(nodeToTest.Parent);
                }

			}
			EasyDebug(debugString + "\nFailed: latest node was null.");
			return EpisodeNumber.None;
        }
		#endregion

		#region ISaveable Implementation
		public void ResetData()
		{
			flowPlayer.GlobalVariables.ResetVariables();
		}

		public void SaveData(SaveData data)
		{
			data.storySaveData.boolVariables.Clear();
			data.storySaveData.intVariables.Clear();
			data.storySaveData.stringVariables.Clear();

			StringBuilder debugString = new StringBuilder("[AFC] Saving Articy variables.");

			Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
			foreach (KeyValuePair<string, object> kvp in variables)
			{
				if (doDebugs) debugString.Append($"\nKey: {kvp.Key} : Value: {kvp.Value}");

				if (flowPlayer.GlobalVariables.IsVariableOfTypeBoolean(kvp.Key))
				{
					data.storySaveData.boolVariables.Add(kvp.Key, (bool)kvp.Value);
				}
				else if (flowPlayer.GlobalVariables.IsVariableOfTypeInteger(kvp.Key))
				{
					data.storySaveData.intVariables.Add(kvp.Key, (int)kvp.Value);
				}
				else if (flowPlayer.GlobalVariables.IsVariableOfTypeString(kvp.Key))
				{
					data.storySaveData.stringVariables.Add(kvp.Key, (string)kvp.Value);
				}
			}

			if (doDebugs) Debug.Log(debugString, this);

			data.storySaveData.articyObjectID = flowPlayer.CurrentObject.id;
			data.storySaveData.articyInstanceID = flowPlayer.CurrentObject.instanceId;
		}

		public void LoadData(SaveData data)
		{
			//Make sure the flow player is initialized.  It's possible that this is called before Start.
			Initialize();
			if (!flowPlayer.GlobalVariables.IsInitialized) flowPlayer.GlobalVariables.Init();

			StringBuilder debugString = new StringBuilder("[AFC] Loading bool values:");

			List<string> keyList = flowPlayer.GlobalVariables.Variables.Keys.ToList();
			Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
			foreach (KeyValuePair<string, bool> kvp in data.storySaveData.boolVariables)
			{
				if(doDebugs) debugString.Append($"\n{kvp.Key} as {kvp.Value}");
				if (keyList.Contains(kvp.Key)) variables[kvp.Key] = kvp.Value;
				else if (doDebugs) debugString.Append(" (ignoring because not in variables list)");
			}
			if (doDebugs) debugString.Append("\nLoading int values:");
			foreach (KeyValuePair<string, int> kvp in data.storySaveData.intVariables)
			{
				if (doDebugs) debugString.Append($"{kvp.Key} as {kvp.Value}");
				if (keyList.Contains(kvp.Key)) variables[kvp.Key] = kvp.Value;
				else if (doDebugs) debugString.Append(" (ignoring because not in variables list)");
			}
			if (doDebugs) debugString.Append("\nLoading string values:");
			foreach (KeyValuePair<string, string> kvp in data.storySaveData.stringVariables)
			{
				if (doDebugs) debugString.Append($"{kvp.Key} as {kvp.Value}");
				if (keyList.Contains(kvp.Key)) variables[kvp.Key] = kvp.Value;
				else if (doDebugs) debugString.Append(" (ignoring because not in variables list)");
			}
			if (doDebugs) Debug.Log(debugString, this);
			flowPlayer.GlobalVariables.Variables = variables;
			if (!SaveManager.loadDataOnly)
			{
				PlayArticyObject(ArticyDatabase.GetObject(data.storySaveData.articyObjectID, data.storySaveData.articyInstanceID));
			}
		}
		#endregion

		private void EasyDebug(string message)
        {
			if (doDebugs) Debug.Log("[AFC] " + message, this);
        }
	}
}
