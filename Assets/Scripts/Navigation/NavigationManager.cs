using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpecialAssignment
{
    public class NavigationManager
    {
        #region scene name consts

        public const string SPLASH = "Splash";
        public const string MAINMENU = "MainMenu";
        public const string GAMEPLAY = "Gameplay";
        public const string STORE = "Store";
        public const string CREDITS = "Credits";
        public const string GALLERY = "Gallery";
        public const string ARTICY_GALLERY = "ArticyGallery";
        public const string ARTICY_DEBUGGER = "ArticyDebugger";
        public const string VOICE_PRINT_TOOL = "VoicePrintTool";

        public static string ArticyDoc = "";

        public static Stack<string> Pages = new Stack<string>();

        #endregion

        public static async void LoadScene(string sceneName)
        {
            Pages.Push(sceneName);
            
            // await ShowTransitionWipe();

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            // await HideTransitionWipe();
        }

        public static async void Back()
        {
            if (Pages.Count > 0)
            {
                var currPage = Pages.Pop();
                var lastPage = Pages.Peek();
                SceneManager.LoadScene(lastPage, LoadSceneMode.Single);
            }

        }
    }
}