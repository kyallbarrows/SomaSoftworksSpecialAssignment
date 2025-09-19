using System.Collections.Generic;
using Articy.Unity;

namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class FlowHistorySaveData
    {
        public List<string> visitedScenes;
        public List<CheckpointSceneData> visitedCheckpointScenes;
        public List<string> trackedLocalizedStrings;

        public FlowHistorySaveData()
        {
            visitedScenes = new List<string>();
            visitedCheckpointScenes = new List<CheckpointSceneData>();
            trackedLocalizedStrings = new List<string>();
        }

        [System.Serializable]
        public struct CheckpointSceneData
        {
            public ulong articyObjectID;
            public string foregroundArticyHexID;
            public string backgroundArticyHexID;
            public string locationSceneName;

            public CheckpointSceneData(AltEnding.MapNodeData mapNodeData)
            {
                articyObjectID = mapNodeData.articyObjectID;
                foregroundArticyHexID = mapNodeData.foregroundArticyHexID;
                backgroundArticyHexID = mapNodeData.backgroundArticyHexID;
                locationSceneName = mapNodeData.locationSceneName;
            }
            
            public CheckpointSceneData(ulong aObjectID, string locationSceneName)
            {
                articyObjectID = aObjectID;
                ArticyObject aObject = ArticyDatabase.GetObject(articyObjectID);
                if(aObject != null)
                {
                    CheckpointFeature checkpointFeature = ArticyStoryHelper.Instance.GetCheckpointFeature(aObject);
                    if(checkpointFeature != null && !string.IsNullOrWhiteSpace(checkpointFeature.foregroundArticyHexID))
                    {
                        foregroundArticyHexID = checkpointFeature.foregroundArticyHexID;
                    }
                    else
                    {
                        foregroundArticyHexID = "";
                    }
                    backgroundArticyHexID = "";
                }
                else
                {
                    foregroundArticyHexID = "";
                    backgroundArticyHexID = "";
                }
                this.locationSceneName = locationSceneName;
            }
        }
    }
}
