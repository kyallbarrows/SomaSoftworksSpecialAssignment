using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AltEnding.SaveSystem;
using System.Linq;

namespace AltEnding
{
	public class ArticyFlowHistoryTracker : MonoBehaviour, ISaveable, IProfileSaveable
	{
		public static Action<MapNodeData> NewCheckpointLogged;

		[SerializeField] protected List<string> visitedArticyObjects = new List<string>();
		[SerializeField] protected List<string> profileVisitedArticyObjects = new List<string>();
		[SerializeField] protected List<MapNodeData> mapNodeHistory;
		//Public accessor that gives a copy of the list so that the above won't be edited
		//by the requester, and the requester's list isn't changed by edits to the above
		public List<MapNodeData> MapNodeHistory { get { return new List<MapNodeData>(mapNodeHistory); } }
		public List<string> trackedLocalizedStrings = new List<string>();

		#region Instance Management
		public static ArticyFlowHistoryTracker Instance { get; private set; }
		public static bool instanceInitialized { get { return Instance != null; } }

		private void Awake()
		{
			if (Instance != null)
			{
				Debug.LogWarning("Found more than one Articy Flow History Tracker in the scene. Destroying the newest one.");
				Destroy(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
		}
		#endregion

		#region Event Listening
		private void OnEnable()
		{
			ArticyFlowController.NewFlowObject += ArticyFlowController_NewFlowObject;
		}

		private void OnDisable()
		{
			ArticyFlowController.NewFlowObject -= ArticyFlowController_NewFlowObject;
		}

        private void ArticyFlowController_NewFlowObject(IFlowObject newFlowObject)
        {
            //Record visited scene
            IArticyObject articyObj = newFlowObject as IArticyObject;
            if (articyObj != null)
            {
                string hexID = articyObj.Id.ToHex();
                if (!visitedArticyObjects.Contains(hexID)) visitedArticyObjects.Add(hexID);
                if (!profileVisitedArticyObjects.Contains(hexID)) profileVisitedArticyObjects.Add(hexID);
            }

            //Check for checkpoints
            CheckpointFeature checkpointFeature = ArticyStoryHelper.Instance.GetCheckpointFeature(newFlowObject);
            if (checkpointFeature == null)
                return;
            
            //We have reached a checkpoint
            //When loading a checkpoint, this flow object will already be in the mapNodeHistory list.  Make sure it's not before adding it.
            ulong articyID = ((ArticyObject)newFlowObject).Id;
            if (mapNodeHistory.Count == 0 || mapNodeHistory[mapNodeHistory.Count - 1].articyObjectID != articyID)
            {
                mapNodeHistory.Add(new MapNodeData(articyID, checkpointFeature));
                NewCheckpointLogged?.Invoke(mapNodeHistory[mapNodeHistory.Count - 1]);
            }

            if (ArticyFlowController.Instance != null)
            {
                ArticyFlowController.Instance.SaveCheckpointOnNextFlowPause();

                //Auto-continue is a checkpoint template is reached
                if (ArticyStoryHelper.Instance.IsCheckpoint(newFlowObject))
                {
                    StartCoroutine(ContinueFlowAfterCheckpoint());
                }
            }
            else
            {
                Debug.LogError("ERROR: ArticyFlowHistoryTracker received a NewFlowObject event when the ArticyFlowController instance is null.  Something is very wrong!");
            }
        }

        IEnumerator ContinueFlowAfterCheckpoint()
		{
			yield return null;
			ArticyFlowController.Instance.PlayFirstBranch();
		}
		#endregion

		#region External Information Access Functions
		public bool HasSceneBeenVisited(string articyHexID)
		{
			return visitedArticyObjects.Contains(articyHexID);
		}

		public bool HasSceneBeenVisitedByProfile(string articyHexID)
		{
			return profileVisitedArticyObjects.Contains(articyHexID);
		}

		/// <summary>
		/// Adds the term to the list of localization terms being tracked.
		/// </summary>
		/// <param name="localizationTerm">The term to track.</param>
		/// <returns>True if already on the list.</returns>
		public bool TrackLocalizationTerm(string localizationTerm)
        {
            if (trackedLocalizedStrings.Contains(localizationTerm))
            {
				return true;
            }
            else
            {
				trackedLocalizedStrings.Add(localizationTerm);
				return false;
            }
        }

		/// <summary>
		/// Checks to see if the term is already beeing tracked.
		/// </summary>
		/// <param name="localizationTerm"></param>
		/// <returns>True if the string is already present in the 'trackedLocalizedStrings' list.</returns>
		public bool HasLocalizedTextBeenSeen(string localizationTerm)
		{
			return trackedLocalizedStrings.Contains(localizationTerm);
		}
		#endregion

		#region ISaveable Implementation
		public void ResetData()
		{
			visitedArticyObjects.Clear();
			mapNodeHistory.Clear();
		}

		public void SaveData(SaveData data)
		{
			//Save visited scenes
			data.flowHistorySaveData.visitedScenes = new List<string>(visitedArticyObjects);
			data.flowHistorySaveData.trackedLocalizedStrings = new List<string>(trackedLocalizedStrings);

			//Save visited checkpoints
			List<FlowHistorySaveData.CheckpointSceneData> visitedCheckpointScenes = new List<FlowHistorySaveData.CheckpointSceneData>();
			foreach (MapNodeData mapNodeData in mapNodeHistory)
			{
				visitedCheckpointScenes.Add(new FlowHistorySaveData.CheckpointSceneData(mapNodeData));
			}
			data.flowHistorySaveData.visitedCheckpointScenes = visitedCheckpointScenes;
		}

		public void LoadData(SaveData data)
		{
			//Load visited scenes
			visitedArticyObjects = new List<string>(data.flowHistorySaveData.visitedScenes);
			trackedLocalizedStrings = new List<string>(data.flowHistorySaveData.trackedLocalizedStrings);
			mapNodeHistory.Clear();
            
			//Load visited checkpoints
			foreach (FlowHistorySaveData.CheckpointSceneData objectID in data.flowHistorySaveData.visitedCheckpointScenes)
			{
				mapNodeHistory.Add(new MapNodeData(objectID));
			}

			DoBackwardsCompatabilityThings();
		}
		#endregion

		#region IProfileSaveable Implementation
		public void ResetProfileData()
		{
			Debug.Log($"[AFHT] Resetting Profile Data");
			profileVisitedArticyObjects.Clear();
		}

		public void SaveProfileData(ProfileData data)
		{
			Debug.Log($"[AFHT] Saving Profile Data");
			data.flowHistoryProfileData.visitedScenes = new List<string>(profileVisitedArticyObjects);
		}

		public void LoadProfileData(ProfileData data)
		{
			Debug.Log($"[AFHT] Loading Profile Data");
			profileVisitedArticyObjects = new List<string>(data.flowHistoryProfileData.visitedScenes);

			DoBackwardsCompatabilityThings();
		}
		#endregion

		protected void DoBackwardsCompatabilityThings()
		{
			//Add any playthrough visited objects to the profile visited objects if they aren't present
			profileVisitedArticyObjects.AddRange(visitedArticyObjects.Except(profileVisitedArticyObjects));
		}
	}
}
