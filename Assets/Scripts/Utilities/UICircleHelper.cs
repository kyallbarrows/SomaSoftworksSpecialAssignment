using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpecialAssignment
{
    [ExecuteInEditMode]
    public class UICircleHelper : MonoBehaviour
    {
        public float childSizeX = 100f;
        public float childSizeY = 100f;
        [Range(0f, 1f)]
        public float diameterPercent;
        [Range(0f, 1f)]
        public float rotation;

        public float circleXOffset;
        public List<RectTransform> circleObjects = new();

        private RectTransform myTransform;
        
        private void OnEnable()
        {
            myTransform = GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            var parentWidth = Screen.width;
            var parentHeight = Screen.height;
            var maxDiameter = Mathf.Max(parentWidth, parentHeight);
            var diameter = diameterPercent * maxDiameter;
            var radius = diameter / 2f;
            var circleRotation = Mathf.PI * 2 * rotation;
            
            for (int i = 0; i < circleObjects.Count; i++)
            {
                var angle = 2 * Mathf.PI * i / circleObjects.Count;
                circleObjects[i].localPosition = new Vector3(radius * Mathf.Cos(angle + circleRotation) + circleXOffset, radius * Mathf.Sin(angle + circleRotation), 0f);
                circleObjects[i].sizeDelta = new Vector2(childSizeX, childSizeY);
                circleObjects[i].localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (angle + circleRotation - Mathf.PI));
            }
        }
    }
}
