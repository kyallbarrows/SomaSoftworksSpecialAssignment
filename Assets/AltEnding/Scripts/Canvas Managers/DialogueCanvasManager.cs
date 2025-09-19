using System.Collections.Generic;
using UnityEngine;
using AltEnding.Dialog;
using Articy.Unity;
using Articy.Unity.Interfaces;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.GUI
{
    public class DialogueCanvasManager : GenericCanvasManager
    {
        [Header("Dialogue Variables")]
        [SerializeField] private CurrentDialogHandler myCurrentDialogHandler;

        [Header("Choices Variables")]
        [SerializeField]
        private List<BranchButton> choiceButtonList;
        [SerializeField]
        private BranchButton exampleChoiceButton;
        [SerializeField]
        private RectTransform choiceLayoutPanel;
        [SerializeField]
        private CanvasGroup choiceCanvasGroup;

        [Header("Conversation History")]
        [SerializeField]
        protected Transform conversationHistoryParent;
#if UseNA
        [Foldout("Dialog Prefabs")]
#endif
        [SerializeField] protected DialogSnippetGUI dialogPrefabFullbodyPlayer;
#if UseNA
        [Foldout("Dialog Prefabs")]
#endif
        [SerializeField] protected DialogSnippetGUI dialogPrefabFullbodyNPC;
#if UseNA
        [Foldout("Dialog Prefabs")]
#endif
		[SerializeField] protected DialogImageGUI dialogImagePrefabPlayer;
#if UseNA
        [Foldout("Dialog Prefabs")]
#endif
		[SerializeField] protected DialogImageGUI dialogImagePrefabNPC;
		[SerializeField] protected GameObject conversationHistorySpacerPrefab;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] protected SpeakerType lastSpeakerType;

        [Header("RenderTexturePrefab Management")]
		[SerializeField] Transform renderTexturePrefabParent;
		[SerializeField] List<RenderTexturePrefab> currentRenderTexturePrefabs = new List<RenderTexturePrefab>();
		[SerializeField] int renderTexturePrefabSpacing;

		[Header("Options"), Tooltip("You can set this to true to see false branches in red, very helpful for debugging."), SerializeField]
        protected bool showFalseBranches = false;

        private bool initialized;


        protected IFlowObject currentArticyFlowObject;
        [field: SerializeField] public bool canContinue { get; private set; }
#if UseNA
        [ReadOnly]
#endif
        [SerializeField]
        private List<Component> continueBlockers;

        private void OnEnable()
        {
            initialized = false;
            ArticyFlowController.NewFlowObject += ArticyFlowController_NewFlowObject;
            ArticyFlowController.NewChoices += ArticyFlowController_NewChoices;
            ArticyFlowController.FlowStarting += ArticyFlowController_FlowStarting;
            ArticyFlowController.FlowEnded += ArticyFlowController_FlowEnded;
#if UNITY_EDITOR
            OnValidate();
#endif
		}

        private void OnDisable()
        {
            ArticyFlowController.NewFlowObject -= ArticyFlowController_NewFlowObject;
            ArticyFlowController.NewChoices -= ArticyFlowController_NewChoices;
            ArticyFlowController.FlowStarting -= ArticyFlowController_FlowStarting;
            ArticyFlowController.FlowEnded -= ArticyFlowController_FlowEnded;
		}

		protected override void Start()
        {
            continueBlockers.CleanList();
            base.Start();
            if (!initialized) ClearAll();
        }

        public override void TurnOn()
        {
            if (!initialized) ClearAll();
            base.TurnOn();
            RemoveContinueBlocker(null);
        }

        public virtual void ValidateOn()
        {
            if (!initialized) ClearAll();
            if (!canvas.enabled || !graphicRaycaster.enabled) base.TurnOn();
        }

        public override void TurnOff()
        {
            base.TurnOff();
            ClearAll();
        }

        private void ClearAll()
        {
            ClearConversationHistory();
            ClearAllChoices();
            initialized = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (exampleChoiceButton == null && choiceButtonList != null && choiceButtonList.Count > 0 && choiceButtonList[0] != null)
                exampleChoiceButton = choiceButtonList[0];
            
            if (choiceLayoutPanel == null && exampleChoiceButton != null)
                choiceLayoutPanel = (RectTransform)exampleChoiceButton.transform.parent;
        }
#endif

        /// <summary>
        /// Adds the given component to the list of references blocking narrative continuing.
        /// </summary>
        /// <param name="blockingComponent"></param>
        /// <returns>The current 'CanContinue' value.</returns>
        public bool AddContinueBlocker(Component blockingComponent)
        {
            continueBlockers.CleanList();

            if (blockingComponent != null)
            {
                if (!continueBlockers.Contains(blockingComponent))
                {
                    continueBlockers.Add(blockingComponent);
                }
            }

            canContinue = continueBlockers.Count <= 0;
            return canContinue;
        }

        /// <summary>
        /// Removes the given component from the list of references blocking narrative continuing.
        /// </summary>
        /// <param name="blockingComponent"></param>
        /// <returns>The current 'CanContinue' value.</returns>
        public bool RemoveContinueBlocker(Component blockingComponent)
        {
            continueBlockers.CleanList();

            if (blockingComponent != null)
            {

                for (int c = continueBlockers.Count - 1; c >= 0; c--)
                {
                    if (continueBlockers[c] == blockingComponent) continueBlockers.RemoveAt(c);
                }
            }

            canContinue = continueBlockers.Count <= 0;
            return canContinue;
        }

        #region Articy Flow Controller Events
        private void ArticyFlowController_NewFlowObject(IFlowObject aObject)
        {
            currentArticyFlowObject = aObject;

            if (aObject == null)
            {
                ArticyFlowController_FlowEnded();
                return;
            }

            if (ArticyStoryHelper.Instance.IsCheckpoint(aObject))
                return;

            if (doDebugs)
                Debug.Log($"Object type: {aObject.GetType()}");
            
            TurnOn();

            // Check if there's already dialog that needs to be moved to the conversation history
            MoveCurrentMessageToMessageHistory();

			// This will make sure that we find a proper preview image to show in our ui.
			if (SpeakerVisualsManager.instance_Initialised && SpeakerVisualsManager.instance.UpdateSpeakerImages(aObject))
            {
                // It's loading the new speaker, so implement a delay?
                AddContinueBlocker(SpeakerVisualsManager.instance);
            }
            else
            {
                SetTextFromArticyFlowObject(aObject);
            }
        }

        public void DPPLoaded()
        {
            if (!canContinue)
            {
                SetTextFromArticyFlowObject(currentArticyFlowObject);
                RemoveContinueBlocker(SpeakerVisualsManager.instance);
            }
        }
        
        private void SetTextFromArticyFlowObject(IFlowObject aObject)
        {
            ArticyObject imageObject = ArticyStoryHelper.Instance.GetImageFromObject(aObject);
            if (imageObject != null)
            {
                Debug.Log($"New Image to display:{imageObject.TechnicalName}({DialogImageAsset.GetAddressableAddress(imageObject)})");
                myCurrentDialogHandler?.NewImageDisplay(DialogImageAsset.GetAddressableAddress(imageObject));
                return;
            }

            // To show text in the ui of the current node
            // we just check if it has a text property by using the object property interfaces,
            // if it has the property we use it to show the text in our main text label.
            if (aObject is IObjectWithLocalizableText modelWithLocalizableText)
            {
                if (doDebugs)
                    Debug.Log($"IObjectWithLocalizableText conversion successful, text is as follows: \n{modelWithLocalizableText.Text}");
                
                ShowDialogText(modelWithLocalizableText.Text);
            }
            else
            {
                if (doDebugs)
                    Debug.LogWarning($"Conversion to IObjectWithLocalizableText failed, text will be null");
                
                ShowDialogText(string.Empty);
            }
            
            if (doDebugs)
                Debug.Log($"New flow object does not have the template\nIt's type: {(aObject == null ? "null" : aObject.GetType())}");
        }

        private void ArticyFlowController_NewChoices(IList<Branch> branches)
        {
            Debug.Log($"Show Choices", this);
            // We clear all old branch buttons
            ClearAllChoices();

            // If there's only one choice, don't show any
            if (branches.Count <= 1) return;

            // For every branch provided by the flow player, we will create a button in our vertical list
            foreach (var branch in branches)
            {
                // If the branch is invalid because a script evaluated to false, we don't create a button unless we want to see false branches.
                if (!branch.IsValid && !showFalseBranches) continue;

                // Here we make sure to get the Branch component from our button, either by referencing an already existing one, or by adding it.
                BranchButton branchBtn = GetNewChoiceButton();
                if (branchBtn != null)
                {
                    branchBtn.gameObject.SetActive(true);
                    // This will assign the flow player and branch and will create a proper label for the button.
                    branchBtn.AssignBranch(branch);
                }
                else
                {
                    Debug.LogError($"CRITICAL ERROR: NO NEW BUTTON FOR BRANCH {branch.BranchId}", this);
                }
            }

            UISelectFirstChoice();
		}

        private void ArticyFlowController_FlowStarting()
        {
            ClearAll();
        }

        private void ArticyFlowController_FlowEnded()
        {
            if (SpeakerVisualsManager.instance_Initialised)
            {
                SpeakerVisualsManager.instance.EndConversation();
            }
            TurnOff();
        }
		#endregion

		private void ClearAllChoices()
        {
            foreach (BranchButton branchButton in choiceButtonList)
            {
                branchButton.gameObject.SetActive(false);
            }
        }

        private BranchButton GetNewChoiceButton()
        {
            BranchButton returnButton = null;
            
            // Start by looping through our pool for an unused button
            for (int c = 0; c < choiceButtonList.Count; c++)
            {
                if (!choiceButtonList[c].gameObject.activeSelf)
                {
                    returnButton = choiceButtonList[c];
                    break;
                }
            }
            
            // If the pool is fully utilized, make a new button.
            if (returnButton == null)
            {
                returnButton = Instantiate(exampleChoiceButton, choiceLayoutPanel, true);
                returnButton.transform.SetAsLastSibling();
                choiceButtonList.Add(returnButton);
            }
            return returnButton;
        }

        private void UISelectFirstChoice()
        {
			if (choiceButtonList != null && choiceButtonList.Count > 0 && choiceButtonList[0].isActiveAndEnabled)
                choiceButtonList[0].UISelect();
		}

        private string CurrentDialogSpeakerName()
        {
            return myCurrentDialogHandler ? myCurrentDialogHandler.CurrentName() : SpeakerVisualsManager.currentSpeakerName;
        }

        private string CurrentDialogText()
        {
            return myCurrentDialogHandler ? myCurrentDialogHandler.CurrentText() : "";
        }

        private void ShowDialogText(string newString)
        {
            myCurrentDialogHandler?.NewDialogText(newString);
        }

        #region Conversation History
        private void MoveCurrentMessageToMessageHistory()
        {
            if (myCurrentDialogHandler == null || myCurrentDialogHandler.currentMode == DialogMode.None)
                return;

            if (myCurrentDialogHandler.currentMode == DialogMode.Image)
            {
                AddImageHistoryEntry(SpeakerVisualsManager.currentSpeakerType, CurrentDialogSpeakerName(), myCurrentDialogHandler.CurrentImage());
            }
            else if (myCurrentDialogHandler.currentMode == DialogMode.Dialog || myCurrentDialogHandler.currentMode == DialogMode.Message)
            {
                AddConversationHistoryEntry(SpeakerVisualsManager.currentSpeakerType, CurrentDialogSpeakerName(), myCurrentDialogHandler.CurrentText(), myCurrentDialogHandler.currentMode);
            }
        }
        
        private void AddConversationHistoryEntry(SpeakerType speakerType, string speakerName, string dialogText, DialogMode mode)
        {
            if (speakerType != lastSpeakerType) AddSpacer();
            lastSpeakerType = speakerType;
            myCurrentDialogHandler.OverrideLayoutElement(0);
            DialogSnippetGUI convHistObj = Instantiate(SelectGUIPrefab(speakerType, mode), conversationHistoryParent);
            convHistObj.transform.SetAsLastSibling();
            convHistObj.SetContent(dialogText, speakerName);
            convHistObj.gameObject.SetActive(true);
        }

        private void AddImageHistoryEntry(SpeakerType speakerType, string speakerName, DialogImageGUI previousGUI)
		{
			if (speakerType != lastSpeakerType)
                AddSpacer();
            
			lastSpeakerType = speakerType;
			myCurrentDialogHandler.OverrideLayoutElement(0);
			DialogImageGUI convHistObj = Instantiate(SelectConversationHistoryImagePrefab(speakerType), conversationHistoryParent);
            convHistObj.transform.SetAsLastSibling();
            
            if (previousGUI.isLoading) 
                convHistObj.SetContent(previousGUI.imageAddress, speakerName);
            else
                convHistObj.SetContent(previousGUI.myDIA, speakerName);
            
            convHistObj.gameObject.SetActive(true);
        }

        private DialogSnippetGUI SelectGUIPrefab(SpeakerType type, DialogMode mode)
        {
            switch (type)
            {
                case SpeakerType.Player:
                    switch (mode)
                    {
                        case DialogMode.Dialog:
                            return dialogPrefabFullbodyPlayer;
                        default:
                            return null;
                    }
                case SpeakerType.NPC:
                    switch (mode)
                    {
                        case DialogMode.Dialog:
                            return dialogPrefabFullbodyNPC;
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }

        private DialogImageGUI SelectConversationHistoryImagePrefab(SpeakerType type)
		{
            switch (type)
            {
                case SpeakerType.Player:
                    return dialogImagePrefabPlayer;
                case SpeakerType.NPC:
                    return dialogImagePrefabNPC;
                default:
                    return null;
			}
        }

        private void AddSpacer()
        {
            GameObject convHistObj = Instantiate(conversationHistorySpacerPrefab, conversationHistoryParent);
            convHistObj.transform.SetAsLastSibling();
            convHistObj.SetActive(true);
        }

        private string BuildConversationHistoryString(string speakerName, string dialogString)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine(speakerName);
            builder.Append(dialogString);
            // Remove the left alignment tags Articy adds
            builder.Replace("<align=left>", "");
            builder.Replace("</align>", "");
            return builder.ToString();
        }

        private void ClearConversationHistory()
        {
            myCurrentDialogHandler?.ClearText();
            for (int i = conversationHistoryParent.childCount - 1; i >= 0; i--)
            {
                Destroy(conversationHistoryParent.GetChild(i).gameObject);
            }

            // Destroy render texture GameObjects
			for (int i = renderTexturePrefabParent.childCount - 1; i >= 0; i--)
			{
				Destroy(renderTexturePrefabParent.GetChild(i).gameObject);
			}
            currentRenderTexturePrefabs.Clear();
		}
        #endregion

        public bool InstatiateNewRenderTexturePrefabClone(RenderTexturePrefab sourceRTP, System.Action<RenderTexture> renderTextureCreatedCallback = null)
        {
            if (sourceRTP == null) return false;

            // Create a new instance of the given RenderTexturePrefab
            RenderTexturePrefab newRTP = Instantiate(sourceRTP, renderTexturePrefabParent);
            newRTP.transform.position = Vector3.right * currentRenderTexturePrefabs.Count * renderTexturePrefabSpacing;
            currentRenderTexturePrefabs.Add(newRTP);

            // Create a render texture at runtime
            RenderTexture newRT = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            newRT.Create();
			Debug.Log("[RT] Created RT. ID: " + newRT.GetInstanceID());
            renderTextureCreatedCallback?.Invoke(newRT);

			// Tell the RenderTexturePrefab to use the new RenderTexture
			newRTP.SetRenderTexture(newRT);

			return true;
        }
    }
}
