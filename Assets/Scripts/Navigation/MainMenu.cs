using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpecialAssignment
{
    public class MainMenu : MonoBehaviour
    {
        public void OnPlayPressed()
        {
            NavigationManager.LoadScene(NavigationManager.GAMEPLAY);
        }

        public void OnGalleryPressed()
        {
            NavigationManager.LoadScene(NavigationManager.GALLERY);
        }
        
        public void OnArticyGalleryPressed()
        {
            NavigationManager.LoadScene(NavigationManager.ARTICY_GALLERY);
        }
        
        public void OnStorePressed()
        {
            NavigationManager.LoadScene(NavigationManager.STORE);
        }
        
        public void OnExtrasPressed()
        {
            NavigationManager.LoadScene(NavigationManager.CREDITS);
        }
        
        public void OnToolsPressed()
        {
            NavigationManager.LoadScene(NavigationManager.VOICE_PRINT_TOOL);
        }
    }
}
