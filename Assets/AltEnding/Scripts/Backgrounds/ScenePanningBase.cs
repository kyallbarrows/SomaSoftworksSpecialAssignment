using System.Collections;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace AltEnding
{
	public class ScenePanningBase : MonoBehaviour
	{
		public static System.Action StartDragAction;

#if UseNA
        [Label("Current Panning Position"), OnValueChanged(nameof(UpdatePosition))]
#endif
		[SerializeField, Range(0f, 1f)]
		protected float panningPosition;

        #region Lerping Variables

        protected const string label_lerping = "Lerping";
		protected Coroutine lerpCoroutine;

#if UseNA
        [CurveRange(0, 0, 1, 1), Label("Lerping Curve"), Foldout(label_lerping)]
#endif
        [SerializeField]
		protected AnimationCurve blendingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
#if UseNA
        [Foldout(label_lerping)]
#endif
		[SerializeField, Min(0f), UnityEngine.Serialization.FormerlySerializedAs("debugLerpTime")]
		private float defaultLerpTime = 1f;

		[Header("Dialog Variables")]
#if UseNA
        [MinMaxSlider(0f, 1f), Foldout(label_lerping)]
#endif
		[SerializeField]
		protected Vector2 dialogPanningBounds = new Vector2(0.4f, 0.6f);
        
#if UseNA
		[MinMaxSlider(0f, 1f), Foldout(label_debug), ReadOnly]
#endif
        [SerializeField]
		protected Vector2 conversationPanningBounds = new Vector2(0.4f, 0.6f);

#if UseNA
        [Foldout(label_lerping)]
#endif
		[SerializeField, Tooltip("Time in seconds it takes to change position during dialog")]
		protected float dialogLerpTime = 1f;

#if UseNA
        [Foldout(label_lerping)]
#endif
		[SerializeField]
		protected float positionSnapshot = -1;
        
#if UseNA
        [ReadOnly, Foldout(label_lerping)]
#endif
		[SerializeField]
		protected bool dialogShowing;

        #endregion

        #region Manual Panning Variables

        protected const string label_manual = "Legacy Manual Panning";

#if UseNA
        [InfoBox("If the speed is set to 0, the user will not be able to manually pan the background", EInfoBoxType.Warning), Foldout(label_manual)]
#endif
		[SerializeField, Range(0f, .002f)]
		protected float panningSpeed;
#if UseNA
        [ReadOnly, Foldout(label_manual)]
#endif
		[SerializeField]
		protected Vector3 lastCursorPosition;
#if UseNA
        [ReadOnly, Foldout(label_manual)]
#endif
		[SerializeField]
		protected Vector3 inputPosition;
#if UseNA
        [ReadOnly, Foldout(label_manual)]
#endif
		[SerializeField]
		protected bool positionSetExternally;
        #endregion

        protected const string label_debug = "Debugging";
#if UseNA
        [Foldout(label_debug)]
#endif
		[SerializeField]
		protected bool doDebugText;
#if UseNA
        [DisableIf("debugDialogSwitching"), Foldout(label_debug)]
#endif
		[SerializeField]
		protected bool debugSwitching;
#if UseNA
        [DisableIf("debugSwitching"), Foldout(label_debug)]
#endif
		[SerializeField, ]
		protected bool debugDialogSwitching;
#if UseNA
        [Foldout(label_debug)]
#endif
		[SerializeField]
		protected bool fallbackMousePanning;

		protected virtual void OnEnable()
		{
			AltEnding.SpeakerVisualsManager.NewPanningPosition += DialogLerpToNewPosition;
			AltEnding.ArticyFlowController.FlowStarting += ArticyFlowController_FlowStarting;
			AltEnding.ArticyFlowController.FlowStarted += ArticyFlowController_FlowStarted;
			AltEnding.ArticyFlowController.FlowEnded += ArticyFlowController_FlowEnded;
		}

		protected virtual void OnDisable()
		{
			AltEnding.SpeakerVisualsManager.NewPanningPosition -= DialogLerpToNewPosition;
			AltEnding.ArticyFlowController.FlowStarting -= ArticyFlowController_FlowStarting;
			AltEnding.ArticyFlowController.FlowStarted -= ArticyFlowController_FlowStarted;
			AltEnding.ArticyFlowController.FlowEnded -= ArticyFlowController_FlowEnded;
		}
		void ArticyFlowController_FlowStarting()
		{
			positionSnapshot = panningPosition;
			conversationPanningBounds = GetRelativeDialogPanning(panningPosition);
		}

		void ArticyFlowController_FlowStarted()
		{
			dialogShowing = true;
		}

		void ArticyFlowController_FlowEnded()
		{
			dialogShowing = false;
			conversationPanningBounds = dialogPanningBounds;
			LerpToNewPosition(positionSnapshot);
		}


		protected virtual void Start()
		{
			conversationPanningBounds = dialogPanningBounds;
			SetNewPositionInternal(0.5f);
		}

		public void Update()
		{
			if (debugSwitching)
			{
#if ENABLE_LEGACY_INPUT_MANAGER
				if (Input.GetKeyDown(KeyCode.LeftArrow))
				{
					LerpLeft();
				}
				if (Input.GetKeyDown(KeyCode.RightArrow))
				{
					LerpRight();
				}
#endif
			}
			if (debugDialogSwitching)
			{
#if ENABLE_LEGACY_INPUT_MANAGER
				if (Input.GetKeyDown(KeyCode.LeftArrow))
				{
					DialogLerpToNewPosition(0f);
				}
				if (Input.GetKeyDown(KeyCode.RightArrow))
				{
					DialogLerpToNewPosition(1f);
				}
#endif
			}

			if (fallbackMousePanning && !dialogShowing)
			{
#if ENABLE_LEGACY_INPUT_MANAGER
				if (Input.GetMouseButtonDown(0))
				{
					OnBeginDrag();
				}
				else if (Input.GetMouseButton(0))
				{
					OnDragSimple();
				}
#endif
			}
		}

		/// <summary>
		/// Called when an external source sets the position, primarily manually panning during a HOG.
		/// </summary>
		/// <param name="newPosition"></param>
		public void SetNewPosition(float newPosition)
		{
			SetNewPositionInternal(newPosition);
			positionSetExternally = true;
		}

		protected void SetNewPositionInternal(float newPosition)
		{
			panningPosition = Mathf.Clamp01(newPosition);
			positionSetExternally = false;
			UpdatePosition();
		}

		/// <summary>
		/// Overridden by child scripts to provide actual functionality.
		/// </summary>
		protected virtual void UpdatePosition()
		{
			if (!enabled) return;
			if (doDebugText)
			{
				Debug.Log($"Update the position!\nPosition value: {panningPosition}\nTime: {Time.time}");
			}
		}

		#region Lerping

		[ContextMenu("Lerp Left")]
		public void LerpLeft()
		{
			LerpToNewPosition(0f);
		}

		[ContextMenu("Lerp Right")]
		public void LerpRight()
		{
			LerpToNewPosition(1f);
		}

		public void LerpToNewPosition(float newPosition, float lerpTime)
		{
            if (doDebugText)
            {
				Debug.Log($"Lerp to {newPosition} over {lerpTime} seconds.", this);
            }
#if UNITY_EDITOR
			if (editorLerpCoroutine != null) EditorCoroutineUtility.StopCoroutine(editorLerpCoroutine);
			if (!Application.isPlaying)
				editorLerpCoroutine = EditorCoroutineUtility.StartCoroutine(LerpPositionToNewValueCoroutine(newPosition, lerpTime), this);
			else
			{
#endif
				if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
				lerpCoroutine = StartCoroutine(LerpPositionToNewValueCoroutine(newPosition, lerpTime));
#if UNITY_EDITOR
			}
#endif
		}

		public void LerpToNewPosition(float newPosition)
		{
			LerpToNewPosition(newPosition, defaultLerpTime);
		}

#if UNITY_EDITOR
		private UnityEditor.EditorWindow window;
		private Unity.EditorCoroutines.Editor.EditorCoroutine editorLerpCoroutine;
#endif

		protected IEnumerator LerpPositionToNewValueCoroutine(float newPosition, float lerpTime)
		{
			float timeLeft = lerpTime;
			float positionStart = panningPosition;
			float lerpValue;
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				window = UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView"));
				window?.Repaint();
			}
#endif
			while (timeLeft > 0f)
			{
				lerpValue = Mathf.Clamp01((lerpTime - timeLeft) / lerpTime);
				SetNewPositionInternal(Mathf.Lerp(positionStart, newPosition, blendingCurve.Evaluate(lerpValue)));

#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					window?.Repaint();
				}
#endif
				yield return new WaitForEndOfFrame();
				timeLeft -= Time.deltaTime;
			}

			SetNewPositionInternal(newPosition);
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				window?.Repaint();
			}
			editorLerpCoroutine = null;
