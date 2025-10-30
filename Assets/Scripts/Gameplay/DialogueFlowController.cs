using System.Collections.Generic;
using AltEnding;
using Articy.Unity;
using Articy.Unity.Interfaces;
using UnityEngine;

namespace SpecialAssignment
{
    public class DialogueFlowController : MonoBehaviour
    {
        public DialogueUIController dialogueUIController;
        public DirectorReferences directors;
        
        private DialogueMediaPlayer dialogueMediaPlayer;

        private void OnEnable()
        {
			ArticyFlowController.NewFlowObject += ArticyFlowController_NewFlowObject;
			ArticyFlowController.NewChoices += ArticyFlowController_NewChoices;
            ArticyFlowController.SpecialActionObjectReached += OnSpecialAction;
		}

		private void OnDisable()
		{
			ArticyFlowController.NewFlowObject -= ArticyFlowController_NewFlowObject;
			ArticyFlowController.NewChoices -= ArticyFlowController_NewChoices;
            ArticyFlowController.SpecialActionObjectReached -= OnSpecialAction;
		}
        
        private void Start()
        {
            ClearAllBranches();
            dialogueMediaPlayer = new(directors);
        }

        private void ArticyFlowController_NewFlowObject(IFlowObject aObject)
        {
            if (aObject == null)
            {
                Debug.LogWarning("New flow object is null");
                return;
            }

            var speakerName = ArticyStoryHelper.Instance.GetDisplayNameFromObject(aObject);
            dialogueUIController.ChangeSpeaker(speakerName);
            
            if (aObject is IObjectWithLocalizableText modelWithText)
                dialogueUIController.SetDialogueText(modelWithText.Text.Value);
            else
                dialogueUIController.SetDialogueText(string.Empty);
        }

        private void ArticyFlowController_NewChoices(IList<Branch> branches)
        {
			ClearAllBranches();

            if (branches.Count > 1)
            {
                foreach (var branch in branches)
                {
                    if (!branch.IsValid)
                        continue;

                    dialogueUIController.AddBranch(branch);
                }
            }
            else
            {
                bool isPlaying = dialogueMediaPlayer.IsPlaying();
                if (!isPlaying)
                    ArticyFlowController.Instance.PlayBranch(branches[0]);
            }
        }

        private void OnSpecialAction(string fullAction)
        {
            var actionParts = fullAction.Split('|');
            if (actionParts.Length < 8 || !actionParts[0].Equals("Scene"))
                return;
            
            string scene = actionParts[1];
            string cameraAngle = actionParts[3];
            string speaker = actionParts[5];
            string line = actionParts[7];

            string assetId = $"{scene}_{cameraAngle}_{speaker}_{line}";

            dialogueMediaPlayer.Play(assetId, speaker, line);
        }
        
		private void ClearAllBranches()
		{
			dialogueUIController.ClearBranches();
		}
    }
}
