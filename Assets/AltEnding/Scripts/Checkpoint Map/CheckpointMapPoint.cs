using System;

namespace AltEnding.CheckpointMap
{
    public class CheckpointMapPoint : CheckpointMapVisualElement
    {
        public Action<VisitedState> visualStateUpdated;

		/// <summary>
		/// After updating the visuals on this object, if there are any listeners, tell them to update too.
		/// </summary>
		protected override void SetVisitedVisuals(VisitedState visitedState)
		{
			base.SetVisitedVisuals(visitedState);
			visualStateUpdated?.Invoke(visitedState);
		}
	}
}