#endif
			lerpCoroutine = null;
		}

		#endregion

		#region Dialog

#if UseNA
        [Button()]
#endif
		public void DialogLerpLeft()
		{
			DialogLerpToNewPosition(0f);
		}

#if UseNA
        [Button()]
#endif
		public void DialogLerpRight()
		{
			DialogLerpToNewPosition(1f);
		}

		public void DialogLerpToNewPosition(float newPosition, float lerpTime)
		{
			LerpToNewPosition(Mathf.Lerp(conversationPanningBounds.x, conversationPanningBounds.y, newPosition), lerpTime);
		}

		public void DialogLerpToNewPosition(float newPosition)
		{
			DialogLerpToNewPosition(newPosition, dialogLerpTime);
		}

		/// <summary>
		/// This function is for if a dialog happens during a HOG, we want the panning to be based off of where you currently are in the scene, not the center or something else.
		/// </summary>
		private Vector2 GetRelativeDialogPanning(float centerPoint)
		{
			if (centerPoint < 0)
				centerPoint = panningPosition;

			Vector2 result = Vector2.up;

			float delta = Mathf.Clamp01(dialogPanningBounds.y - dialogPanningBounds.x);
			result.x = Mathf.Max(0f, centerPoint - (0.5f * delta));
			result.y = result.x + delta;

			if (result.y > 1)
			{
				result.y = 1;
				result.x = 1 - delta;
			}

			return result;
		}

		#endregion

		#region Fallback Legacy Dragging
		//I feel like this section could be separated out to its own "draggable panning" script that it optionally added.

		/// <summary>
		/// Called from an Event Trigger
		/// </summary>
		public void OnBeginDrag()
		{
			GetCursorPosition();
			if (doDebugText) Debug.Log($"Start Scene Drag: Last Cursor: {lastCursorPosition} Current Cursor: {inputPosition}", this);
			lastCursorPosition = inputPosition;
			StartDragAction?.Invoke();
		}

		/// <summary>
		/// Called from an Event Trigger
		/// </summary>
		public void OnDragSimple()
		{
			GetCursorPosition();
			SetNewPosition(panningPosition + (lastCursorPosition.x - inputPosition.x) * panningSpeed);
			lastCursorPosition = inputPosition;
		}

		protected void GetCursorPosition()
		{
			inputPosition = Vector3.zero;
#if ENABLE_LEGACY_INPUT_MANAGER
			inputPosition = Input.mousePosition;
#endif

		}

		#endregion

		#region During Setup/Editing


		#endregion
	}
}