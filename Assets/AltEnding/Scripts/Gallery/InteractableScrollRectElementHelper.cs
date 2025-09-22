using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AltEnding.Gallery
{
	public class InteractableScrollRectElementHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] protected ScrollRect scrollRect;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (scrollRect == null)
			{
				GetDependents(transform);
			}
		}

		private void GetDependents(Transform transform)
		{
			if (scrollRect == null)
			{
				scrollRect = transform.GetComponent<ScrollRect>();
			}
			if (scrollRect == null && transform.parent != null) GetDependents(transform.parent);
		}
#endif

		#region Drag Handling Events
		public void OnBeginDrag(PointerEventData pointerEventData)
		{
			ExecuteEvents.Execute(scrollRect.gameObject, pointerEventData, ExecuteEvents.beginDragHandler);
		}

		public void OnDrag(PointerEventData pointerEventData)
		{
			ExecuteEvents.Execute(scrollRect.gameObject, pointerEventData, ExecuteEvents.dragHandler);
		}

		public void OnEndDrag(PointerEventData pointerEventData)
		{
			ExecuteEvents.Execute(scrollRect.gameObject, pointerEventData, ExecuteEvents.endDragHandler);
		}
		#endregion
	}
}
