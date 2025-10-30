using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpecialAssignment
{
    public class WheelMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
    {
        public float spinMultiplier = 0.4f;
        public float firstButtonRotationOffset = 0.5f;
        public AnimationCurve buttonMagnetCurve;
        public List<WheelMenuButton> buttons;
        
        private UICircleHelper circleHelper;
        private WheelMenuButton currentButton;

        private float startY;
        private float startRotation;
        private float yDelta;
        private Vector2 pointerDownPosition;
        
        private bool movingToNearestButton;
        private float targetRotation;
        private float moveToButtonProgress;

        private void Awake()
        {
            circleHelper = GetComponent<UICircleHelper>();
            SetButton(0);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var startPoint = eventData.pressPosition;
            startY = startPoint.y / Screen.height;
            startRotation = circleHelper.rotation;
            SetButton(-1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var normalizedCurrentVertical = eventData.position.y / Screen.height;
            yDelta = (startY - normalizedCurrentVertical) * spinMultiplier;
            circleHelper.rotation = (startRotation + yDelta + 1) % 1f;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            movingToNearestButton = true;
            moveToButtonProgress = 0f;
            startRotation = circleHelper.rotation;
            targetRotation = GetClosestButtonRotation();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownPosition = eventData.position;
            movingToNearestButton = false;
            moveToButtonProgress = 0f;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.position == pointerDownPosition && currentButton != null)
                currentButton.Activate();
        }

        public void Update()
        {
            if (movingToNearestButton)
            {
                moveToButtonProgress += Mathf.Min(Time.deltaTime * 4, 1f);
                var t = buttonMagnetCurve.Evaluate(moveToButtonProgress);
                circleHelper.rotation = Mathf.Lerp(startRotation, targetRotation, t);
                if (moveToButtonProgress >= 1f)
                {
                    movingToNearestButton = false;
                    var buttonIndex = GetClosestButtonIndexToRotation();
                    SetButton(buttonIndex);
                }
            }
        }

        private void SetButton(int index)
        {
            if (index >= buttons.Count || index < 0)
            {
                if (currentButton != null)
                    currentButton.Unselect();
                currentButton = null;
            }
            else
            {
                currentButton = buttons[index];
                currentButton.Select();
            }
        }

        private float GetClosestButtonRotation()
        {
            var current = circleHelper.rotation;
            var numButtons = circleHelper.circleObjects.Count;
            var buttonInterval = 1f / numButtons;
            var intervalRemainder = current % buttonInterval;
            if (intervalRemainder < buttonInterval / 2f)
                return current - intervalRemainder;
            else
                return current + (buttonInterval - intervalRemainder);
        }

        private int GetClosestButtonIndexToRotation()
        {
            var current = 1 - (circleHelper.rotation + 1 - firstButtonRotationOffset) % 1;
            var numButtons = circleHelper.circleObjects.Count;
            int buttonPosition = Mathf.RoundToInt(current * numButtons);
            if (buttonPosition == numButtons)
                return 0;

            return buttonPosition;
        }
    }
}
