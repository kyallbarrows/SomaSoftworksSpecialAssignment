using System.Collections.Generic;

namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public long lastUpdated;
        public ArticyFlowSaveData storySaveData;
        public string currentLocation;
        public FlowHistorySaveData flowHistorySaveData;
        public SerializableDictionary<string, bool> notesUnlocked;
        public SpeakerVisualsSaveData speakerVisualsSaveData;
        public AnalyticsSaveData analyticsSaveData;

        // the values defined in this constructor will be the default values
        // the game starts with when there's no data to load
        public SaveData()
        {
            storySaveData = new ArticyFlowSaveData();
            flowHistorySaveData = new FlowHistorySaveData();
            notesUnlocked = new SerializableDictionary<string, bool>();
            speakerVisualsSaveData = new SpeakerVisualsSaveData();
            analyticsSaveData = new AnalyticsSaveData();
        }
    }
}
