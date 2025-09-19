using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UseNA
using NaughtyAttributes;
#endif
using System;

namespace AltEnding.GUI
{
	public class AnimatedCanvasManager : GenericCanvasManager
	{
		public enum FunctionTriggerStyle
		{
			[InspectorName("Trigger Normally")] None,
			[InspectorName("Trigger when Animation stops")] Is_Animation_Playing,
			[InspectorName("Trigger from Animation Event")] Animation_Event
		}


		[Header("Transition Animations")]
		[SerializeField] protected Animation animationComponent;
		[SerializeField] protected AnimationClip turnOnClip;
		[SerializeField] protected AnimationClip turnOffClip;
		[SerializeField] protected FunctionTriggerStyle outroTriggerStyle = FunctionTriggerStyle.Is_Animation_Playing;

#if UseNA
        [ReadOnly]
#endif
		[SerializeField] protected bool isInTransition;
#if UseNA
        [ReadOnly]
#endif
		[SerializeField] protected bool outroAnimationPlaying;
		[SerializeField] protected UnityEvent callbackEvent;
		protected List<Action> callbackList = new List<Action>();


#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			if (animationComponent == null) animationComponent = GetComponent<Animation>();
			if (animationComponent != null)
			{
				if (turnOnClip != null && animationComponent.GetClip(turnOnClip.name) == null) animationComponent.AddClip(turnOnClip, turnOnClip.name);
				if (turnOffClip != null && animationComponent.GetClip(turnOffClip.name) == null) animationComponent.AddClip(turnOffClip, turnOffClip.name);
			}
		}
#endif

		protected override void Start()
		{
			switch (currentOpenState)
			{
				case OpenState.Open:
					BaseTurnOn();
					break;
				case OpenState.Opening:
					currentOpenState = OpenState.Closed;
					TurnOn();
					break;
				case OpenState.Closing:
					currentOpenState = OpenState.Open;
					TurnOff();
					break;
				case OpenState.Closed:
				default:
					BaseTurnOff();
					break;
			}
		}

		protected void LateUpdate()
		{
			if (isInTransition)
			{
				isInTransition = animationComponent.isPlaying;
				if (!isInTransition){
                    switch (currentOpenState)
                    {
						case OpenState.Opening:
							currentOpenState = OpenState.Open;
							break;
						case OpenState.Closing:
							currentOpenState = OpenState.Closed;
							break;
                    }
					if ((outroTriggerStyle == FunctionTriggerStyle.Is_Animation_Playing || outroTriggerStyle == FunctionTriggerStyle.Animation_Event) && outroAnimationPlaying)
					{
						FinishOutro();
					}
					ProcessCallbackList();
				}
			}
		}

		protected void ProcessCallbackList()
		{
			if(callbackEvent != null)
			{
				callbackEvent.Invoke();
				callbackEvent.RemoveAllListeners();
			}
			for (int c = callbackList.Count - 1; c >= 0; c--)
			{
				callbackList[c]?.Invoke();
				callbackList.RemoveAt(c);
			}
		}

		public override void TurnOn()
		{
			if (doDebugs) Debug.Log($"AnimatedCanvasManger[{gameObject.name}].TurnOn();\nCurrent State: {currentOpenState.ToString()}; Is In Transition: {isInTransition.ToString()}; Animation Component: {(animationComponent != null ? animationComponent.gameObject.name : "Null")}; Turn On Clip: {(turnOnClip != null ? turnOnClip.name : "Null")}");
			if (currentOpenState.OpenOrOpening()) return;
			//if (!isInTransition)
			//{
				base.TurnOn();
				if (animationComponent != null && turnOnClip != null)
				{
					currentOpenState = OpenState.Opening;
					isInTransition = true;
					animationComponent.Play(turnOnClip.name);
				}
			//}
		}

		public override void TurnOn(Action callback)
		{
			if (currentOpenState.OpenOrOpening()) return;
			isInTransition = true;
			callbackList.Add(callback);
			TurnOn();
		}

		public override void TurnOff()
		{
			if (currentOpenState.ClosedOrClosing()) return;
			//if (!isInTransition)
			//{
				if (animationComponent != null && turnOffClip != null)
				{
					isInTransition = true;
					currentOpenState = OpenState.Closing;
					outroAnimationPlaying = true;
					animationComponent.Play(turnOffClip.name);
					if (outroTriggerStyle == FunctionTriggerStyle.None)
					{
						BaseTurnOff();
						outroAnimationPlaying = false;

					}
				}
				else
				{
					BaseTurnOff();
				}
			//}
		}

		public override void TurnOff(Action callback)
		{
			if (currentOpenState.ClosedOrClosing()) return;
			isInTransition = true;
			callbackList.Add(callback);
			TurnOff();
		}

		public void FinishOutro()
		{
			BaseTurnOff();
			outroAnimationPlaying = false;
		}

		/// <summary>
		/// For derived clases to be able to call the basic turn on functionality, skipping any transitions.
		/// </summary>
		protected void BaseTurnOn()
		{
			base.TurnOn();
		}

		/// <summary>
		/// For derived clases to be able to call the basic turn off functionality, skipping any transitions.
		/// </summary>
		protected void BaseTurnOff()
		{
			base.TurnOff();
		}

		#region Inspector QOL
#if UNITY_EDITOR
		private bool animationComponentIsSet_EditorOnly => animationComponent != null;
#if UseNA
        [NaughtyAttributes.HideIf("animationComponentIsSet_EditorOnly"), NaughtyAttributes.Button("Add Animation Component")]
#endif
		private void AddCanvas_EditorOnly()
		{
			animationComponent = gameObject.AddComponent<Animation>();
			animationComponent.playAutomatically = false;
			OnValidate();
		}
#endif
		#endregion
	}
}
