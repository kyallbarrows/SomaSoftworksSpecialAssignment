using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class WheelMenuButton : MonoBehaviour
    {
        public bool buttonEnabled = true;
        public Image outlineImage;
        public Image mainColorImage;
        public TextMeshProUGUI buttonText;
        
        public Color inactiveOutlineColor;
        public Color inactiveColor;
        public Color inactiveTextColor;
        
        public Color activeColor;
        public Color activeOutlineColor;
        public Color activeTextColor;
        
        public Color disabledColor;
        public Color disabledOutlineColor;
        public Color disabledTextColor;

        public UnityEvent OnClick;

        private void Awake()
        {
            if (!buttonEnabled)
            {
                outlineImage.color = disabledOutlineColor;
                mainColorImage.color = disabledColor;
                buttonText.color = disabledTextColor;
            }
        }

        public void Unfocus()
        {
            if (!buttonEnabled)
                return;
            
            outlineImage.color = inactiveOutlineColor;
            mainColorImage.color = inactiveColor;
            buttonText.color = inactiveTextColor;
        }

        public void Focus()
        {
            if (!buttonEnabled)
                return;
            
            outlineImage.color = activeOutlineColor;
            mainColorImage.color = activeColor;
            buttonText.color = activeTextColor;
        }

        public void Select()
        {
            if (!buttonEnabled)
                return;
            
            OnClick?.Invoke();
        }
    }
}
