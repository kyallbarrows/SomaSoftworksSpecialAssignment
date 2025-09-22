using AltEnding.Gallery;
using System.Collections.Generic;

namespace AltEnding.SaveSystem
{
    [System.Serializable]
    public class ProfileData
	{
        public long lastUpdated;
        public FlowHistoryProfileData flowHistoryProfileData;
        public GalleryManager.GalleryProfileData galleryProfileData;

        // the values defined in this constructor will be the default values
        // the game starts with when there's no data to load
        public ProfileData()
        {
            flowHistoryProfileData = new FlowHistoryProfileData();
        }
    }
}
