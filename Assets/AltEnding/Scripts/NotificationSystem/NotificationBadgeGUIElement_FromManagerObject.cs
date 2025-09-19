using UnityEngine;

namespace AltEnding.Notifications
{
    public class NotificationBadgeGUIElement_FromManagerObject : NotificationBadgeGUIElement 
    {
		[Header("Badge Data")]
#if UseNA
        [NaughtyAttributes.Expandable]
#endif
        [SerializeField] private NotificationBadgeManagerObject myNBMObject;
		private bool initialized = false;

		private void OnEnable()
		{
			if(myNBMObject == null)
			{
				Debug.LogWarning($"NotificationBadgeGUIElement: {gameObject.name} has no Manager Object. Disabling.", this);
				UpdateStatus(false, true);
				enabled = false;
				return;
			}
			UpdateStatus(myNBMObject.hasUnreadNotifications);
			myNBMObject.UnreadStatusUpdated += UpdateStatus;
			initialized = true;
		}

		private void OnDisable()
		{
			if (myNBMObject == null) return;
			myNBMObject.UnreadStatusUpdated -= UpdateStatus;
		}

		/// <summary>
		/// Call the function to update the visual elements to reflect the new notification status. Passes the initialized value so that it only forces an update if the script hasn't been initialized yet.
		/// </summary>
		/// <param name="newStatus">The new notification status to attempt to show.</param>
		protected void UpdateStatus(bool newStatus)
		{
			UpdateStatus(newStatus, !initialized);
		}
	}
}
