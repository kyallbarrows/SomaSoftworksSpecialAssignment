using System;

namespace AltEnding
{

	[Serializable]
	public enum EpisodeNumber
	{
		None = 0,
		Episode1 = 1,
		Episode2 = 2,
		Episode3 = 3,
		Episode4 = 4,
		Episode5 = 5
	}

	[Flags]
	public enum EpisodeNumberFlags
	{
		None = 0b00000,
		Episode1 = 0b00001,
		Episode2 = 0b00010,
		Episode3 = 0b00100,
		Episode4 = 0b01000,
		Episode5 = 0b10000,
		All = 0b11111
	}


	public static class EpisodeNumberUtility
	{
		public static bool IsEpisodeFlagSet(this EpisodeNumberFlags episodeNumberFlags, EpisodeNumber numberToCheck)
		{
			switch (numberToCheck)
			{
				case EpisodeNumber.Episode1:
				case EpisodeNumber.Episode2:
				case EpisodeNumber.Episode3:
				case EpisodeNumber.Episode4:
				case EpisodeNumber.Episode5:
					return episodeNumberFlags.HasFlag(numberToCheck);
				case EpisodeNumber.None:
					return (int)episodeNumberFlags == 0;
			}
			return false;
		}
    }
}
