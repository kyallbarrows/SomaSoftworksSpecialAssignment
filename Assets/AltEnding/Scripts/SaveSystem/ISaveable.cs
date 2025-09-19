
namespace AltEnding.SaveSystem
{
    public interface ISaveable
    {
        void ResetData();

        void SaveData(SaveData data);

        void LoadData(SaveData data);
    }
}
