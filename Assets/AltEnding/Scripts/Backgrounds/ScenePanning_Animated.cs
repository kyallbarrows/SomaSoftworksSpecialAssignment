using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
	[RequireComponent(typeof(Animation))]
	public class ScenePanning_Animated : ScenePanningBase
	{
#if UseNA
        [Required]
#endif
		[SerializeField]
		protected Animation animationComponent;

		[Header("Motion Variables")]
#if UseNA
        [Required("Panning Clip needs to be set!")]
#endif
		[SerializeField]
		protected AnimationClip panningClip;
        
		protected AnimationState currentState;
        
		private bool ClipSet(AnimationClip clip) { return clip != null; }

		private void OnValidate()
		{
			if (animationComponent == null) animationComponent = GetComponent<Animation>();
			if (!enabled) return;
			if (animationComponent.clip == null && panningClip != null)
			{
				animationComponent.clip = panningClip;
				if (animationComponent.GetClip(panningClip.name) == null) animationComponent.AddClip(panningClip, panningClip.name);
			}
			if (panningClip == null && animationComponent.clip != null)
			{
				panningClip = animationComponent.clip;
			}

			if (panningClip == null)
			{
				enabled = false;
				return;
			}

			if (currentState == null) GetState();
			if (blendingCurve == null) blendingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			if (Application.isPlaying)
			{
				UpdatePosition();
			}
		}

		private void GetState()
		{
			if (!animationComponent.isPlaying)
			{
				animationComponent.Play(panningClip.name);
			}
			foreach (AnimationState aState in animationComponent)
			{
				if (aState.clip == panningClip)
				{
					currentState = aState;
					break;
				}
			}
		}

		protected override void UpdatePosition()
		{
			if (!enabled) return;
			if (doDebugText)
			{
				Debug.Log($"Update the position!\nPosition value: {panningPosition}\nTime: {Time.time}");
			}

			if (currentState == null) GetState();

			if (currentState != null)
			{
				currentState.normalizedTime = panningPosition; //blendingCurve.Evaluate(panningPosition);
				currentState.speed = 0;
				currentState.enabled = true;
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					animationComponent.Sample();
				}
#endif
			}
		}
	}
}
