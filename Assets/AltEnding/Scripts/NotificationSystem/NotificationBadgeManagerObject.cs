using System;
using System.Collections.Generic;
using UnityEngine;

namespace AltEnding.Notifications
{
	[CreateAssetMenu(fileName = "New NBMO", menuName = "ScriptableObjects/Notification Badge Manager")]
	public class NotificationBadgeManagerObject : ScriptableObject
    {
		/// <summary>
		/// Called when the current unread status changes.
		/// </summary>
		public Action<bool> UnreadStatusUpdated;
		/// <summary>
		/// Called when the count of unread objects changes.
		/// </summary>
		public Action<int> CountUpdated;
        
		[field: SerializeField
#if UseNA
        ,NaughtyAttributes.ReadOnly
#endif
        ] public bool hasUnreadNotifications { get; private set; }
		[SerializeField] private NotificationBadgeManagerObject parentBadgeManager;
		[SerializeField] private List<ScriptableObject> myUnreadObjects;


		private bool ValidateList()
		{
			if(myUnreadObjects == null || myUnreadObjects.Count < 0)
			{
				myUnreadObjects = new List<ScriptableObject>();
				return false;
			}else if(myUnreadObjects.Count > 0)
			{
				for (int c = myUnreadObjects.Count - 1; c >= 0; c--)
				{
					if (myUnreadObjects[c] == null)
					{
						myUnreadObjects.RemoveAt(c);
					}
				}
			}
			return true;
		}

		public int GetUnreadNotificationsCount()
		{
			if (!ValidateList()) return -1;

			return myUnreadObjects.Count;
		}

		public void AddUnreadNotification(ScriptableObject newSO)
		{
			ValidateList();

			if (!myUnreadObjects.Contains(newSO))
			{
				myUnreadObjects.Add(newSO);
				CountUpdated?.Invoke(myUnreadObjects.Count);
			}

			if (!hasUnreadNotifications)
			{
				hasUnreadNotifications = true;
				UnreadStatusUpdated?.Invoke(true);
				if (parentBadgeManager != null) parentBadgeManager.AddUnreadNotification(this);
			}
		}

		public void RemoveUnreadNotification(ScriptableObject oldSO)
		{
			if (!ValidateList() || !hasUnreadNotifications || myUnreadObjects.Count < 1) return;

			for(int c = myUnreadObjects.Count-1; c >= 0; c--)
			{
				if (myUnreadObjects[c] == oldSO) myUnreadObjects.RemoveAt(c);
			}
			CountUpdated?.Invoke(myUnreadObjects.Count);

			//We know hasUnreadNotifications is true because we got past the check at the top of this function, so we don't need to check it here
			if (myUnreadObjects.Count == 0)
			{
				hasUnreadNotifications = false;
				UnreadStatusUpdated?.Invoke(false);
				if (parentBadgeManager != null) parentBadgeManager.RemoveUnreadNotification(this);
			}
		}

		#region Resetting values changed in play mode
#if UNITY_EDITOR
		bool initialHasUnreadNotifications;
		List<ScriptableObject> initialUnreadObjects;

		private void OnEnable()
		{
			UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
		}

		private void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
		}

		private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
			{
				StoreInitialRuntimeValues();
			}
			else if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
			{
				RestoreInitialRuntimeValues();
			}
		}

		public void StoreInitialRuntimeValues()
		{
			initialHasUnreadNotifications = hasUnreadNotifications;
			initialUnreadObjects = new List<ScriptableObject>();
			foreach (ScriptableObject so in myUnreadObjects)
			{
				initialUnreadObjects.Add(so);
			}
		}

		public void RestoreInitialRuntimeValues()
		{
			hasUnreadNotifications = initialHasUnreadNotifications;
			myUnreadObjects = new List<ScriptableObject>();
			foreach (ScriptableObject so in initialUnreadObjects)
			{
				myUnreadObjects.Add(so);
			}
		}
#endif
		#endregion
	}
}
