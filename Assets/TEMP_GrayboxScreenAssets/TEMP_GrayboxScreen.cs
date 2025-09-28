using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class TEMP_GrayboxScreen : MonoBehaviour
    {
        [SerializeField] private string screenName;
        [SerializeField] private TextMeshProUGUI screenNameDisplay;
        [SerializeField] private GameObject backButton;

        void Start()
        {
            if (screenNameDisplay != null)
                screenNameDisplay.text = $"You are on the\n<b>{screenName}</b>\nscreen";

            if (screenName == NavigationManager.SPLASH || screenName == NavigationManager.MAINMENU)
            {
                backButton.SetActive(false);
            }
        }

        public void OnBackButtonClicked()
        {
            NavigationManager.Back();
            /*
            Debug.Log("Back button clicked");
            if (screenName == NavigationManager.ARTICY_DEBUGGER)
                NavigationManager.LoadScene(NavigationManager.ARTICY_GALLERY);
            else
                NavigationManager.LoadScene(NavigationManager.MAINMENU);
                */
        }

        public void OnVoicePrintButtonPressed()
        {
            NavigationManager.LoadScene(NavigationManager.VOICE_PRINT_TOOL);
        }
    }
}