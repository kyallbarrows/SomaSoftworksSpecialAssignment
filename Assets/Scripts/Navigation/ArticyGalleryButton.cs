using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpecialAssignment
{
    public class ArticyGalleryButton : MonoBehaviour
    {
        public string TechnicalName { 
            get { 
                return buttonLabel.text; 
            }

            set {
                buttonLabel.text = value;
            }
        }
        
        [SerializeField]
        private TextMeshProUGUI buttonLabel;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public void OnClick() {
            NavigationManager.ArticyDoc = TechnicalName;
            NavigationManager.LoadScene(NavigationManager.ARTICY_DEBUGGER);
        }
    }
}
