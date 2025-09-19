using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif
using AltEnding.GUI;

namespace AltEnding.Gallery
{
	public class GalleryUIEntry : MonoBehaviour
	{
		[SerializeField] protected AnimatedCanvasManager canvasManager;
		[SerializeField] protected Image lockedImage;
		[SerializeField] protected Image previewImage;
#if UseNA
        [ReadOnly]
#endif
		[SerializeField] protected bool showing;

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (canvasManager == null)
			{
				GetDependents(transform);
			}
		}

		private void GetDependents(Transform transform)
		{
			if (canvasManager == null)
			{
				canvasManager = transform.GetComponent<AnimatedCanvasManager>();
			}
			if (canvasManager == null && transform.parent != null) GetDependents(transform.parent);
		}
#endif

		private void OnEnable()
		{
			if (canvasManager != null) canvasManager.TurnOnEvent.AddListener(UpdateVisibility);
		}

		private void OnDisable()
		{
			if (canvasManager != null) canvasManager.TurnOnEvent.RemoveListener(UpdateVisibility);
		}

		private void UpdateVisibility()
		{
			showing = ShouldShow();
			if (showing)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		protected virtual void Show()
		{
			if (lockedImage != null) lockedImage.enabled = false;
			if (previewImage != null) previewImage.enabled = true;
		}

		protected virtual void Hide()
		{
			if (lockedImage != null) lockedImage.enabled = true;
			if (previewImage != null) previewImage.enabled = false;
		}

		//Overwrite this
		protected virtual bool ShouldShow()
		{
			return true;
		}

		public virtual void Selected() { }
	}
}
