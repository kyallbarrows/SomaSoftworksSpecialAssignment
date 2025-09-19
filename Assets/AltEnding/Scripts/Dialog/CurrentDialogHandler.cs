using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Dialog
{
    public class CurrentDialogHandler : MonoBehaviour
    {
        [SerializeField] private bool doDebugs;

		#region Player Variables
#if UseNA
        [OnValueChanged(nameof(AutoPlayerVariables)), BoxGroup("Player")]
#endif
        [SerializeField] private DialogSnippetGUI playerDialogGUI;
        private GameObject playerDialogRoot;
        [HideInInspector] public bool pDAssigned { get { return playerDialogGUI != null; } }
#if UseNA
        [OnValueChanged(nameof(AutoPlayerVariables)), BoxGroup("Player")]
#endif
        [SerializeField] private DialogImageGUI playerImageGUI;
        private GameObject playerImageRoot;
        [HideInInspector] public bool pIAssigned { get { return playerImageGUI != null; } }
        private void AutoPlayerVariables()
		{
            if(pDAssigned)
			{
                playerDialogRoot = playerDialogGUI.gameObject;
			}
            if (pIAssigned)
            {
                playerImageRoot = playerImageGUI.gameObject;
            }
        }
        #endregion

        #region NPC Variables
#if UseNA
        [OnValueChanged(nameof(AutoNPCVariables)), BoxGroup("NPC")]
#endif
        [SerializeField] private DialogSnippetGUI npcDialogGUI;
        private GameObject nPCDialogRoot;
        [HideInInspector] public bool npcDAssigned { get { return npcDialogGUI != null; } }
#if UseNA
        [OnValueChanged(nameof(AutoNPCVariables)), BoxGroup("NPC")]
#endif
        [SerializeField] private DialogImageGUI npcImageGUI;
        private GameObject nPCImageRoot;
        [HideInInspector] public bool npcIAssigned { get { return npcImageGUI != null; } }
        private void AutoNPCVariables()
        {
            if (npcDAssigned)
            {
                nPCDialogRoot = npcDialogGUI.gameObject;
            }
            if (npcIAssigned)
            {
                nPCImageRoot = npcImageGUI.gameObject;
            }
        }
        #endregion
        
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] private SpeakerType currentSpeakerType;

        [field: SerializeField
#if UseNA
        ,ReadOnly
#endif
        ] public DialogMode currentMode { get; private set; }

        [SerializeField] private LayoutElement myLayoutElement;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        [SerializeField] private bool updateLayout;
        [SerializeField] private AnimationCurve lerpCurve;
        [SerializeField, Min(0f)] private float lerpDuration;
        private Coroutine animationCoroutine;

        [Header("Prefabs")]
        [SerializeField] private GameObject spacerObject;

        public bool spacerActive
        {
            get { return spacerObject != null ? spacerObject.activeSelf : false; }
            set { if (spacerObject != null) spacerObject.SetActive(value); }
        }

		private void OnEnable()
		{
            AutoPlayerVariables();
            AutoNPCVariables();
            SetObjectsInactive();
            spacerObject?.SetActive(false);
		}

		public void ClearText()
		{
            currentMode = DialogMode.None;
            
            playerDialogGUI?.ClearAll();
            playerImageGUI?.ClearAll();

            npcDialogGUI?.ClearAll();
            npcImageGUI?.ClearAll();

            SetObjectsInactive();

            spacerObject?.SetActive(false);

            OverrideLayoutElement(0);
        }

        #region Object Handling
        private void SetObjectsActive(DialogMode dialogMode, SpeakerType speakerType)
        {
            if(dialogMode == DialogMode.None || speakerType == SpeakerType.None)
            {
                //All the following comparisons should return a value of "turn off" so let's skip the complicated stuff and do that.
                SetObjectsInactive();
                return;
            }

            //Player Objects
            if (pDAssigned && playerDialogRoot.activeSelf !=    SpeakerVisualsManager.ModeAndSpeakerMatch(dialogMode, speakerType, DialogMode.Dialog, SpeakerType.Player))
                playerDialogRoot.SetActive(!playerDialogRoot.activeSelf);
            if (pIAssigned && playerImageRoot.activeSelf !=     SpeakerVisualsManager.ModeAndSpeakerMatch(dialogMode, speakerType, DialogMode.Image, SpeakerType.Player))
                playerImageRoot.SetActive(!playerImageRoot.activeSelf);

            //NPC Objects
            if (npcDAssigned && nPCDialogRoot.activeSelf !=     SpeakerVisualsManager.ModeAndSpeakerMatch(dialogMode, speakerType, DialogMode.Dialog, SpeakerType.NPC))
                nPCDialogRoot.SetActive(!nPCDialogRoot.activeSelf);
            if (npcIAssigned && nPCImageRoot.activeSelf !=      SpeakerVisualsManager.ModeAndSpeakerMatch(dialogMode, speakerType, DialogMode.Image, SpeakerType.NPC))
                nPCImageRoot.SetActive(!nPCImageRoot.activeSelf);
        }

        private void SetObjectsInactive()
        {
            //Player Objects
            if (pDAssigned && playerDialogRoot.activeSelf)
                playerDialogRoot.SetActive(false);
            if (pIAssigned && playerImageRoot.activeSelf)
                playerImageRoot.SetActive(false);
            //NPC Objects
            if (npcDAssigned && nPCDialogRoot.activeSelf)
                nPCDialogRoot.SetActive(false);
            if (npcIAssigned && nPCImageRoot.activeSelf)
                nPCImageRoot.SetActive(false);
        }
        #endregion

        /// <summary>
        /// Sets the dialog to the new string, choosing which option to show based on the SpeakerVisualsManager instance.
        /// </summary>
        /// <param name="newString">The text to show.</param>
        /// <returns>False if necessary variables are invalid, true if assignment was complete.</returns>
        public bool NewDialogText(string newString)
        {
            if (!SpeakerVisualsManager.instance_Initialised || !pDAssigned || !npcDAssigned) return false;

            currentMode = DialogMode.Dialog;

            if(currentSpeakerType != SpeakerVisualsManager.currentSpeakerType)
            {
                currentSpeakerType = SpeakerVisualsManager.currentSpeakerType;
                spacerObject?.SetActive(true);
			}
			else if(spacerObject != null && spacerObject.activeSelf)
			{
                spacerObject.SetActive(false);
            }

            SetObjectsActive(currentMode, currentSpeakerType);

            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    playerDialogGUI.SetContent(newString, SpeakerVisualsManager.currentSpeakerName);
                    break;
                case SpeakerType.NPC:
                    npcDialogGUI.SetContent(newString, SpeakerVisualsManager.currentSpeakerName);
                    break;
                default:
                    break;
            }
            ShowNewLine();

            return true;
        }

        public bool NewImageDisplay(Sprite newSprite)
        {
            if (!SpeakerVisualsManager.instance_Initialised || !pIAssigned || !npcIAssigned) return false;

            currentMode = DialogMode.Message;

            if (currentSpeakerType != SpeakerVisualsManager.currentSpeakerType)
            {
                currentSpeakerType = SpeakerVisualsManager.currentSpeakerType;
                if (spacerObject != null && !spacerObject.activeSelf) spacerObject.SetActive(true);
            }
            else if (spacerObject != null && spacerObject.activeSelf)
            {
                spacerObject.SetActive(false);
            }

            SetObjectsActive(currentMode, currentSpeakerType);

            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    playerImageGUI.SetContent(newSprite, SpeakerVisualsManager.currentSpeakerName);
                    break;
                case SpeakerType.NPC:
                    npcImageGUI.SetContent(newSprite, SpeakerVisualsManager.currentSpeakerName);
                    break;
                default:
                    break;
            }

            ShowNewLine();
            return true;
        }

        public bool NewImageDisplay(string spriteAddress)
        {
            if (!SpeakerVisualsManager.instance_Initialised || !pIAssigned || !npcIAssigned)
            {
                Debug.LogWarning($"Recieved instruction to load image at address {spriteAddress} but failed null check step; Exiting.");
                return false;
            }else if (doDebugs)
            {
                Debug.Log($"Recieved instruction to load image at address {spriteAddress} and passed null check step; Continuing.");
            }

            currentMode = DialogMode.Image;

            if (currentSpeakerType != SpeakerVisualsManager.currentSpeakerType)
            {
                currentSpeakerType = SpeakerVisualsManager.currentSpeakerType;
                if (spacerObject != null && !spacerObject.activeSelf) spacerObject.SetActive(true);
            }
            else if (spacerObject != null && spacerObject.activeSelf)
            {
                spacerObject.SetActive(false);
            }

            SetObjectsActive(currentMode, currentSpeakerType);

            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    playerImageGUI.SetContent(spriteAddress, SpeakerVisualsManager.currentSpeakerName);
                    break;
                case SpeakerType.NPC:
                    npcImageGUI.SetContent(spriteAddress, SpeakerVisualsManager.currentSpeakerName);
                    break;
                default:
                    break;
            }

            ShowNewLine();
            return true;
        }

        public string CurrentName()
		{
            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                            return pDAssigned ? playerDialogGUI.nameText : "";
                        case DialogMode.Message:
                            return pDAssigned ? playerDialogGUI.nameText : "";
                        case DialogMode.Image:
                            return pIAssigned ? playerImageGUI.nameText : "";
                        default:
                            return "";
                    }
                case SpeakerType.NPC:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                            return npcDAssigned ? npcDialogGUI.nameText : "";
                        case DialogMode.Message:
                            return npcDAssigned ? npcDialogGUI.nameText : "";
                        case DialogMode.Image:
                            return npcIAssigned ? npcImageGUI.nameText : "";
                        default:
                            return "";
                    }
                default: return "";
            }
        }

        public string CurrentText()
		{
            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                                return pDAssigned ? playerDialogGUI.bodyText : "";
                        case DialogMode.Message:
                                return pDAssigned ? playerDialogGUI.bodyText : "";
                        default:
                            return "";
                    }
                case SpeakerType.NPC:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                                return npcDAssigned ? npcDialogGUI.bodyText : "";
                        case DialogMode.Message:
                                return npcDAssigned ? npcDialogGUI.bodyText : "";
                        default:
                            return "";
                    }
                default: return "";
            }
		}

        public DialogImageGUI CurrentImage()
        {
            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    return playerImageGUI;
                case SpeakerType.NPC:
                    return npcImageGUI;
                default: return null;
            }
        }

        public DialogSnippetGUI CurrentSnippet()
        {
            switch (currentSpeakerType)
            {
                case SpeakerType.Player:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                            return playerDialogGUI;
                        case DialogMode.Message:
                            return playerDialogGUI;
                        default:
                            return null;
                    }
                case SpeakerType.NPC:
                    switch (currentMode)
                    {
                        case DialogMode.Dialog:
                            return npcDialogGUI;
                        case DialogMode.Message:
                            return npcDialogGUI;
                        default:
                            return null;
                    }
                default: 
                    return null;
            }
        }

        public bool currentSnippetAssigned
		{
			get
            {
                switch (currentSpeakerType)
                {
                    case SpeakerType.Player:
                        switch (currentMode)
                        {
                            case DialogMode.Dialog:
                                return pDAssigned;
                            case DialogMode.Message:
                                return pDAssigned;
                            default:
                                return false;
                        }
                    case SpeakerType.NPC:
                        switch (currentMode)
                        {
                            case DialogMode.Dialog:
                                return npcDAssigned;
                            case DialogMode.Message:
                                return npcDAssigned;
                            default:
                                return false;
                        }
                    default: 
                        return false;
                }
            }
		}

		private void LateUpdate()
		{
			if (updateLayout)
			{
                UpdateLayoutElement(1f);
			}
		}

        public void ShowNewLine()
		{
            if(lerpDuration > 0)
			{
                updateLayout = false;
                if (animationCoroutine != null) StopCoroutine(animationCoroutine);
                animationCoroutine = StartCoroutine(LerpNewLine(lerpDuration));
			}
			else
			{
                updateLayout = true;
			}
		}

        private IEnumerator LerpNewLine(float duration)
		{
            float time = 0f;
            UpdateLayoutElement(0f);
            while(time < duration)
			{
                yield return null;
                time += Time.deltaTime;
                UpdateLayoutElement(time / duration);
			}
            
		}

        public void OverrideLayoutElement(float lerpSample)
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            UpdateLayoutElement(lerpSample);
        }

		protected void UpdateLayoutElement(float lerpSample)
		{
            if (myLayoutElement == null || layoutGroup == null) return;
            lerpSample = Mathf.Clamp01(lerpSample);
            myLayoutElement.minHeight = layoutGroup.preferredHeight * lerpCurve.Evaluate(lerpSample);
		}
	}
}
