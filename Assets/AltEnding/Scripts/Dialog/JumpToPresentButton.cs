using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AltEnding.Dialog
{

	public class JumpToPresentButton : MonoBehaviour
	{

		[SerializeField]
		protected Button snapToBottomButton;
		[SerializeField]
		protected ScrollRect chatScrollRect;
		[SerializeField, Range(0f, 1f)]
		protected float normalizedThreshold;
		[SerializeField]
		private AnimationCurve lerpCurve;
		[SerializeField, Min(0f)]
		private float lerpTime; 
#if UseNA
        [NaughtyAttributes.ReadOnly]
#endif
		[SerializeField]
		private float lerpTimeLeft;
#if UseNA
        [NaughtyAttributes.ReadOnly]
#endif
		[SerializeField]
		private float oldLerpPosition;
		private Coroutine scrollingRouting;

		private void Start()
		{
			if (chatScrollRect != null) ScrollValueUpdated(chatScrollRect.normalizedPosition);
		}

		public void ScrollValueUpdated(Vector2 newCoord)
		{
			if (snapToBottomButton == null || chatScrollRect == null || chatScrollRect.content == null || chatScrollRect.viewport == null) return;
			snapToBottomButton.interactable = chatScrollRect.content.rect.height > chatScrollRect.viewport.rect.height && newCoord.y > normalizedThreshold;
		}

		public void ScrollFastToBottom()
		{
			//Instead of teleporting, do a speed scroll. Will need to implement a coroutine, but this can be polish later.
			if (chatScrollRect == null) return;
			if (scrollingRouting == null) scrollingRouting = StartCoroutine(LerpScrollRectTo(0));
		}

		private IEnumerator LerpScrollRectTo(float newPosition)
        {
			oldLerpPosition = chatScrollRect.verticalNormalizedPosition;
			lerpTimeLeft = lerpTime;
			while(lerpTimeLeft > 0)
            {
				chatScrollRect.verticalNormalizedPosition = Mathf.Lerp(oldLerpPosition, newPosition, lerpCurve != null ? lerpCurve.Evaluate(1f - (lerpTimeLeft / lerpTime)) : (1f - (lerpTimeLeft / lerpTime)));
				yield return new WaitForEndOfFrame();
				lerpTimeLeft -= Time.deltaTime;
            }

			chatScrollRect.verticalNormalizedPosition = newPosition;
			yield return new WaitForEndOfFrame();
			chatScrollRect.verticalNormalizedPosition = newPosition;

			scrollingRouting = null;
		}
	}
}
