using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using DarkTonic.MasterAudio;
using SpecialAssignment;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AltEnding
{
	public class SpeakerChangedMessage
	{
		public string SpeakerName;
	}

	public class SampleUIController : MonoBehaviour
	{
		[Header("UI")]
		// A prefab used to instantiate a branch
		public GameObject branchPrefab;
		// The display name label, used to show the name of the current paused on node
		public TMP_Text displayNameLabel;
		// The main text label, used to show the text of the current paused on node
		public TMP_Text textLabel;

		// Our 3 info labels in the middle of the panel, displaying basic information about the current pause.
		public TMP_Text typeLabel;
		public TMP_Text technicalNameLabel;
		public TMP_Text idLabel;

		public TMP_Text expressionLabel;
		public TMP_Text backgroundLabel;

		// The ui target for our vertical list of branch buttons
		public RectTransform branchLayoutPanel;

		// The preview image UI element. A simple 64x64 image that will show the articy preview image or speaker, depending on the current pause.
		public Image previewImagePanel;

		[Header("Options"), Tooltip("You can set this to true to see false branches in red, very helpful for debugging.")]
		public bool showFalseBranches = false;
        
        [Header("Other References")]
        public DirectorReferences directors;

        private AsyncOperationHandle<IList<IResourceLocation>> loadDPPLocationsHandle;
        private AsyncOperationHandle<DialogPortraitPackage> loadDPPHandle;
        
        private static Dictionary<string, CharacterReferences> characterReferences = new();

        private const string TEMP_TIMELINE_TRIGGER_LINE = "002_B";

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

        private void ArticyFlowController_NewFlowObject(IFlowObject aObject)
        {
            if (aObject == null)
            {
                Debug.Log("New flow object is null");
                return;
            }

            typeLabel.text = "Type: " + aObject.GetType().Name;

            // Reset, just in case we don't have any
            idLabel.text = string.Empty;
            technicalNameLabel.text = string.Empty;

            if (aObject is IArticyObject articyObj)
            {
                string hexID = articyObj.Id.ToHex();
                idLabel.text = "ID: " + hexID;
                technicalNameLabel.text = "TN: " + articyObj.TechnicalName;
            }

            var speakerName = ArticyStoryHelper.Instance.GetDisplayNameFromObject(aObject);
            ChangeSpeaker(speakerName);

            // To show text in the ui of the current node
            // we just check if it has a text property by using the object property interfaces,
            // if it has the property we use it to show the text in our main text label.
            if (aObject is IObjectWithLocalizableText modelWithText)
                textLabel.text = modelWithText.Text;
            else
                textLabel.text = string.Empty;

            // This will make sure that we find a proper preview image to show in our ui.
            ExtractCurrentPausePreviewImage(aObject);

            string expression = ArticyStoryHelper.Instance.GetSpeakerExpressionDescription(aObject);
            if (string.IsNullOrWhiteSpace(expression))
                expression = "---";
            expressionLabel.SetText("Expression: " + expression);
            
            string background = ArticyStoryHelper.Instance.GetBackgroundDescription(aObject);
            if (string.IsNullOrWhiteSpace(background))
                background = "---";
            backgroundLabel.SetText("Background: " + background);
        }

        private void ChangeSpeaker(string speakerName)
        {
	        displayNameLabel.text = speakerName;
	        EventBetter.Raise(new SpeakerChangedMessage() { SpeakerName = speakerName });
        }

        private void ArticyFlowController_NewChoices(IList<Branch> branches)
        {
			// We clear all old branch buttons
			ClearAllBranches();

			// For every branch provided by the flow player, we will create a button in our vertical list
			foreach (var branch in branches)
			{
				// If the branch is invalid because a script evaluated to false, we don't create a button unless we want to see false branches.
				if (!branch.IsValid && !showFalseBranches) continue;

				// We instantiate a button prefab and parent it to our vertical list
				GameObject btnObj = Instantiate(branchPrefab);
				RectTransform rect = btnObj.GetComponent<RectTransform>();
				rect.SetParent(branchLayoutPanel, false);

				// Here we make sure to get the BranchButton component from our button, either by referencing an already existing one or by adding it.
				BranchButton branchBtn = btnObj.GetComponent<BranchButton>();
				if (branchBtn == null)
					branchBtn = btnObj.AddComponent<BranchButton>();

				// This will assign the flow player and branch and will create a proper label for the button.
				branchBtn.AssignBranch(branch);
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
            Debug.Log($"[Dialogue] Using assetId: {assetId}");

            // TODO: Switch this check out for checking for "Timeline" camera type when it exists
            if (line.Equals(TEMP_TIMELINE_TRIGGER_LINE))
            {
                var director = directors.GetDirector(assetId);
                if (director != null)
                    director.Play();
                else
                    Debug.LogWarning($"[Dialogue] Couldn't find playable director for assetId: {assetId}");
                
                return;
            }
            
            if (!characterReferences.ContainsKey(speaker))
            {
                Debug.LogWarning($"[Dialogue] Speaker {speaker} is not in characterReferences");
                return;
            }
            
            MasterAudio.PlaySound3DAtTransform(assetId, characterReferences[speaker].audioTransform);
            characterReferences[speaker].animator.CrossFade(assetId, 0.2f);
        }

        public static void AddCharacterReferences(CharacterReferences characterReference)
        {
            characterReferences.Add(characterReference.characterName, characterReference);
        }

		// Used to initialize our debug flow player handler.
		private void Start()
		{
			// By clearing at start we can safely have a prefab instantiated in the editor for our convenience and automatically get rid of it when we play.
			ClearAllBranches();
		}

		// Convenience method to clear everything underneath our branch layout panel, this should only be our dynamically created branch buttons.
		private void ClearAllBranches()
		{
			foreach (Transform child in branchLayoutPanel)
				Destroy(child.gameObject);
		}

		// Method to find a preview image to show in the ui.
		private void ExtractCurrentPausePreviewImage(IFlowObject aObject)
		{
			IAsset articyAsset = null;

			previewImagePanel.sprite = null;

			// To figure out which asset we could show in our preview, we first try to see if it is an object with a speaker
			if (aObject is IObjectWithSpeaker dlgSpeaker)
			{
				// If we have a speaker we extract it, because now we have to check if it has a preview image.
				ArticyObject speaker = dlgSpeaker.Speaker;
                if (speaker != null && speaker is IObjectWithPreviewImage speakerWithPreviewImage)
                {
                    // Our speaker has the property for preview image so we assign it to our asset.
                    articyAsset = speakerWithPreviewImage.PreviewImage.Asset;
                }
            }

			// If we have no asset until now, we could try to check if the target itself has a preview image.
            if (articyAsset == null && aObject is IObjectWithPreviewImage objectWithPreviewImage)
            {
                articyAsset = objectWithPreviewImage.PreviewImage.Asset;
            }

            // If we have an asset at this point, we load it as a sprite and show it in our ui image.
			if (articyAsset != null)
			{
				previewImagePanel.sprite = articyAsset.LoadAssetAsSprite();
			}
            else if (aObject is IObjectWithSpeaker speaker)
            {
                string address = DialogPortraitPackage.GetAddressableAddress(speaker.Speaker);
                loadDPPLocationsHandle = Addressables.LoadResourceLocationsAsync(address);
                loadDPPLocationsHandle.Completed += LoadDppLocationsHandleCompleted;
            }
		}

        private void LoadDppLocationsHandleCompleted(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            loadDPPLocationsHandle.Completed -= LoadDppLocationsHandleCompleted;
            if (obj.Status != AsyncOperationStatus.Succeeded)
                return;

            var dPPLocations = obj.Result;
            if (dPPLocations.Count <= 0)
                return;

            loadDPPHandle = Addressables.LoadAssetAsync<DialogPortraitPackage>(dPPLocations[0]);
            loadDPPHandle.Completed += OnDPPLoadComplete;
        }

        private void OnDPPLoadComplete(AsyncOperationHandle<DialogPortraitPackage> obj)
        {
            loadDPPHandle.Completed -= OnDPPLoadComplete;
            if (obj.Status != AsyncOperationStatus.Succeeded)
                return;

            var dpp = obj.Result;
            previewImagePanel.sprite = dpp.staticAvatar;
        }
    }
}
