using UnityEngine;

namespace AltEnding.GUI
{
    public class QuitButton : MonoBehaviour
    {
        public void OpenQuitModal()
        {
            if (GlobalConfirmationModal.instance_Initialised)
            {
                GlobalConfirmationModal.instance.ShowConfirmationPrompt("Menus/Quit_ConfirmPrompt",
                    "Menus/Quit_Header", QuitModalCallback);
            }
        }

        private void QuitModalCallback(bool confirm)
        {
            if (confirm) QuitFunction();
        }

        public void QuitFunction()
        {
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.ExitPlaymode();
#endif
            }
            else
            {
                Application.Quit();
            }
        }
    }
}