using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AltEnding.GUI
{
    public enum OpenState { Closed = 0, Closing = 1, Opening = 2, Open = 3 }
    public enum TwoLevelOpenState { Closed = 0, Closing = 1, Opening = 2, Open = 3, SubClosing = 4, SubOpening = 5, SubOpen = 6}
    
	public class GenericCanvasManager : MonoBehaviour
	{
		[SerializeField] protected bool doDebugs;
		[SerializeField, Tooltip("To Enable/Disable")]//, NaughtyAttributes.ValidateInput("")]
		protected Canvas canvas;
        [SerializeField, Tooltip("To Enable/Disable")]//, NaughtyAttributes.ValidateInput("")]
		protected GraphicRaycaster graphicRaycaster;
		[SerializeField, Tooltip("To toggle the 'Interactive' and 'Blocks Raycast' values")]//, NaughtyAttributes.ValidateInput("")]
		protected CanvasGroup canvasGroup;
		[SerializeField]
		protected Selectable startingSelectable;
		[field: SerializeField]
		public OpenState currentOpenState { get; protected set; }
		
		public UnityEvent TurnOnEvent;
		public UnityEvent TurnOffEvent;

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (canvas == null) canvas = GetComponent<Canvas>();
			if (graphicRaycaster == null)
			{
				if (canvas != null)
				{
					graphicRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
					if (graphicRaycaster == null) graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
				}
				else graphicRaycaster = GetComponent<GraphicRaycaster>();
			}
			if (canvasGroup == null)
			{
				if(canvas != null)
				{
					canvasGroup = canvas.gameObject.GetComponent<CanvasGroup>();
					if (canvasGroup == null) canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
				}
				else canvasGroup = GetComponent<CanvasGroup>();
			}
		}
#endif

		protected virtual void Start()
		{
			if (currentOpenState == OpenState.Open || currentOpenState == OpenState.Opening)
			{
				TurnOn();
			}
			else
			{
				TurnOff();
			}
		}

		[ContextMenu("Turn On")]
		public virtual void TurnOn()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				List<Object> validObjects = new List<Object>() { this};
				if (canvas != null) validObjects.Add(canvas);
				if (graphicRaycaster != null) validObjects.Add(graphicRaycaster);
				if (canvasGroup != null) validObjects.Add(canvasGroup);
				if (startingSelectable != null) validObjects.Add(startingSelectable);
				UnityEditor.Undo.RegisterCompleteObjectUndo(validObjects.ToArray(), $"Turn Off {gameObject.name} Canvas");
			}
#endif
			if (canvas != null) canvas.enabled = true;
			if (graphicRaycaster != null) graphicRaycaster.enabled = true;
			if (canvasGroup != null)
			{
				canvasGroup.interactable = true;
				canvasGroup.blocksRaycasts = true;
			}
			if (startingSelectable != null) startingSelectable.Select();
			currentOpenState = OpenState.Open;
			TurnOnEvent?.Invoke();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (canvas != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
				if (graphicRaycaster != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(graphicRaycaster);
				if (canvasGroup != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(canvasGroup);
				if (startingSelectable != null) UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(startingSelectable);
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			}
#endif
		}

		public virtual void TurnOn(System.Action callback)
		{
			TurnOn();
			callback?.Invoke();
		}

		[ContextMenu("Turn Off")]
		public virtual void TurnOff()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				List<Object> validObjects = new List<Object>() { this };
				if (canvas != null) validObjects.Add(canvas);
				if (graphicRaycaster != null) validObjects.Add(graphicRaycaster);
				if (canvasGroup != null) validObjects.Add(canvasGroup);
				if (startingSelectable != null) validObjects.Add(startingSelectable);
				UnityEditor.Undo.RegisterCompleteObjectUndo(validObjects.ToArray(), $"Turn Off {gameObject.name} Canvas");
			}
#endif
			if (canvas != null) canvas.enabled = false;
			if (graphicRaycaster != null) graphicRaycaster.enabled = false;
			if (canvasGroup != null)
			{
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
			currentOpenState = OpenState.Closed;
			TurnOffEvent?.Invoke();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(graphicRaycaster);
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(canvasGroup);
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			}
#endif
		}

		public virtual void TurnOff(System.Action callback)
		{
			TurnOff();
			callback?.Invoke();
		}

		[ContextMenu("Toggle")]
		public virtual void Toggle()
		{
			if (canvas.enabled)
				TurnOff();
			else
				TurnOn();
		}

		public virtual void TransitionToNewCanvas(GenericCanvasManager newCanvas)
		{
			if (newCanvas != null)
			{
				newCanvas.TurnOn();
				TurnOff();
			}
		}

		#region Inspector QOL
#if UNITY_EDITOR
		private bool canvasIsSet_EditorOnly => canvas != null;
        
#if UseNA
		[NaughtyAttributes.HideIf("canvasIsSet_EditorOnly"), NaughtyAttributes.Button("Add Canvas")]
#endif
		private void AddCanvas_EditorOnly()
		{
			canvas = gameObject.AddComponent<Canvas>();
			OnValidate();
		}
#endif
		#endregion
	}
}
