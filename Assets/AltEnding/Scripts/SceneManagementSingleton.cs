using System.Collections;
using System.Collections.Generic;
using Articy.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UseNA
using NaughtyAttributes;
#endif
using AltEnding.Gallery;
using AltEnding.GUI;
using AltEnding.SaveSystem;

namespace AltEnding
{
    public class SceneManagementSingleton : PersistentSingleton<SceneManagementSingleton>, ISaveable
    {
        public static System.Action<string> newLocationLoaded;

#if UseNA
        [Scene]
#endif
        public string mainScene = "Core UI";
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] private string nextScene;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] protected string currentLocationScene;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] protected string loadingLocationScene;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField] protected bool waitForSaveLoad;

        public string CurrentLocationScene
        {
            get { return currentLocationScene; }
        }
        
        private DialogueCanvasManager myDialogCanvasManager;

        protected Coroutine loadingCoroutine = null;

        private void OnEnable()
        {
            ArticyFlowController.NewFlowObject += ArticyFlowController_NewFlowObject;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void OnDisable()
        {
            ArticyFlowController.NewFlowObject -= ArticyFlowController_NewFlowObject;
            SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
        }

        protected void ArticyFlowController_NewFlowObject(IFlowObject aObject)
        {
            if (waitForSaveLoad)
                return;

            string background = ArticyStoryHelper.Instance.GetBackgroundDescription(aObject);

            // "Dont_Change" is the signal to not load a scene
            if (background.Equals("Dont_Change"))
                return;

            // Check if the new background is the current
            if (!background.Equals(currentLocationScene))
                LoadNewLocation(background);
        }

        private void SceneManager_sceneUnloaded(Scene unloadedScene)
        {
            // We need to reset the current scene when the UI scene is unloaded
            if (unloadedScene.name == currentLocationScene)
            {
                currentLocationScene = "";
            }
        }

        public void LoadScene()
        {
            if (!string.IsNullOrEmpty(nextScene))
            {
                SceneManager.LoadScene(nextScene);
                nextScene = "";
            }
        }

        public void LoadSceneWithTransition(string sceneName)
        {
            if (loadingCoroutine == null)
            {
                loadingCoroutine = StartCoroutine(SceneTransitionLoading(sceneName));
            }
        }

        IEnumerator SceneTransitionLoading(string newSceneName)
        {
            yield return StartFade(0.5f);
            yield return SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Single);
            yield return new WaitForSeconds(0.1f); //A little extra buffer time for the scene unloading process to finish
            yield return EndFade(0.5f);
            loadingCoroutine = null;
        }

        public void LoadScene(string sceneName, LoadSceneMode mode)
        {
            SceneManager.LoadScene(sceneName, mode);
        }

        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        #region Changing Location
        public void LoadNewLocation(string newLocationSceneName, IEnumerator extraLoadProcess = null)
        {
            if (loadingCoroutine == null)
            {
                ChangeDialogBlockingStatus(true);
                loadingCoroutine = StartCoroutine(LocationLoading(newLocationSceneName, extraLoadProcess));
                if (GalleryManager.instance_Initialised)
                    GalleryManager.instance.UnlockLocationWithSceneName(newLocationSceneName);
            }
        }

        IEnumerator LocationLoading(string newLocationSceneName, IEnumerator extraLoadProcess = null)
        {
            loadingLocationScene = newLocationSceneName;
            yield return StartFade(0.5f);
            if (!string.IsNullOrEmpty(currentLocationScene) &&
                SceneManager.GetSceneByName(currentLocationScene).IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(currentLocationScene,
                    UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            }

            yield return SceneManager.LoadSceneAsync(newLocationSceneName, LoadSceneMode.Additive);
            if (extraLoadProcess != null) yield return extraLoadProcess;
            currentLocationScene = newLocationSceneName;
            newLocationLoaded?.Invoke(newLocationSceneName);
            yield return
                new WaitForSeconds(0.1f); //A little extra buffer time for the scene unloading process to finish
            yield return EndFade(0.5f);
            ChangeDialogBlockingStatus(false);
            loadingLocationScene = null;
            loadingCoroutine = null;
        }

        private void ChangeDialogBlockingStatus(bool isBlocking)
        {
            if (myDialogCanvasManager == null)
            {
                if (!AltEnding.SpeakerVisualsManager.instance_Initialised) return;

                myDialogCanvasManager = AltEnding.SpeakerVisualsManager.instance.myDialogCanvasManager;
            }

            if (myDialogCanvasManager != null)
            {
                if (isBlocking)
                {
                    myDialogCanvasManager.AddContinueBlocker(this);
                }
                else
                {
                    myDialogCanvasManager.RemoveContinueBlocker(this);
                }
            }
        }
        #endregion

        #region Play Game Functionality
        public void PlayNewGame()
        {
            SaveManager.Instance.NewGame();
            Play(null);
        }

        public void PlayFromContinue()
        {
            waitForSaveLoad = true;
            Play(SaveManager.currentFileName);
        }

        public void PlayFromMapNode(string saveName)
        {
            waitForSaveLoad = true;
            Play(saveName);
        }

        protected void Play(string saveName)
        {
            if (loadingCoroutine == null)
                loadingCoroutine = StartCoroutine(PlayWithSave(saveName));
        }

        IEnumerator PlayWithSave(string saveName)
        {
            yield return StartFade(0.5f);
            yield return SceneManager.LoadSceneAsync(mainScene, LoadSceneMode.Single);
            if (!string.IsNullOrEmpty(saveName))
                SaveManager.Instance.LoadGame(saveName);
            loadingCoroutine = null;
        }
        #endregion

        #region Screen Fade Helpers
        IEnumerator StartFade(float fadeTime)
        {
            if (ScreenFade.instance_Initialised && !ScreenFade.instance.FadeFirstHalf)
            {
                ScreenFade.instance.StartFade(fadeTime);
                yield return new WaitUntil(() => ScreenFade.instance.CurrentFadeStatus == TransitionStatus.Hold);
            }
        }

        IEnumerator EndFade(float fadeTime)
        {
            if (ScreenFade.instance_Initialised && !ScreenFade.instance.FadeSecondHalf)
            {
                ScreenFade.instance.EndFade(fadeTime);
                yield return new WaitUntil(() => ScreenFade.instance.CurrentFadeStatus == TransitionStatus.Complete);
            }
        }
        #endregion

        #region ISaveable Implementation
        public void ResetData()
        {

        }

        public void SaveData(SaveData data)
        {
            //If a new scene is loading, save that as the current location, otherwise save the current location
            data.currentLocation = string.IsNullOrWhiteSpace(loadingLocationScene)
                ? currentLocationScene
                : loadingLocationScene;
        }

        public void LoadData(SaveData data)
        {
            waitForSaveLoad = false;
            if (!SaveManager.loadDataOnly) LoadNewLocation(data.currentLocation);
        }
        #endregion

        #region Converting Scene Names To Location Addresses
        Dictionary<string, string> backgroundToLocationAddressDict = new Dictionary<string, string>()
        {
            // Add in mappings from background label to Articy Location hex id
        };

        public string GetLocationDataArticyHexIDForCurrentScene() =>
            GetLocationDataArticyHexIDForScene(currentLocationScene);

        public string GetLocationDataArticyHexIDForScene(string sceneName)
        {
            backgroundToLocationAddressDict.TryGetValue(sceneName, out string address);
            return address;
        }
        #endregion
    }
}