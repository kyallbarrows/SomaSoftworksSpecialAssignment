using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    public class ScrubLegacyAnimation : MonoBehaviour
    {
#if UseNA
        [Required]
#endif
        [SerializeField] protected Animation animationComponent;

        [Header("Motion Variables")]
#if UseNA
        [Required("Panning Clip needs to be set!")]
#endif
        [SerializeField]
        protected AnimationClip panningClip;

        [Header("Manual Panning")]
#if UseNA
        [Label("Current Blended Position"), OnValueChanged(nameof(UpdateFromBlend))]
#endif
        [SerializeField, Range(0f, 1f)]
        protected float blendedPosition;
#if UseNA
        [CurveRange(0, 0, 1, 1), Label("Blending Curve")]
#endif
        [SerializeField]
        protected AnimationCurve blendingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
#if UseNA
        [Label("Raw Position (Normalized)"), OnValueChanged(nameof(UpdatePosition))]
#endif
        [SerializeField, Range(0f, 1f)]
        protected float normalizedPosition;

        protected AnimationState currentState;
        protected Coroutine lerpCoroutine;

        [Header("Debugging")]
        [SerializeField] protected bool doDebugText;

        #region NaughtyHelpers

        private bool ClipSet(AnimationClip clip)
        {
            return clip != null;
        }

        #endregion

        private void OnValidate()
        {
            if (animationComponent == null)
                animationComponent = GetComponent<Animation>();

            if (animationComponent.clip == null && panningClip != null)
            {
                animationComponent.clip = panningClip;
                if (animationComponent.GetClip(panningClip.name) == null)
                    animationComponent.AddClip(panningClip, panningClip.name);
            }

            if (panningClip == null && animationComponent.clip != null)
            {
                panningClip = animationComponent.clip;
            }

            if (panningClip == null)
            {
                enabled = false;
                return;
            }

            if (currentState == null)
                GetState();
            if (blendingCurve == null)
                blendingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private void Start()
        {
            GetState();
            UpdatePosition();
        }

        private void GetState()
        {
            if (!animationComponent.isPlaying)
            {
                animationComponent.Play(panningClip.name);
            }

            foreach (AnimationState aState in animationComponent)
            {
                if (aState.clip == panningClip)
                {
                    currentState = aState;
                    break;
                }
            }

            if (currentState != null)
            {
                currentState.speed = 0;
            }
        }

        protected void UpdatePosition()
        {
            if (doDebugText)
            {
                Debug.Log($"Update the position!\nPosition value: {normalizedPosition}\nTime: {Time.time}");
            }

            if (currentState == null)
                GetState();

            if (currentState != null)
            {
                currentState.normalizedTime = normalizedPosition; //blendingCurve.Evaluate(panningPosition);
                currentState.speed = 0;
                currentState.enabled = true;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    animationComponent.Sample();
                }
#endif
            }
        }

        public void BlendNewPosition(float newPosition)
        {
            blendedPosition = Mathf.Clamp01(newPosition);
            UpdateFromBlend();
        }

        protected void UpdateFromBlend()
        {
            SetNewPosition(blendingCurve.Evaluate(blendedPosition));
        }

        public void SetNewPosition(float newPosition)
        {
            normalizedPosition = Mathf.Clamp01(newPosition);
            UpdatePosition();
        }
    }
}