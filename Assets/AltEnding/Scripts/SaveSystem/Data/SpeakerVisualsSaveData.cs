namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class SpeakerVisualsSaveData
    {
        public SpeakerSaveData leftSpeakerSaveData;
        public SpeakerSaveData rightSpeakerSaveData;

        public SpeakerVisualsSaveData()
        {
            leftSpeakerSaveData = new SpeakerSaveData();
            rightSpeakerSaveData = new SpeakerSaveData();
        }

        [System.Serializable]
        public class SpeakerSaveData
		{
            public string speakerDPPHexID;
            public string expression;

			public SpeakerSaveData()
			{
                speakerDPPHexID = "";
                expression = "Dont_Change";
			}

			public SpeakerSaveData(string name, string expression)
			{
				this.speakerDPPHexID = name;
				this.expression = expression;
			}
		}
    }
}
