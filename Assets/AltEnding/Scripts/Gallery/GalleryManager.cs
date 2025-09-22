using Articy.Unity;
using AltEnding.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Gallery
{
	public class GalleryManager : PersistentSingleton<GalleryManager>, IProfileSaveable
	{
		private GalleryManager() { } //Private constructor to prevent additional instances

		[SerializeField] protected List<string> unlockedCharacters;
		[SerializeField] protected List<string> unlockedLocations;

#if UNITY_EDITOR
		[Header("Debug Info")]
		[SerializeField] protected bool everythingUnlocked;
#endif

		private void OnEnable()
		{
			ArticyFlowController.NewFlowObject += ArticyFlowObject_NewFlowObject;
		}

		private void OnDisable()
		{
			ArticyFlowController.NewFlowObject -= ArticyFlowObject_NewFlowObject;
		}

		private void ArticyFlowObject_NewFlowObject(IFlowObject newFlowObject)
		{
            GalleryFeature galleryFeature = ArticyStoryHelper.Instance.GetGalleryFeature(newFlowObject);
            if (galleryFeature == null)
                return;

            // TODO: Check incoming Articy flow objects for a trigger to unlock Gallery entries
		}

		#region Characters
		public bool UnlockCharacter(string characterArticyID)
		{
			if (string.IsNullOrWhiteSpace(characterArticyID))
                return false;
            
			if (unlockedCharacters != null && !unlockedCharacters.Contains(characterArticyID))
			{
				unlockedCharacters.Add(characterArticyID);
				return true;
			}
            
			return false;
		}

		public bool IsCharacterUnlocked(string characterArticyID)
		{
			if (string.IsNullOrWhiteSpace(characterArticyID))
                return false;
#if UNITY_EDITOR
			if (everythingUnlocked)
                return true;
#endif
			return unlockedCharacters != null && unlockedCharacters.Contains(characterArticyID);
		}
		#endregion

		#region Locations
		public bool UnlockLocationWithSceneName(string sceneName)
		{ 
			return UnlockLocation(ArticyStoryHelper.Instance.ConvertSceneNameToArticyID(sceneName));
		}

		public bool UnlockLocation(string locationArticyID)
		{
			if (string.IsNullOrWhiteSpace(locationArticyID))
                return false;
            
			if (unlockedLocations != null && !unlockedLocations.Contains(locationArticyID))
			{
				unlockedLocations.Add(locationArticyID);
				return true;
			}
            
			return false;
		}

		public bool IsLocationUnlocked(string locationArticyID)
		{
			if (string.IsNullOrWhiteSpace(locationArticyID))
                return false;
#if UNITY_EDITOR
			if (everythingUnlocked)
                return true;
#endif
			return unlockedLocations != null && unlockedLocations.Contains(locationArticyID);
		}
		#endregion

		#region IProfileSavable Implementation
		private void Reset()
        {
			ResetProfileData();
        }

#if UseNA
		[Button("Reset Profile Data")]
#endif
        public void ResetProfileData()
        {
            unlockedCharacters = new List<string>();
			unlockedLocations = new List<string>();
		}

		public void SaveProfileData(ProfileData data)
		{
			data.galleryProfileData = new GalleryProfileData(unlockedCharacters, unlockedLocations);
		}
        
		public void LoadProfileData(ProfileData data)
		{
			if (data.galleryProfileData != null)
			{
				unlockedCharacters = new List<string>(unlockedCharacters.Union(data.galleryProfileData.unlockedCharacters));
				unlockedLocations = new List<string>(unlockedLocations.Union(data.galleryProfileData.unlockedLocations));
			}
			else
			{
				ResetProfileData();
			}	
		}

		[System.Serializable]
		public class GalleryProfileData
		{
			public List<string> unlockedCharacters;
			public List<string> unlockedLocations;

			public GalleryProfileData()
            {
                unlockedCharacters = new List<string>();
				unlockedLocations = new List<string>();
			}

			public GalleryProfileData(List<string> unlockedCharacters, List<string> unlockedLocations)
			{
				this.unlockedCharacters = unlockedCharacters;
				this.unlockedLocations = unlockedLocations;
			}
		}
		#endregion
	}
}
