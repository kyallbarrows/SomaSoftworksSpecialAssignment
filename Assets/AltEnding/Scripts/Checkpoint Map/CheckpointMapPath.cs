using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif
using Articy.Unity.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AltEnding.CheckpointMap
{
    /// <summary>
    /// This script provides helper functionality to set up paths in the editor more easily.
    /// </summary>
    public class CheckpointMapPath : CheckpointMapVisualElement
    {
        public enum EvalType
        {
            Primary = 0,
            Both = 1,
            Secondary = 2
        }
#if UNITY_EDITOR
#if UseNA
        [OnValueChanged(nameof(UpdatePrimaryNode)), Required]
#endif
        [SerializeField]
#endif
        protected CheckpointMapPoint primaryNode;
#if UNITY_EDITOR
#if UseNA
        [OnValueChanged(nameof(AlignTransform))]
#endif
        [SerializeField] 
        protected RectTransform lengthRect;

        [Header("Connected Node")]
#if UseNA
        [OnValueChanged(nameof(UpdateSecondaryNode)), Required, ValidateInput(nameof(AreNodesDifferent), "Primary and Secondary Nodes must be different")]
#endif
        [SerializeField]
#endif 
        
        protected CheckpointMapPoint secondaryNode;
#if UseNA
        [OnValueChanged(nameof(SetVisitedVisuals))]
#endif
        [SerializeField] 
        protected EvalType evaluationType;

#if UseNA
        [HideIf(nameof(evaluationType), EvalType.Primary), OnValueChanged(nameof(SetVisitedVisuals))]
#endif
        [SerializeField]
        protected VisitedState secondaryState;
#if UseNA
        [HideIf(nameof(evaluationType), EvalType.Primary)]
#endif
        [SerializeField] 
        protected Articy.Unity.ArticyRef connectedArticyObject;

        public string connectedArticyObjectId { get { return connectedArticyObject != null ? connectedArticyObject.id.ToHex() : "null"; } }

		private void OnEnable()
		{
            if (primaryNode != null)
            {
                primaryNode.visualStateUpdated += PrimaryNodeVisualStateUpdated;
                PrimaryNodeVisualStateUpdated(primaryNode.CurrentState);
            }
            if (secondaryNode != null)
            {
                secondaryNode.visualStateUpdated += SecondaryNodeVisualStateUpdated;
                SecondaryNodeVisualStateUpdated(secondaryNode.CurrentState);
            }
		}

		private void OnDisable()
		{
			if (primaryNode != null) primaryNode.visualStateUpdated -= PrimaryNodeVisualStateUpdated;
			if (secondaryNode != null) secondaryNode.visualStateUpdated -= SecondaryNodeVisualStateUpdated;
		}

        protected void PrimaryNodeVisualStateUpdated(VisitedState newState)
        {
            currentState = newState;
            SetVisitedVisuals();
        }

		protected void SecondaryNodeVisualStateUpdated(VisitedState newState)
		{
			secondaryState = newState;
			SetVisitedVisuals();
		}

		protected override void SetVisitedVisuals()
        {
            switch (evaluationType) {
                case EvalType.Secondary:
                    SetVisitedVisuals(secondaryState);
                    break;
                case EvalType.Both:
                    SetVisitedVisuals(MinimumVisitedState(currentState, secondaryState));
                    break;
                case EvalType.Primary:
                default:
                    SetVisitedVisuals(currentState);
                    break;
            }
        }

#if UNITY_EDITOR
        private void UpdatePrimaryNode()
		{
            AlignTransform();
		}

        private void UpdateSecondaryNode()
		{
            AlignTransform();
		}

        private bool AreNodesDifferent()
        {
            return primaryNode != secondaryNode;
        }

#if UseNA
        [Button]
#endif
        [ContextMenu("Align Transform to Nodes")]
        private void ManuallyAlignTransform()
        {
            Undo.RegisterFullObjectHierarchyUndo(transform, "Align Path");
            AlignTransform();
        }
        private void AlignTransform()
        {
            if (primaryNode == null) return;

            transform.position = primaryNode.transform.position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(transform);

            if (lengthRect == null)
            {
                lengthRect = (RectTransform)transform;
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
            if (lengthRect == null)
            {
                return;
            }

            if(secondaryNode == null)
            {
                lengthRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(lengthRect);
                return;
            }
            lengthRect.eulerAngles = GetAngle(primaryNode.transform, secondaryNode.transform);
            lengthRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Vector3.Magnitude(lengthRect.InverseTransformPoint(secondaryNode.transform.position)));
            PrefabUtility.RecordPrefabInstancePropertyModifications(lengthRect);
        }

        private Vector3 GetAngle(Transform pos1, Transform pos2)
        {
            Vector3 result = Vector3.zero;
            result.z = Vector3.SignedAngle(Vector3.right, pos1.InverseTransformPoint(pos2.position), Vector3.forward);
            return result;
        }

#endif
    }
}
