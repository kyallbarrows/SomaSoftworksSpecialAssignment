
namespace AltEnding.SaveSystem
{
    public interface IProfileSaveable
	{
        void ResetProfileData();

        void SaveProfileData(ProfileData data);

        void LoadProfileData(ProfileData data);
    }
}
