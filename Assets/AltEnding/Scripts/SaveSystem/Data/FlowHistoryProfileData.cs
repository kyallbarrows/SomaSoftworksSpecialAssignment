using System.Collections.Generic;
using Articy.Unity.Utils;

namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class FlowHistoryProfileData
	{
        public List<string> visitedScenes;

        public FlowHistoryProfileData()
        {
            visitedScenes = new List<string>();
        }
    }
}
