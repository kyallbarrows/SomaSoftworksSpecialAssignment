using System;
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;
using AltEnding.SaveSystem;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    public enum DialogMode
    {
        None = 0,
        Dialog = 1,
        Message = 2,
        Image = 3
    }
    public enum SpeakerType
    {
        None = 0,
        Player = 1,
        NPC = 2
    }

    public class SpeakerVisualsManager : Singleton<SpeakerVisualsManager>, ISaveable
    {
        public static Action<float> NewPanningPosition;
        
        [field: SerializeField
#if UseNA
        ,Label("Fallback Portrait Package"), Required("A Fallback DPP is required in case of errors or performance issues.")
#endif
        ]
        public DialogPortraitPackage fallbackPortraitPackage { get; protected set; }
        public static string fallbackName => instance_Initialised && instance.fallbackPortraitPackage != null ? instance.fallbackPortraitPackage.displayName : "";

#if UseNA
        [Required]
#endif
        [SerializeField, Tooltip("This will always be the player")]
        private SpeakerVisualsInstance leftSpeakerManager;
#if UseNA
        [Required]
#endif
        [SerializeField]
        private SpeakerVisualsInstance rightSpeakerManager;
        
        [field: SerializeField] public AltEnding.GUI.DialogueCanvasManager myDialogCanvasManager { get; protected set; }
        [SerializeField] private float _currentPanningPosition;
        public float currentPanningPosition { get { return _currentPanningPosition; } }

#if UseNA
        [ReadOnly]
#endif
        [SerializeField]
        private SpeakerType _currentSpeakerType;
        public static SpeakerType currentSpeakerType { get { return instance_Initialised ? instance._currentSpeakerType : SpeakerType.None; } }

        public static string currentSpeakerName
        {
            get
            {
                if (instance_Initialised)
                {
                    switch (currentSpeakerType)
                    {
                        case SpeakerType.Player:
                            return instance.leftSpeakerManager.currentDisplayName;
                        case SpeakerType.NPC:
                            return instance.rightSpeakerManager.currentDisplayName;
                        default:
                            return "";
                    }
                }
                else return "";
            }
        }

        [SerializeField] private DialogMode _currentMode;
        public static DialogMode currentMode { get { return instance_Initialised ? instance._currentMode : DialogMode.None; } }

        [field: SerializeField] public IFlowObject currentArticyFlowObject { get; protected set; }
        [field: SerializeField] public StoryFeature currentStoriesFeature { get; protected set; }

        [SerializeField]
        private bool debugSwitching;

        /// <summary>
        /// Update the speaker images with expressions and other visuals to indicate who is talking/listening.
        /// </summary>
        /// <param name="aObject">Articy flow Object</param>
        /// <returns>Returns true if a new, unloaded speaker is detected. Use to add a delay of further processing.</returns>
        public bool UpdateSpeakerImages(IFlowObject aObject)
        {
            if (!(aObject is IObjectWithSpeaker))
            {
                Debug.LogWarning($"passed flowobject is not an object with speaker.", this);
                return false;
            }

            // if we have a speaker, we extract it, because now we have to check what type.
            ArticyObject speaker = (aObject as IObjectWithSpeaker).Speaker;
            if (speaker == null) return false;

            StoryFeature storyFeature;
            if (!ValidateArticyObject(aObject, out storyFeature))
            {
                Debug.LogWarning($"Validation function failed", this);
                _currentMode = DialogMode.None;
                return false;
            }

            currentArticyFlowObject = aObject;
            currentStoriesFeature = storyFeature;
            _currentSpeakerType = GetSpeakerTypeFromArticyHexID(speaker.Id.ToHex());
            _currentMode = DialogMode.Dialog;

            if (storyFeature.removeAllDialogParticipants)
            {
                rightSpeakerManager.ParticipantLeaves();
            }

            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    leftSpeakerManager.UpdatePortraitImage(storyFeature);
                    rightSpeakerManager.UpdateListenerExpression(storyFeature);
                    NewPanningPosition?.Invoke(0f);
                    return false;
                case SpeakerType.NPC:
                    leftSpeakerManager.UpdateListenerExpression(storyFeature);
                    NewPanningPosition?.Invoke(1f);
                    return rightSpeakerManager.NewConversationBeat(speaker, storyFeature, this);
                default:
                    return false;
            }
        }

        private void DebugNewPanPosition(float newPos)
        {
            NewPanningPosition?.Invoke(newPos);
        }

        public void Update()
        {
            if (debugSwitching)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    DebugNewPanPosition(0f);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    DebugNewPanPosition(1f);
                }
            }
        }

        public SpeakerType GetSpeakerTypeFromArticyHexID(string hexID)
        {
            //Start with some sanitation
            if (String.IsNullOrWhiteSpace(hexID)) return SpeakerType.None;

            //Check to see if it's the player
            if (hexID == leftSpeakerManager.currentDPPArticyHexID) return SpeakerType.Player; //Could this be better by storing the value instead of the chain of references?

            //Return NPC because other options have failed
            //We don't have proper testing to make sure it's still a valid HexID though.
            return SpeakerType.NPC;
        }

        public static bool ValidateArticyObject(IFlowObject aObject, out StoryFeature conversion)
        {
            conversion = ArticyStoryHelper.Instance.GetStoryFeature(aObject);
            return conversion != null;
        }

        List<SpeakerVisualsInstance> loadingInstances = new List<SpeakerVisualsInstance>();
        public void InstanceLoading(SpeakerVisualsInstance newInstance)
        {
            if (!loadingInstances.Contains(newInstance)) loadingInstances.Add(newInstance);
        }

        public void InstanceLoaded(SpeakerVisualsInstance oldInstance)
        {
            if (!oldInstance.isLoadingAddressable && loadingInstances.Contains(oldInstance))
            {
                loadingInstances.Remove(oldInstance);
                if (loadingInstances.Count == 0)
                {
                    myDialogCanvasManager?.DPPLoaded();
                }
            }
        }

        public void EndConversation()
        {
            Debug.Log("SpeakerVisualsManager: End the conversation", this);
            leftSpeakerManager?.ParticipantLeaves();
            rightSpeakerManager.ParticipantLeaves();
        }

        #region ISaveable Implementation
        public void ResetData()
		{
            //Do nothing
		}

		public void SaveData(SaveData data)
		{
            leftSpeakerManager.Save(data.speakerVisualsSaveData.leftSpeakerSaveData);
            rightSpeakerManager.Save(data.speakerVisualsSaveData.rightSpeakerSaveData);
		}

		public void LoadData(SaveData data)
		{
            leftSpeakerManager.Load(data.speakerVisualsSaveData.leftSpeakerSaveData);
            rightSpeakerManager.Load(data.speakerVisualsSaveData.rightSpeakerSaveData);
        }
        #endregion

        #region HelperFunctions
        public static bool ModeAndSpeakerMatch(DialogMode inputMode, SpeakerType inputType, DialogMode compareMode, SpeakerType compareType)
        {
            return inputMode == compareMode && inputType == compareType;
        }
        #endregion
    }
}
