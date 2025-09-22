using AltEnding.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif
#if UseMasterAudio
using DarkTonic.MasterAudio;
#endif

namespace AltEnding.Gallery
{
	public class GalleryGUIController : MonoBehaviour
	{
		[SerializeField] protected AnimatedCanvasManager galleryCM;
		[SerializeField] protected AnimatedCanvasManager locationCM;
		[SerializeField] protected DialogueCanvasManager dialogueCM;
		[SerializeField] protected AudioListener galleryAL;
#if UseMasterAudio
		[SerializeField] protected PlaylistController galleryPlaylistController;
#endif
        
		void Start()
		{
			galleryCM.TurnOn();
			locationCM.TurnOff();
			dialogueCM.TurnOff();
		}

		#region Location Loading
		Coroutine loadingCoroutine = null;
#if UseNA
        [ReadOnly]
#endif
		[SerializeField] Location currentLocation = null;

		public void LoadLocation(Location newLocation)
		{
			//Validate the new location.  Make sure it has a set scene name.
			//Because the scene name is set through a NaughtyAttributes 'Scene' attribute, the default value is the first scene in the build settings: "MainMenu"
			if (newLocation == null || string.IsNullOrWhiteSpace(newLocation.sceneName) || newLocation.sceneName == "MainMenu") return;
			if (currentLocation == null && loadingCoroutine == null)
			{
				currentLocation = newLocation;
				loadingCoroutine = StartCoroutine(LoadLocation(newLocation.sceneName));
			}
		}

		IEnumerator LoadLocation(string sceneName)
		{
			//Hide gallery UI
			galleryCM.TurnOff();

#if UseMasterAudio
			//Fade out gallery music
			if (galleryPlaylistController != null)
			{
				galleryPlaylistController.FadeToVolume(0f, 0.5f);
				yield return new WaitForSeconds(0.5f);
				galleryPlaylistController.PausePlaylist();
				galleryPlaylistController.gameObject.SetActive(false);
			}
#endif

			//Fade out
			if (ScreenFade.instance_Initialised) yield return ScreenFade.instance.StartAndReturnFade(0.5f, Color.black);

			//Load location scene
			yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

			//Turn on location UI
			locationCM.TurnOn();
			dialogueCM.TurnOff();

			//Disable gallery audio listener (so there aren't two while the location is loaded)
			if (galleryAL) galleryAL.enabled = false;

			//Fade back in
			if (ScreenFade.instance_Initialised) yield return ScreenFade.instance.EndAndReturnFade(0.5f);

			loadingCoroutine = null;
		}

		public void UnloadCurrentLocation()
		{
			if (loadingCoroutine == null)
			{
				loadingCoroutine = StartCoroutine(UnloadLocation(currentLocation.sceneName));
			}
		}

		IEnumerator UnloadLocation(string sceneName)
		{
			//Turn off locations UIs
			locationCM.TurnOff();
			dialogueCM.TurnOff();

			//Fade out
			if (ScreenFade.instance_Initialised) yield return ScreenFade.instance.StartAndReturnFade(0.5f, Color.black);

			//Unload location scene
			yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

			//Turn on gallery UI
			galleryCM.TurnOn();

#if UseMasterAudio
			//Fade gallery music back in
			if (galleryPlaylistController != null)
			{
				galleryPlaylistController.gameObject.SetActive(true);
				galleryPlaylistController.UnpausePlaylist();
				galleryPlaylistController.FadeToVolume(1, 0.5f);
			}
#endif

			//Re-enable gallery audio listener
			if (galleryAL) galleryAL.enabled = true;

			//Clear current location
			currentLocation = null;

			//Fade back in
			if (ScreenFade.instance_Initialised) yield return ScreenFade.instance.EndAndReturnFade(0.5f);

			loadingCoroutine = null;
		}
		#endregion
	}
}
