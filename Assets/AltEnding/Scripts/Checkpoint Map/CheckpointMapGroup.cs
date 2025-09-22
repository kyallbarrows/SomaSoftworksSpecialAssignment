using System.Collections.Generic;
using UnityEngine;

namespace AltEnding.CheckpointMap
{
    public class CheckpointMapGroup : CheckpointMapPoint
    {
        [SerializeField] protected List<CheckpointMapPoint> myPoints;
#if UNITY_EDITOR
		[ContextMenuItem("Get Nodes In Children", nameof(GetNodesInChildren))]
		[SerializeField] private Transform nodesParent;
#endif

		private void OnEnable()
		{
			foreach (CheckpointMapPoint point in myPoints)
			{
				if(point != null) point.visualStateUpdated += UpdateVisualState;
			}
			UpdateVisualState(currentState);
			SetVisitedVisuals(currentState);
		}

		private void OnDisable()
		{
			foreach (CheckpointMapPoint point in myPoints)
			{
				if (point != null) point.visualStateUpdated -= UpdateVisualState;
			}
		}

		private void UpdateVisualState(VisitedState state)
		{
			VisitedState newState = VisitedState.NotVisited;
			foreach (CheckpointMapPoint point in myPoints)
			{
				newState = MaximumVisitedState(newState, point.CurrentState);
				if (newState == VisitedState.VisitedInPlaythrough) break;
			}
			if (currentState != newState)
			{
				currentState = newState;
				SetVisitedVisuals(currentState);
			}
		}

#if UNITY_EDITOR
		private void GetNodesInChildren()
		{
			if (nodesParent != null)
			{
				if (myPoints == null) myPoints = new List<CheckpointMapPoint>();

				CheckpointMapPoint childPoint;
				for (int i = 0; i < nodesParent.childCount; i++)
				{
					childPoint = nodesParent.GetChild(i).GetComponent<CheckpointMapPoint>();

					if (childPoint != null && !myPoints.Contains(childPoint))
					{
						myPoints.Add(childPoint);
					}
				}
			}
		}
#endif
	}
}
