using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpecialAssignment
{
    public class WheelMenu : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public float spinMultiplier = 0.4f;
        public AnimationCurve buttonMagnetCurve;
        public List<WheelMenuButton> buttons;
        
        private UICircleHelper circleHelper;
        private WheelMenuButton currentButton;

        private float startY;
        private float startRotation;
        private float yDelta;
        
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
            movingToNearestButton = false;
            moveToButtonProgress = 0f;
        }

        public void Update()
        {
            if (movingToNearestButton)
            {
                moveToButtonProgress += Mathf.Min(Time.deltaTime * 4, 1f);
                var t = buttonMagnetCurve.Evaluate(moveToButtonProgress);
                circleHelper.rotation = Mathf.Lerp(startRotation, targetRotation, t);
                if (moveToButtonProgress >= 1f)
                    movingToNearestButton = false;
            }
        }

        private void SetButton(int index)
        {
            if (index >= buttons.Count || index < 0)
                currentButton = null;
            else
                currentButton = buttons[index];
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
    }
}
