using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Articy.Unity;
using Articy.Unity.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using AltEnding.SaveSystem;
using AltEnding.Gallery;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
	public enum SpeakingStates { None = 0, Listening = 1, Speaking = 2, Joining = 3, Leaving = 4 }

	public class SpeakerVisualsInstance : MonoBehaviour
	{
		[SerializeField] private bool doDebugs;

		public static System.Action<SpeakerVisualsInstance> NewCharacter;

		#region Components
		[Header("Component References")]
		[SerializeField]
		protected Image portraitImage;
		[SerializeField]
		protected CanvasGroup portraitCanvasGroup;
		[Space]
#if UseNA
        [Foldout("Animation Variables")]
#endif
		[SerializeField]
		private Animation animationComponent;
		#endregion

		#region Animations
#if UseNA
        [Foldout("Animation Variables"), Label("Speaking to Listening")]
#endif
		[SerializeField]
		private AnimationClip a_speakingToListening;
#if UseNA
        [Foldout("Animation Variables"), Label("Listening to Speaking")]
#endif
		[SerializeField]
		private AnimationClip a_listeningToSpeaking;
#if UseNA
        [Foldout("Animation Variables"), Label("Enter")]
#endif
		[SerializeField]
		private AnimationClip a_enter;
#if UseNA
        [Foldout("Animation Variables"), Label("Leave")]
#endif
		[SerializeField]
		private AnimationClip a_leave;
		#endregion
        
		[SerializeField]
		private SpeakerType myType;

		#region Debug/Current Status
#if UseNA
        [Foldout("Current Status")]
#endif
		[SerializeField]
		private string currentCharacterName;

		[field: SerializeField
#if UseNA
        ,Foldout("Current Status")
#endif
        ]
		public DialogPortraitPackage currentDPP { get; private set; }
#if UseNA
        [Foldout("Current Status")]
#endif
		[SerializeField]
		protected SpeakingStates currentState;

		[field: SerializeField
#if UseNA
        ,Foldout("Current Status")
#endif
        ]
		public string currentExpression { get; private set; }

		public string currentDisplayName { get { return currentCharacterName; } }
		public string currentDPPArticyHexID
		{ 
			get 
			{
				if (currentDPP != null && !string.IsNullOrEmpty(currentDPP.articyHexID))
				{
					return currentDPP.articyHexID;
				}
                
				if (characterToLoad != null)
				{
					// If a character is currently loading their DPP, their ID can be gotten through 'characterToLoad'.
					// This is useful because the save system may want the ID before the DPP is finished loading
					return characterToLoad.Id.ToHex();
				}
                
				return "";
			}
		}

		private Coroutine delayedExtraction;
		public System.Action JoinCompleted;
		public System.Action LeaveCompleted;
        #endregion

        private void OnDisable()
        {
			DeRegAddressablesCallbacks();
        }

        protected virtual void Start()
		{
			if (currentDPP != null)
				ExtractPortraitPackage();
			else
				SnapToState(SpeakingStates.Leaving);
		}

		#region Addressables Loading
		public bool isLoadingAddressable { get { return characterToLoad != null; } }
		private ArticyObject characterToLoad;
		public string loadingSource { get; protected set; }
        private AsyncOperationHandle<IList<IResourceLocation>> m_dPPLocationsOpHandle;
		private IList<IResourceLocation> dPPLocations;
		private AsyncOperationHandle<DialogPortraitPackage> _dPPLoadOpHandle;

		private void DeRegAddressablesCallbacks()
		{
			if (_dPPLoadOpHandle.IsValid()) _dPPLoadOpHandle.Completed -= OnDPPLoadComplete;
			if (m_dPPLocationsOpHandle.IsValid()) m_dPPLocationsOpHandle.Completed -= M_dPPLocationsOpHandle_Completed;
			characterToLoad = null;
		}

		protected void StartLoadingProcess(ArticyObject newCharacterToLoad)
		{
			if (newCharacterToLoad == null) return;
			string address = DialogPortraitPackage.GetAddressableAddress(newCharacterToLoad);
			Debug.Log($"Check Location Address\nAddress: {address}");
			DeRegAddressablesCallbacks();
			characterToLoad = newCharacterToLoad;
			m_dPPLocationsOpHandle = Addressables.LoadResourceLocationsAsync(address);
			m_dPPLocationsOpHandle.Completed += M_dPPLocationsOpHandle_Completed;
		}

		private void M_dPPLocationsOpHandle_Completed(AsyncOperationHandle<IList<IResourceLocation>> obj)
		{
			Debug.Log($"Locations check completed. \nValid: {obj.IsValid()}. \nStatus: {obj.Status.ToString()}");
			if (obj.Status == AsyncOperationStatus.Succeeded)
			{
				dPPLocations = obj.Result;
				Debug.Log($"Locations check Succeeded. \nLength: {dPPLocations.Count}");
				if (dPPLocations.Count > 0)
				{
					_dPPLoadOpHandle = Addressables.LoadAssetAsync<DialogPortraitPackage>(dPPLocations[0]);
					_dPPLoadOpHandle.Completed += OnDPPLoadComplete;
				}
				else
				{
					Debug.LogWarning($"Failed to load DPP from addressables. No resource locations found.");
					characterToLoad = null;
					if (SpeakerVisualsManager.instance_Initialised)
					{
						SpeakerVisualsManager.instance.InstanceLoaded(this);
					}
				}
			}
			else
			{
				Debug.LogWarning($"Failed to load DPP from addressables. Get Resource location operation failed.\n{obj.OperationException.Message}");
				characterToLoad = null;
				if (SpeakerVisualsManager.instance_Initialised)
				{
					SpeakerVisualsManager.instance.InstanceLoaded(this);
				}
			}
		}

		private void OnDPPLoadComplete(AsyncOperationHandle<DialogPortraitPackage> obj)
		{
			if (obj.Status == AsyncOperationStatus.Succeeded)
			{
				if (isLoadingAddressable && obj.Result.articyHexID == characterToLoad.Id.ToHex())
				{
					characterToLoad = null;
					NewPortraitPackage(obj.Result);
				}
				else
				{
					Debug.Log($"Loaded the wrong DPP. Waiting for {characterToLoad.Id.ToHex()}, received {obj.Result.articyHexID}");
				}
			}
			else
			{
				Debug.LogWarning($"Failed to load DPP from addressables. Load resource from location failed.\n{obj.OperationException.Message}");
				if (SpeakerVisualsManager.instance_Initialised)
				{
					SpeakerVisualsManager.instance.InstanceLoaded(this);
				}
			}
		}
		#endregion

        /// <summary>
        /// Update the speaker images with expressions and other visuals to indicate who is talking/listening.
        /// </summary>
        /// <param name="speaker">The Articy Object representing the speaker.</param>
        /// <param name="storyFeature">The StoryFeature with all the necessary data for expressions and such.</param>
        /// <param name="svm">A callback reference to the Speaker Visuals Manager.</param>
        /// <returns>Returns true if a new, unloaded speaker is detected. Use to add a delay of further processing.</returns>
        public bool NewConversationBeat(ArticyObject speaker, StoryFeature storyFeature, SpeakerVisualsManager svm)
        {
            if (speaker == null || storyFeature == null || svm == null)
            {
                return false;
            }

            if (speaker.Id.ToHex() == currentDPPArticyHexID)
            {
                //It's the same person we've been talking to, just update with expressions.
                UpdatePortraitImage(storyFeature);
                return false;
            }

            //New person that we're speaking to, start the process to get the data
            NewPortraitPackage(svm.fallbackPortraitPackage);

            //Get and send a new DialogPortraitPackage for the new speaker.
            Debug.Log($"Start speaking to a new person. HexID: {speaker.Id.ToHex()}");
            loadingSource = storyFeature.ownerId;
            StartLoadingProcess(speaker);

            //Let the gallery manager know this person has been met
            if (GalleryManager.instance_Initialised)
                GalleryManager.instance.UnlockCharacter(speaker.Id.ToHex());

            return true;
        }

        public void NewPortraitPackage(DialogPortraitPackage dPP)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.NewPortraitPackage(dPP{(dPP != null ? dPP.name : "null")})", this);
			if (dPP == null || dPP.articyHexID == currentDPP?.articyHexID) return;

			if (SpeakerVisualsManager.instance_Initialised)
			{
				SpeakerVisualsManager.instance.InstanceLoading(this);
			}
			currentDPP = dPP;
			if (currentState == SpeakingStates.None || currentState == SpeakingStates.Joining)
			{
				if (delayedExtraction != null)
				{
					StopCoroutine(delayedExtraction);
					delayedExtraction = null;
				}
				ExtractPortraitPackage();
			}
			else
			{
				if (delayedExtraction != null)
				{
					StopCoroutine(delayedExtraction);
					delayedExtraction = null;
				}
				delayedExtraction = StartCoroutine(ExtractDPPAfterLeaving(dPP));
			}
		}

		public IEnumerator ExtractDPPAfterLeaving(DialogPortraitPackage newDPP)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.ExtractDPPAfterLeaving()", this);
			if (!ParticipantLeaves()) Left();
			while(currentState != SpeakingStates.None)
			{
				yield return new WaitForEndOfFrame();
			}
			currentDPP = newDPP;
			ExtractPortraitPackage();
			delayedExtraction = null;
		}

		protected virtual void ExtractPortraitPackage()
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.ExtractPortraitPackage()", this);
			if (currentDPP == null)
				return;
            
			currentExpression = "Neutral";
			currentState = SpeakingStates.None;
			portraitImage.sprite = currentDPP.staticAvatar;
            
            UpdateExpression("Neutral");
			SetDisplayName(currentDPP.displayName);

			if (SpeakerVisualsManager.instance_Initialised)
			{
				UpdatePortraitImage(SpeakerVisualsManager.instance.currentStoriesFeature, myType == SpeakerVisualsManager.currentSpeakerType);
				SpeakerVisualsManager.instance.InstanceLoaded(this);
			}
		}

		public void UpdatePortraitImage(StoryFeature storyFeature, bool isSpeaker = true, bool forceAnimationUpdate = false)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.UpdatePortraitImage(StoryFeature, isSpeaker[{isSpeaker}])", this);
            
			// Verify that this is in fact an articy flow object with the feature that we need
			if (storyFeature == null || currentState == SpeakingStates.Joining || currentState == SpeakingStates.Leaving) return;
            
			UpdateExpression(isSpeaker ? storyFeature.speakerExpression : storyFeature.listenerExpression);

			if (currentState == SpeakingStates.None)
			{
				ParticipantJoins(isSpeaker ? SpeakingStates.Speaking : SpeakingStates.Listening );
			}
            else if (isSpeaker) StartSpeaking(forceAnimationUpdate);
			else StartListening(forceAnimationUpdate);
			
		}

		public void UpdateListenerExpression(StoryFeature storyFeature)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.UpdateListenerExpression(StoryFeature)", this);
			if (currentState != SpeakingStates.Listening && currentState != SpeakingStates.Speaking) return;

			StartListening();
            
			if (storyFeature == null)
                return;

			UpdateExpression(storyFeature.listenerExpression);
		}

		public void UpdateExpression(string newExpression)
		{
			if (currentState != SpeakingStates.Listening && currentState != SpeakingStates.Speaking && currentState != SpeakingStates.None)
			{
				if(doDebugs) Debug.Log($"SpeakerVisualsInstance.UpdateExpression(Dialog_Expression[{newExpression}]) failed: current SpeakingState is {currentState.ToString()} instead of {SpeakingStates.Speaking.ToString()} or {SpeakingStates.Listening.ToString()}", this);
				return;
			}
            
			if (newExpression == "Dont_Change" || currentDPP == null)
			{
				Debug.Log($"SpeakerVisualsInstance.UpdateExpression(Dialog_Expression[{newExpression}]) failed: {(newExpression.Equals("Dont_Change") && doDebugs ? "\nExpression was set to not change" : "")}{(currentDPP == null ? "\nThe Current DPP is null" : "")}", this);
				return;
			}

			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.UpdateExpression({newExpression})", this);

			currentExpression = newExpression;
			AnimationClip nextAnimation = currentDPP.GetExpressionAnimation(newExpression);
			if (nextAnimation != null)
			{
				if(doDebugs) Debug.Log($"Playing {currentDPP.displayName}'s {newExpression} animation, {nextAnimation.name}!", this);
            }
            else if (doDebugs)
			{
				Debug.Log($"No animation in {currentDPP.displayName}'s DPP for {newExpression}!", this);
            }
		}

		public void SetDisplayName(string newName)
        {
            bool sendEvent = !newName.IsNullOrWhiteSpace()
                             && newName != SpeakerVisualsManager.fallbackName
                             && newName != currentCharacterName;
            
            currentCharacterName = newName;
            if(sendEvent)
                NewCharacter?.Invoke(this);
        }
        
