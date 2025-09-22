
namespace AltEnding.SaveSystem
{
	[System.Serializable]
	public class AnalyticsSaveData
	{
		public SerializableDictionary<string, int> locVisits;

		public AnalyticsSaveData()
		{
			locVisits = new SerializableDictionary<string, int>();
		}
	}
}
