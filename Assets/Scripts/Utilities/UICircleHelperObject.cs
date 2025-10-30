using UnityEngine;

namespace SpecialAssignment
{
    public class UICircleHelperObject : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform;

        public virtual void SetTransformValues(CircleHelperObjectData data)
        {
            var angle = 2 * Mathf.PI * data.index / data.total;
            rectTransform.localPosition = new Vector3(
                data.radius * Mathf.Cos(angle + data.circleRotation) + data.adjustedXOffset,
                data.radius * Mathf.Sin(angle + data.circleRotation) + data.adjustedYOffset,
                0f);
            rectTransform.sizeDelta = new Vector2(data.childSizeX * data.maxDiameter, data.childSizeY * data.maxDiameter);
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (angle + data.circleRotation - Mathf.PI));
        }
    }
}