using AltEnding.Settings;

namespace AltEnding.GUI
{
    public class SettingsMenuCanvasManager : AnimatedCanvasManager
    {
		private bool saveOnTurnOff; //Used to prevent saving settings on start, before they've been loaded

		protected override void Start()
		{
			saveOnTurnOff = false;
			base.Start();
			saveOnTurnOff = true;
		}

		public override void TurnOff()
		{
			base.TurnOff();
			if (saveOnTurnOff && SettingsManager.instance_Initialised) SettingsManager.instance.SaveSettings();
		}

		public void ResetAudioSettings()
		{
			SettingsManager.instance.DefaultSoundSettings();
			SettingsManager.instance.ExternalInvokeSettingsResetEvent();
		}

		public void ResetGraphicsSettings()
		{
			SettingsManager.instance.DefaultGraphicsSettings();
			SettingsManager.instance.ExternalInvokeSettingsResetEvent();
		}

		public void ResetGameplaySettings()
		{
			SettingsManager.instance.DefaultGameplaySettings();
			SettingsManager.instance.ExternalInvokeSettingsResetEvent();
		}

		public void ResetDevSettings()
		{
			SettingsManager.instance.DefaultDevSettings();
			SettingsManager.instance.ExternalInvokeSettingsResetEvent();
		}
	}
}
