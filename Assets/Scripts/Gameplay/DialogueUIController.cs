using System.Collections.Generic;
using Articy.Unity;
using UnityEngine;

namespace SpecialAssignment
{
    public class DialogueUIController : MonoBehaviour
    {
        public GameObject allDialoguePanel;
        public Transform dialogueContent;
        public DialogueTextUI leftDialogueTextPrefab;
        public DialogueTextUI rightDialogueTextPrefab;
        public GameObject lastDialoguePanel;
        public GameObject dialogueOptionPanel;
        public List<DialogueOptionButton> dialogueOptionButtons;
        
        public void ClearBranches()
        {
            Debug.Log("[DialogueUIController] Clearing all branches");
        }

        public void AddBranch(Branch branch)
        {
            Debug.Log($"[DialogueUIController] Adding branch {branch.DefaultDescription}");
        }

        public void ChangeSpeaker(string speakerName)
        {
            Debug.Log($"[DialogueUIController] Changing speaker {speakerName}");
        }

        public void SetDialogueText(string dialogueText)
        {
            Debug.Log($"[DialogueUIController] Setting dialogue text {dialogueText}");
        }
    }
}
