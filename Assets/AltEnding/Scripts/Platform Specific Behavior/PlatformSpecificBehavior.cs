using System.Collections.Generic;
using UnityEngine;

namespace AltEnding
{
    public class PlatformSpecificBehavior : MonoBehaviour
    {
        [ContextMenuItem("Common Lists/Mobile", nameof(SetMobilePlatforms))]
        [ContextMenuItem("Common Lists/Desktop", nameof(SetDesktopPlatforms))]
        [ContextMenuItem("Common Lists/Webplayer", nameof(SetWebList))]
        [ContextMenuItem("Common Lists/Xbox", nameof(SetXBOXList))]
        [ContextMenuItem("Common Lists/Playstation", nameof(SetPlaystationList))]
        [SerializeField]
        protected List<RuntimePlatform> platforms = new List<RuntimePlatform>();
        
#if UseNA
        [NaughtyAttributes.OnValueChanged(nameof(PlatformCheckResult))]
#endif
        [SerializeField]
        protected bool currentPassState;

        #region QOL Functions

        protected RuntimePlatform currentPlatform;

        protected RuntimePlatform RetrieveCurrentPlatform()
        {
            currentPlatform = PlatformManager.instance_Initialised
                ? PlatformManager.instance.currentPlatform
                : Application.platform;
            return currentPlatform;
        }

        protected void SetMobilePlatforms() => platforms = PlatformManager.mobilePlatforms;
        protected void SetEditorPlatforms() => platforms = PlatformManager.editorPlatforms;
        protected void SetDesktopPlatforms() => platforms = PlatformManager.desktopPlatforms;
        protected void SetWebList() => platforms = PlatformManager.webPlatforms;
        protected void SetXBOXList() => platforms = PlatformManager.xboxPlatforms;
        protected void SetPlaystationList() => platforms = PlatformManager.playstationPlatforms;

        #endregion


        void Start()
        {
            RunPlatformCheck();
        }

        protected void RunPlatformCheck() => RunPlatformCheck(RetrieveCurrentPlatform());

        protected void RunPlatformCheck(RuntimePlatform currentPlatform)
        {
            currentPassState = PlatformTest(currentPlatform);
            PlatformCheckResult();
        }

        protected void PlatformCheckResult()
        {
            if (currentPassState)
            {
                PlatformCheckPass();
            }
            else
            {
                PlatformCheckFail();
            }
        }

        protected bool PlatformTest() => PlatformTest(RetrieveCurrentPlatform());

        protected bool PlatformTest(RuntimePlatform currentPlatform)
        {
            if (platforms == null) return false;
            return platforms.Contains(currentPlatform);
        }

        [ContextMenu("Test Pass")]
        protected virtual void PlatformCheckPass()
        {
            //Do custom stuff here
        }

        [ContextMenu("Test Fail")]
        protected virtual void PlatformCheckFail()
        {
            //Do custom stuff here
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            platforms = new List<RuntimePlatform>() { RetrieveCurrentPlatform() };
        }

        private void Awake()
        {
            PlatformManager.runPlatformChecks += RunPlatformCheck;
        }

        private void OnDestroy()
        {
            PlatformManager.runPlatformChecks -= RunPlatformCheck;
        }
#endif
    }
}