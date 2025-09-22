#if UseNA
using NaughtyAttributes;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace AltEnding
{
    public class PlatformManager : PersistentSingleton<PlatformManager>
    {
        public static List<RuntimePlatform> mobilePlatforms = new List<RuntimePlatform>()
            { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer };

        public static List<RuntimePlatform> editorPlatforms = new List<RuntimePlatform>()
            { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor };

        public static List<RuntimePlatform> desktopPlatforms = new List<RuntimePlatform>()
            { RuntimePlatform.WindowsPlayer, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxPlayer };

        public static List<RuntimePlatform> webPlatforms = new List<RuntimePlatform>() { RuntimePlatform.WebGLPlayer };

        public static List<RuntimePlatform> xboxPlatforms = new List<RuntimePlatform>()
            { RuntimePlatform.GameCoreXboxOne, RuntimePlatform.GameCoreXboxSeries, RuntimePlatform.XboxOne };

        public static List<RuntimePlatform> playstationPlatforms = new List<RuntimePlatform>()
            { RuntimePlatform.PS4, RuntimePlatform.PS5 };

        //Private constructor to prevent additional instances
        private PlatformManager()
        {
        }

        [field: SerializeField] public RuntimePlatform currentPlatform { get; private set; }

        protected override void Awake()
        {
            currentPlatform = Application.platform;
            base.Awake();
        }

#if UNITY_EDITOR
        public static System.Action runPlatformChecks;

#if UseNA
        [Button("Run Platform Checks", EButtonEnableMode.Playmode)]
#endif
        static void RunPlatformChecks()
        {
            runPlatformChecks?.Invoke();
        }
#endif

        public static bool IsMobilePlatform()
        {
            if (instance_Initialised)
            {
                return mobilePlatforms.Contains(instance.currentPlatform);
            }
            else
            {
                return mobilePlatforms.Contains(Application.platform);
            }
        }

        public static bool IsDesktopPlatform()
        {
            if (instance_Initialised)
            {
                return desktopPlatforms.Contains(instance.currentPlatform);
            }
            else
            {
                return desktopPlatforms.Contains(Application.platform);
            }
        }
    }
}