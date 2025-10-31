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
        public DialogueTextUI lastDialoguePanel;
        public GameObject dialogueOptionPanel;
        public List<DialogueOptionButton> dialogueOptionButtons;

        private List<Branch> activeBranches = new();
        private string lastSpeaker;
        private string lastText;

        private void Start()
        {
            var childCount = dialogueContent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject child = dialogueContent.GetChild(i).gameObject;
                Destroy(child);
            }
        }
        
        public void ClearBranches()
        {
            activeBranches.Clear();
            dialogueOptionPanel.SetActive(false);
            lastDialoguePanel.gameObject.SetActive(false);
            foreach(var button in dialogueOptionButtons)
                button.Reset();
        }

        public void AddBranch(Branch branch)
        {
            int branchIndex = activeBranches.Count;
            if (branchIndex < dialogueOptionButtons.Count)
            {
                var button = dialogueOptionButtons[branchIndex];
                button.SetText(branch.DefaultDescription);
                button.SetBranch(branch);
            }
            
            activeBranches.Add(branch);
            if (activeBranches.Count > 1)
            {
                allDialoguePanel.SetActive(false);
                lastDialoguePanel.gameObject.SetActive(true);
                lastDialoguePanel.SetName(lastSpeaker);
                lastDialoguePanel.SetText(lastText);
                dialogueOptionPanel.SetActive(true);
            }
        }

        public void UpdateDialogue(string speakerName, string dialogueText)
        {
            DialogueTextUI prefab = speakerName.Equals("Whitman")
                ? leftDialogueTextPrefab
                : rightDialogueTextPrefab;
            
            var dialogueInstance = Instantiate(prefab, dialogueContent);
            dialogueInstance.transform.SetAsFirstSibling();
            dialogueInstance.SetName(speakerName);
            dialogueInstance.SetText(dialogueText);
            
            lastSpeaker = speakerName;
            lastText = dialogueText;
        }
    }
}
