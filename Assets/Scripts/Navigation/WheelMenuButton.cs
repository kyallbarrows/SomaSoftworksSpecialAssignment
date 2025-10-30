using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class WheelMenuButton : UICircleHelperObject
    {
        public bool buttonEnabled = true;
        public Image outlineImage;
        public Image mainColorImage;
        public TextMeshProUGUI buttonText;
        public float buttonSelectedSize = 1f;
        public float buttonSizeChangeSpeed = 4f;
        public AnimationCurve buttonSizeSelectCurve;
        public AnimationCurve buttonSizeUnselectCurve;
        
        public Color inactiveOutlineColor;
        public Color inactiveColor;
        public Color inactiveTextColor;
        
        public Color activeColor;
        public Color activeOutlineColor;
        public Color activeTextColor;
        
        public Color disabledColor;
        public Color disabledOutlineColor;
        public Color disabledTextColor;

        public Color pressedColor;

        public UnityEvent OnClick;

        private bool selected;
        private float selectedProgress = 0f;

        private void Awake()
        {
            if (!buttonEnabled)
            {
                outlineImage.color = disabledOutlineColor;
                mainColorImage.color = disabledColor;
                buttonText.color = disabledTextColor;
            }
        }

        public void Unselect()
        {
            if (!buttonEnabled)
                return;
            
            outlineImage.color = inactiveOutlineColor;
            mainColorImage.color = inactiveColor;
            buttonText.color = inactiveTextColor;
            selected = false;
        }

        public void Select()
        {
            if (!buttonEnabled)
                return;
            
            outlineImage.color = activeOutlineColor;
            mainColorImage.color = activeColor;
            buttonText.color = activeTextColor;
            selected = true;
        }

        public void Activate()
        {
            if (!buttonEnabled)
                return;
            
            mainColorImage.color = pressedColor;
            OnClick?.Invoke();
        }

        public override void SetTransformValues(CircleHelperObjectData data)
        {
            float curvedProgress;
            
            if (selected)
            {
                selectedProgress = Mathf.Min(selectedProgress + Time.deltaTime * buttonSizeChangeSpeed, 1f);
                curvedProgress = buttonSizeSelectCurve.Evaluate(selectedProgress);
            }
            else
            {
                selectedProgress = Mathf.Max(selectedProgress - Time.deltaTime * buttonSizeChangeSpeed, 0f);
                curvedProgress = buttonSizeUnselectCurve.Evaluate(selectedProgress);
            }
            
            var x = Mathf.Lerp(data.childSizeX, data.childSizeX * buttonSelectedSize, curvedProgress);
            var y  = Mathf.Lerp(data.childSizeY, data.childSizeY * buttonSelectedSize, curvedProgress);
            var angle = 2 * Mathf.PI * data.index / data.total;
            
            rectTransform.localPosition = new Vector3(
                data.radius * Mathf.Cos(angle + data.circleRotation) + data.adjustedXOffset,
                data.radius * Mathf.Sin(angle + data.circleRotation),
                0f);
            rectTransform.sizeDelta = new Vector2(x * data.maxDiameter, y * data.maxDiameter);
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (angle + data.circleRotation - Mathf.PI));
        }
    }
}
