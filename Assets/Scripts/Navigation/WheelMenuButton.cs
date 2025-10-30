using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    }
}
