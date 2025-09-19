using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using AltEnding.SaveSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AltEnding
{

	[Serializable]
	public class MapNodeData
	{
		public ulong articyObjectID;
		public string description;
		public string locationSceneName;
		public string foregroundArticyHexID;
		public string backgroundArticyHexID;
#if UseNA
		[NaughtyAttributes.ShowAssetPreview]
#endif
		public Sprite foregroundSprite;
        
        private AsyncOperationHandle<IList<IResourceLocation>> loadDPPLocationsHandle;
        private AsyncOperationHandle<DialogPortraitPackage> loadDPPHandle;
        
		/// <summary>
		/// This function is used when generating node data during normal gameplay, from an articy flow object.
		/// </summary>
		/// <param name="articyObjectID">The objectID of the flow node.</param>
		/// <param name="checkpointFeature">the Articy feature info for checkpoints.</param>
		public MapNodeData(ulong articyObjectID, CheckpointFeature checkpointFeature)
		{
            this.articyObjectID = articyObjectID;

			if (checkpointFeature == null) return;

			ArticyObject parentObject = ArticyDatabase.GetObject(articyObjectID);
            string background = ArticyStoryHelper.Instance.GetBackgroundDescription(parentObject);

			description = checkpointFeature.description;

			//Current location
			if (SceneManagementSingleton.instance_Initialised)
			{
				if (background.Equals("Dont_Change"))
				{
					locationSceneName = SceneManagementSingleton.instance.CurrentLocationScene;
				}
				else
				{
					locationSceneName = background;
				}
			}

			//Foreground sprite
			if (checkpointFeature.foregroundArticyHexID != null)
			{
				foregroundArticyHexID = checkpointFeature.foregroundArticyHexID;
			}
			else
			{
				if (parentObject != null && parentObject is IObjectWithSpeaker && ((IObjectWithSpeaker)parentObject).Speaker != null)
				{
					foregroundArticyHexID = ((IObjectWithSpeaker)parentObject).Speaker.Id.ToHex();
                }
                else
                {
					foregroundArticyHexID = "";
                }
			}

			//Background sprite
			if(checkpointFeature.backgroundArticyHexID != null)
			{
				backgroundArticyHexID = checkpointFeature.backgroundArticyHexID;
            }
            else
            {
				if(SceneManagementSingleton.instance_Initialised)
                {
					if (background.Equals("Dont_Change"))
					{
						//Get the current scene as an articyHexID and save it in the background field
						backgroundArticyHexID = SceneManagementSingleton.instance.GetLocationDataArticyHexIDForCurrentScene();
						
                    }
                    else
                    {
						//Get the destination scene as an articyHexID and save it in the background field
						backgroundArticyHexID = SceneManagementSingleton.instance.GetLocationDataArticyHexIDForScene(background);
					}
                }
			}
		}
        
		/// <summary>
		/// This function is used when generating node data during the loading process, from saved data.
		/// </summary>
		/// <param name="checkpointSceneData">The saved data formated for easy retrieval.</param>
		public MapNodeData(FlowHistorySaveData.CheckpointSceneData checkpointSceneData)
		{
			articyObjectID = checkpointSceneData.articyObjectID;
			foregroundArticyHexID = checkpointSceneData.foregroundArticyHexID;
			backgroundArticyHexID = checkpointSceneData.backgroundArticyHexID;
			ArticyObject aObject = ArticyDatabase.GetObject(articyObjectID);
            
            CheckpointFeature checkpointFeature = ArticyStoryHelper.Instance.GetCheckpointFeature(aObject);
			if (checkpointFeature != null)
				description = checkpointFeature.description;
            
			locationSceneName = checkpointSceneData.locationSceneName;
		}

		/// <summary>
		/// Run through the process to try and convert the foregroundArticyHexID into a sprite.
		/// </summary>
		/// <returns>True if a sprite was found or is already valid. False if unable to find a sprite; Check debug logs for errors.</returns>
		public bool FindForegroundSprite()
		{
			if (foregroundSprite != null)
			{
				Debug.Log($"The foreground sprite is already set: {foregroundSprite.name}");
				return true;
			}

			if (foregroundArticyHexID.IsNullOrWhiteSpace())
			{
				Debug.LogWarning($"foregroundArticyHexID is either null, empty, or whitespace, therefore will be unable to find any asset as all data is functionally null.");
				return false;
			}
            
            string address = DialogPortraitPackage.GetAddressableAddress(foregroundArticyHexID);
            loadDPPLocationsHandle = Addressables.LoadResourceLocationsAsync(address);
            loadDPPLocationsHandle.Completed += LoadDppLocationsHandleCompleted;

			Debug.Log($"All attempts to find an image for ArticyHexID '{foregroundArticyHexID}' have failed.");
			return false;
		}
        
        private void LoadDppLocationsHandleCompleted(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            loadDPPLocationsHandle.Completed -= LoadDppLocationsHandleCompleted;
            if (obj.Status != AsyncOperationStatus.Succeeded)
                return;

            var dPPLocations = obj.Result;
            if (dPPLocations.Count <= 0)
                return;

            loadDPPHandle = Addressables.LoadAssetAsync<DialogPortraitPackage>(dPPLocations[0]);
            loadDPPHandle.Completed += OnDPPLoadComplete;
        }

        private void OnDPPLoadComplete(AsyncOperationHandle<DialogPortraitPackage> obj)
        {
            loadDPPHandle.Completed -= OnDPPLoadComplete;
            if (obj.Status != AsyncOperationStatus.Succeeded)
                return;

            var dpp = obj.Result;
            foregroundSprite = dpp.staticAvatar;
        }
	}
}