#if UseNA
        [Button()]
#endif
		protected void StartSpeaking(bool forceAnimation = false)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.StartSpeaking()\nCharacter: {currentCharacterName}\nCurrent State: {currentState}", this);
			if (currentState != SpeakingStates.Speaking || forceAnimation)
			{
				currentState = SpeakingStates.Speaking;
				if (animationComponent != null && a_listeningToSpeaking)
				{
					animationComponent.Play(a_listeningToSpeaking.name);
				}
			}
		}
        
#if UseNA
        [Button()]
#endif
		public virtual void StartListening(bool forceAnimation = false)
		{
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.StartListening()\nCharacter: {currentCharacterName}\nCurrent State: {currentState}", this);
			if (currentState != SpeakingStates.Listening || forceAnimation)
			{
				currentState = SpeakingStates.Listening;
				if (animationComponent != null && a_listeningToSpeaking)
				{
					animationComponent.Play(a_speakingToListening.name);
				}
			}
		}

		public bool ParticipantLeaves()
		{
			if (currentState == SpeakingStates.None)
			{
				if (doDebugs)
                    Debug.Log($"SpeakerVisualsInstance.ParticipantLeaves(); Already Left.\nCharacter: {currentCharacterName}", this);
                
				return true;
			}
            
			if (animationComponent == null || a_leave == null)
			{
				if (doDebugs)
                    Debug.Log($"SpeakerVisualsInstance.ParticipantLeaves(); Animation null error.\nCharacter: {currentCharacterName}", this);
                
				return false;
			}
            
			animationComponent.Play(a_leave.name);
			currentState = SpeakingStates.Leaving;
            
			if (doDebugs)
                Debug.Log($"SpeakerVisualsInstance.ParticipantLeaves(); Animations playing.\nCharacter: {currentCharacterName}", this);
            
			return true;
		}

		public bool ParticipantJoins()
        {
			return ParticipantJoins(SpeakingStates.None);
        }

		public bool ParticipantJoins(SpeakingStates stateToInitialize)
		{
			if (animationComponent == null || !animationComponent.enabled || a_enter == null)
			{
				Debug.LogError($"SpeakerVisualsInstance was told to Join, but the following errors occured: {(animationComponent == null ? "\nAnimation Component is null." : "")}{(animationComponent != null && !animationComponent.enabled ? "\nAnimation Component is not enabled." : "")}{(a_enter == null ? "\nEnter animation is null." : "")}", this);
				return false;
			}
            
			if (doDebugs)
                Debug.Log($"SpeakerVisualsInstance.ParticipantJoins()\nCharacter: {currentCharacterName}; Current State: {currentState.ToString()}; State to Initialize: {stateToInitialize.ToString()}", this);
			
            // I'd like to add something here so that we sample the listening state before playing the joining animation,
			// so that the avatar joins the conversation visually listening, and then if they are speaking they can start speaking.
			// This can ensure scaling and color values of the avatar and Spine Character.
			switch (stateToInitialize)
            {
				case SpeakingStates.Listening:
					a_listeningToSpeaking.SampleAnimation(animationComponent.gameObject, 0f);
					break;
				case SpeakingStates.Speaking:
					a_speakingToListening.SampleAnimation(animationComponent.gameObject, 0f);
					break;
            }
			animationComponent.Play(a_enter.name);
			currentState = SpeakingStates.Joining;
			return true;
		}

		protected bool SnapToState(SpeakingStates stateToInitialize)
		{
			if (animationComponent == null || !animationComponent.enabled)
			{
				Debug.LogError($"SpeakerVisualsInstance was told to SnapTo({stateToInitialize.ToString()}), but the following errors occured: {(animationComponent == null ? "\nAnimation Component is null." : "")}{(animationComponent != null && !animationComponent.enabled ? "\nAnimation Component is not enabled." : "")}", this);
				return false;
			}
            
			if (doDebugs)
                Debug.Log($"SpeakerVisualsInstance.SnapToState()\nCharacter: {currentCharacterName}; Current State: {currentState.ToString()}; State to Initialize: {stateToInitialize.ToString()}", this);

			switch (stateToInitialize)
			{
				case SpeakingStates.Joining:
					if (a_enter == null) return false;
					a_enter.SampleAnimation(animationComponent.gameObject, 1f);
					return true;
				case SpeakingStates.Leaving:
					if (a_leave == null) return false;
					a_leave.SampleAnimation(animationComponent.gameObject, 1f);
					return true;
				case SpeakingStates.Listening:
					if (a_speakingToListening == null) return false;
					a_speakingToListening.SampleAnimation(animationComponent.gameObject, 1f);
					return true;
				case SpeakingStates.Speaking:
					if (a_listeningToSpeaking == null) return false;
					a_listeningToSpeaking.SampleAnimation(animationComponent.gameObject, 1f);
					return true;
				default:
					return true;
			}
		}

		/// <summary>
		/// Called by the animator when the Enter animation is completed
		/// </summary>
		protected virtual void Joined()
        {
			if (SpeakerVisualsManager.instance_Initialised)
			{
				bool isSpeaking = myType == SpeakerVisualsManager.currentSpeakerType;
				if (doDebugs) Debug.Log($"SpeakerVisualsInstance.ParticipantJoined()\nCharacter: {currentCharacterName}; Current State: {currentState.ToString()}; Currently Speaking: {isSpeaking}", this);
				currentState = isSpeaking ? SpeakingStates.Speaking : SpeakingStates.Listening;
				UpdatePortraitImage(SpeakerVisualsManager.instance.currentStoriesFeature, isSpeaking);
			}
			JoinCompleted?.Invoke();
		}

		/// <summary>
		/// Called by the animator when the Exit animation is completed
		/// </summary>
		private void Left()
		{
			portraitImage.enabled = false;
			if (doDebugs) Debug.Log($"SpeakerVisualsInstance.ParticipantLeft()\nCharacter: {currentCharacterName}; Current State: {currentState.ToString()};", this);
			currentState = SpeakingStates.None;
			LeaveCompleted?.Invoke();
		}

		#region Saving & Loading
		internal void Save(SpeakerVisualsSaveData.SpeakerSaveData speakerSaveData)
		{
			speakerSaveData.speakerDPPHexID = currentDPPArticyHexID;
			speakerSaveData.expression = currentExpression;
		}

		internal void Load(SpeakerVisualsSaveData.SpeakerSaveData speakerSaveData)
		{
			if (myType == SpeakerType.Player)
			{
				Debug.Log($"SpeakerVisualsInstance loading player visuals.", this);
				StoryFeature storyFeature = new();
				storyFeature.speakerExpression = speakerSaveData.expression;
				storyFeature.listenerExpression = speakerSaveData.expression;
				UpdatePortraitImage(storyFeature);
			}
			else
			{
				ArticyObject speaker = ArticyDatabase.GetObject(speakerSaveData.speakerDPPHexID.FromHex());
				if (speaker != null)
				{
					Debug.Log($"SpeakerVisualsInstance loading non-player visuals.", this);
					StoryFeature storyFeature = new();
					storyFeature.speakerExpression = speakerSaveData.expression;
					storyFeature.listenerExpression = speakerSaveData.expression;
					NewConversationBeat(speaker, storyFeature, SpeakerVisualsManager.instance);
				}
				else Debug.LogError($"SpeakerVisualsInstance could not load non-player visuals.  It couldn't get a speaker from the Articy database.\nArticy ID: {speakerSaveData.speakerDPPHexID}", this);
			}
		}
		#endregion
	}
}
