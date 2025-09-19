
namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class ArticyFlowSaveData
    {
        public SerializableDictionary<string, bool> boolVariables;
        public SerializableDictionary<string, int> intVariables;
        public SerializableDictionary<string, string> stringVariables;
        public ulong articyObjectID;
        public uint articyInstanceID;

        public ArticyFlowSaveData()
        {
            boolVariables = new SerializableDictionary<string, bool>();
            intVariables = new SerializableDictionary<string, int>();
            stringVariables = new SerializableDictionary<string, string>();
            articyObjectID = 0;
            articyInstanceID = 0;
        }
    }
}
