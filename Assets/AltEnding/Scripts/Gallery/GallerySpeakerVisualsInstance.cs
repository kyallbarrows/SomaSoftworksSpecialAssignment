using UnityEngine;
using Articy.Unity;

namespace AltEnding.Gallery
{
    public class GallerySpeakerVisualsInstance : SpeakerVisualsInstance
    {
		[Header("Gallery Specific Info")]
		[SerializeField] private GameObject nextAnimButton;
		[SerializeField] private GameObject prevAnimButton;

		public void LoadCharacter(ArticyObject character)
		{
			StartLoadingProcess(character);
		}

		public void NextExpression()
        {
            var expressions = ArticyStoryHelper.Instance.GetAllSpeakerExpressionDescriptions();
			int idx = expressions.IndexOf(currentExpression) + 1;
			if (idx >= expressions.Count) idx = 1;
			UpdateExpression(expressions[idx]);
		}

		public void PrevExpression()
		{
            var expressions = ArticyStoryHelper.Instance.GetAllSpeakerExpressionDescriptions();
            int idx = expressions.IndexOf(currentExpression) - 1;
			if (idx < 1) idx = expressions.Count - 1;
			UpdateExpression(expressions[idx]);
		}

		protected override void ExtractPortraitPackage()
		{
			if (currentDPP == null) return;

			currentState = SpeakingStates.None;
			portraitImage.sprite = currentDPP.staticAvatar;

			UpdateExpression("Neutral");
			SetDisplayName(currentDPP.displayName);
			ParticipantJoins(SpeakingStates.Speaking);

			nextAnimButton.gameObject.SetActive(true);
			prevAnimButton.gameObject.SetActive(true);
		}

		public void DisplayCharacterWithManualAvatar(Sprite avatarSprite)
		{
			portraitImage.sprite = avatarSprite;
			nextAnimButton.gameObject.SetActive(false);
			prevAnimButton.gameObject.SetActive(false);
		}

		protected override void Joined()
		{
			currentState = SpeakingStates.Speaking;
			UpdateExpression("Neutral");
			JoinCompleted?.Invoke();
		}
	}
}
