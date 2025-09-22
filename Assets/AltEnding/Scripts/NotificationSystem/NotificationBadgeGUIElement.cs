using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Notifications
{
    public class NotificationBadgeGUIElement : MonoBehaviour
    {

		[Header("GUI Elements")]
#if UseNA
        [Label("Current Visual Status"), OnValueChanged("InspectorUpdateStatus")]
#endif
		[SerializeField] protected bool _currentVisualStatus;
#if UseNA
        [HideIf("legacyAnimationAssigned")]
#endif
        [SerializeField, Tooltip("Will turn this gameobject off/on to reflect the current visual status")] protected Image badgeImage;
		private bool imageAssigned => badgeImage != null;
#if UseNA
        [HideIf("imageAssigned")]
#endif
        [SerializeField, Tooltip("Will play animations when the current visual status changes.")] protected Animation animationComponent;
		private bool legacyAnimationAssigned => animationComponent != null;
#if UseNA
        [ShowIf("legacyAnimationAssigned"), Required]
#endif
        [SerializeField] private AnimationClip showingAnimation;
#if UseNA
        [ShowIf("legacyAnimationAssigned"), Required]
#endif
        [SerializeField] private AnimationClip hidingAnimation;
		public bool currentVisualStatus { get { return _currentVisualStatus; } }

		[SerializeField] private bool doDebugs;
		/// <summary>
		/// Used when interacting with the "currentVisualStatus" boolean in the inspector. Will trigger update functionality.
		/// </summary>
		protected void InspectorUpdateStatus()
		{
			UpdateStatus(_currentVisualStatus, true);
		}

		/// <summary>
		/// Handles updating the visuals to reflect the current notification status.
		/// </summary>
		/// <param name="newStatus">The new status to reflect.</param>
		/// <param name="forceUpdate">If false, only run the visual code if the new status is different than currentVisualStatus. If true, run the code regardless.</param>
		public void UpdateStatus(bool newStatus, bool forceUpdate = false)
		{
			if (!forceUpdate && newStatus == _currentVisualStatus) return;
			if (doDebugs) Debug.Log($"Updating Status of Notification Badge ({gameObject.name}). New Status ({newStatus}), Current Status ({_currentVisualStatus}), Update Forced: {forceUpdate}");
			if (imageAssigned)
			{
				badgeImage.gameObject.SetActive(newStatus);
			}
            if (legacyAnimationAssigned)
            {
				PlayAnimation(newStatus ? showingAnimation : hidingAnimation);
            }
			_currentVisualStatus = newStatus;
		}

		protected void PlayAnimation(AnimationClip clipToPlay)
        {
			if (animationComponent == null || clipToPlay == null) return;
            if (animationComponent.isPlaying)
            {
				animationComponent.CrossFade(clipToPlay.name, clipToPlay.length / 4f);
            }
            else
            {
				animationComponent.Play(clipToPlay.name);
            }
        }
	}
}
