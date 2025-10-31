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
        public List<UICircleHelperObject> circleObjects = new();
        
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
                CircleHelperObjectData data = new()
                {
                    index = i,
                    total = circleObjects.Count,
                    radius = radius,
                    maxDiameter = maxDiameter,
                    circleRotation = circleRotation,
                    childSizeX = childSizeX,
                    childSizeY = childSizeY,
                    adjustedXOffset = adjustedXOffset,
                };
                
                circleObjects[i].SetTransformValues(data);
            }
        }
    }
}
