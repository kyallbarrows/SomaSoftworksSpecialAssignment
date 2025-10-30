using System.Collections.Generic;
using UnityEngine;

namespace SpecialAssignment
{
    [ExecuteAlways]
    public class UICircleHelper : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float childSizeX;
        [Range(0f, 1f)]
        public float childSizeY;
        [Range(0f, 1f)]
        public float diameterPercent;
        [Range(0f, 1f)]
        public float rotation;
        [Range(0f, 1f)]
        public float circleXOffset;
        public List<RectTransform> circleObjects = new();
        
        private void Update()
        {
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            var parentWidth = Screen.width;
            var parentHeight = Screen.height;
            var maxDiameter = Mathf.Max(parentWidth, parentHeight);
            var diameter = diameterPercent * maxDiameter;
            var radius = diameter / 2f;
            var circleRotation = Mathf.PI * 2 * rotation;
            var adjustedXOffset = circleXOffset * parentWidth;
            
            for (int i = 0; i < circleObjects.Count; i++)
            {
                var angle = 2 * Mathf.PI * i / circleObjects.Count;
                circleObjects[i].localPosition = new Vector3(
                    radius * Mathf.Cos(angle + circleRotation) + adjustedXOffset,
                    radius * Mathf.Sin(angle + circleRotation),
                    0f);
                circleObjects[i].sizeDelta = new Vector2(childSizeX * maxDiameter, childSizeY * maxDiameter);
                circleObjects[i].localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (angle + circleRotation - Mathf.PI));
            }
        }
    }
}
