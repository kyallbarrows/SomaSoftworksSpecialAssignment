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
            Debug.Log("Back button clicked");
            NavigationManager.LoadScene(NavigationManager.MAINMENU);
        }
    }
}