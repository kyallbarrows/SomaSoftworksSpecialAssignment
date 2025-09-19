using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.CheckpointMap
{
    public class CheckpointMapVisualElement : MonoBehaviour
    {
        public enum VisitedState
        {
            NotVisited = 0,
            VisitedByProfile = 1,
            VisitedInPlaythrough = 2
        }

        const string stateVisuals = "State Visuals";
#if UseNA
        [OnValueChanged(nameof(SetVisitedVisuals))]
#endif
        [SerializeField] protected VisitedState currentState;
        public VisitedState CurrentState { get { return currentState; } }
#if UseNA
        [Foldout(stateVisuals)]
#endif
        [SerializeField] protected Animation myAnimation;
#if UseNA
        [Foldout(stateVisuals)]
#endif
        [SerializeField] protected AnimationClip notVisitedAnimClip;
#if UseNA
        [Foldout(stateVisuals)]
#endif
        [SerializeField] protected AnimationClip visitedByProfileAnimClip;
#if UseNA
        [Foldout(stateVisuals)]
#endif
        [SerializeField] protected AnimationClip visitedInPlaythroughAnimClip;

        protected bool EvaluateVisitedState(string aObjectID) => EvaluateVisitedState(aObjectID, ref currentState);

		protected bool EvaluateVisitedState(string aObjectID, ref VisitedState vState)
        {
            if (string.IsNullOrWhiteSpace(aObjectID) || aObjectID == "null" || !ArticyFlowHistoryTracker.instanceInitialized) return false;

            if (ArticyFlowHistoryTracker.Instance.HasSceneBeenVisited(aObjectID))
            {
                //This element was visited in the current playthrough
                vState = VisitedState.VisitedInPlaythrough;
            }
            else if (ArticyFlowHistoryTracker.Instance.HasSceneBeenVisitedByProfile(aObjectID))
            {
                //This scene was visited with the current profile, but not during this playthrough
                vState = VisitedState.VisitedByProfile;
            }
            else
            {
                //This scene has not been visited
                vState = VisitedState.NotVisited;
            }
            return true;
        }

        protected virtual void SetVisitedVisuals()
        {
            SetVisitedVisuals(currentState);
        }

        protected virtual void SetVisitedVisuals(VisitedState visitedState)
        {
            if (myAnimation != null)
            {
                switch (visitedState)
                {
                    case VisitedState.NotVisited:
                        SetVisuals(notVisitedAnimClip);
                        break;
                    case VisitedState.VisitedByProfile:
                        SetVisuals(visitedByProfileAnimClip);
                        break;
                    case VisitedState.VisitedInPlaythrough:
                        SetVisuals(visitedInPlaythroughAnimClip);
                        break;
                    default:
                        break;
                }
            }
        }

        protected void SetVisuals(AnimationClip visualStateClip)
        {
            if (visualStateClip == null) return;
            myAnimation.Play(visualStateClip.name);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                myAnimation.Sample();
            }
#endif
        }

        protected VisitedState MinimumVisitedState(VisitedState state1, VisitedState state2)
        {
            if (state1 == state2) return state1;

            if (state1 == VisitedState.NotVisited || state2 == VisitedState.NotVisited) 
                return VisitedState.NotVisited;

            if (state1 == VisitedState.VisitedByProfile || state2 == VisitedState.VisitedByProfile) 
                return VisitedState.VisitedByProfile;

            return VisitedState.VisitedInPlaythrough;
        }

		protected VisitedState MaximumVisitedState(VisitedState state1, VisitedState state2)
		{
			if (state1 == state2) return state1;

			if (state1 == VisitedState.VisitedInPlaythrough || state2 == VisitedState.VisitedInPlaythrough)
				return VisitedState.VisitedInPlaythrough;

			if (state1 == VisitedState.VisitedByProfile || state2 == VisitedState.VisitedByProfile)
				return VisitedState.VisitedByProfile;

			return VisitedState.NotVisited;
		}
	}
}
