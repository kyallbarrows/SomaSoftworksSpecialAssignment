using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AltEnding.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        public static Action saving;
        public static Action saved;
        public static Action loading;
        public static Action loaded;

		public static Action savingProfile;
		public static Action savedProfile;
		public static Action loadingProfile;
		public static Action loadedProfile;

		[Header("Helpful Functionality")]
        [SerializeField] private bool autoInitializeDataIfNull = false;
        [SerializeField] private bool autoLoadCurrentSaveOnStart = false;

        [Header("File Storage Config")]
        [SerializeField] private string currentProfileID = "DefaultProfile";
        public const string currentFileName = "Current";
        [SerializeField] private bool useEncryption;

        [Header("Last Save Data")]
        [SerializeField] private SaveData saveData;
        public SaveData currentSaveData { get { return saveData; } }
		[SerializeField] private ProfileData profileData;
        public ProfileData currentProfileData { get { return profileData; } }

        private JSONFileSaveHandler saveHandler;
        public bool gatheringSaveObjects { get; protected set; }
        private List<ISaveable> saveableObjects;
        private List<IProfileSaveable> profileSaveableObjects;
        Coroutine saveLoadCoroutine;
        Coroutine profileSaveLoadCoroutine;
        private const string profileDataFileName = "profile";

        public static SaveManager Instance { get; private set; }
        public static bool Initialized => Instance != null;

        [Header("Debugging")]
        [SerializeField] private bool doDebugs;

        public static bool loadDataOnly { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Found more than one Save Manager in the scene. Destroying the newest one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveHandler = new JSONFileSaveHandler(Application.persistentDataPath, useEncryption);

            ChangeSelectedProfileToMostRecent();
        }

        private void Start()
        {
            if (autoLoadCurrentSaveOnStart) LoadGame(currentProfileID, currentFileName, true);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(GetDataPersistenceObjects());
        }

		#region Managing Profiles

		public void ChangeSelectedProfile(string newProfileId)
        {
            // Update the profile to use for saving and loading
            currentProfileID = newProfileId;
        }

        [ContextMenu("Change Selected Profile To Most Recent")]
        private void ChangeSelectedProfileToMostRecent()
        {
            string mostRecentProfileID = saveHandler.GetMostRecentlyUpdatedProfileID();
            if (!string.IsNullOrWhiteSpace(mostRecentProfileID))
            {
				ChangeSelectedProfile(mostRecentProfileID);
            }
        }

        //New profile
#if UseNA
		[NaughtyAttributes.Button()]
#endif
		public void NewProfile()
		{
			profileData = new ProfileData();
            
            string debugString = "[SM] Resetting profile objects:";
            foreach (IProfileSaveable saveableObj in profileSaveableObjects)
            {
                debugString += $"\n{saveableObj}";
                saveableObj.ResetProfileData();
            }
			if (doDebugs) Debug.Log(debugString);
        }

		//Save profile
#if UseNA
		[NaughtyAttributes.Button()]
#endif
		public void SaveProfile() => SaveProfile(currentProfileID);
		public void SaveProfile(string profileID)
		{
			if (profileSaveLoadCoroutine == null) profileSaveLoadCoroutine = StartCoroutine(SaveProfileCoroutine(profileID));
			else if (doDebugs) Debug.LogWarning("[SM] Could not save game because a save or load is already in progress.");
		}

		public IEnumerator SaveProfileCoroutine(string profileID)
        {
			//Wait if the save manager hasn't finished getting all the save objects in the scene
			//This may cause additional save calls to be ignored.  For example, a save made for a checkpoint could cause the save to the current file that follows it to fail
			if (gatheringSaveObjects) yield return new WaitWhile(() => gatheringSaveObjects);

			System.Text.StringBuilder debugMessage = new System.Text.StringBuilder($"[SM] Saving profile with ID: {profileID}");

			// if we don't have any data to save, log a warning here
			if (profileData == null)
			{
				//Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.", this);
				//return;

				profileData = new ProfileData();
			}

			savingProfile?.Invoke();

			// pass the data to other scripts so they can update it
			foreach (IProfileSaveable saveableObj in profileSaveableObjects)
			{
				debugMessage.Append($"\nSaving profile object: {saveableObj}");
				saveableObj.SaveProfileData(profileData);
			}

			if (doDebugs) Debug.Log(debugMessage.ToString());
			// timestamp the data so we know when it was last saved
			profileData.lastUpdated = System.DateTime.Now.ToBinary();

			// save that data to a file using the data handler
			saveHandler.Save(profileData, profileID.Trim(), profileDataFileName);

			savedProfile?.Invoke();

			profileSaveLoadCoroutine = null;
		}

		//Load profile
#if UseNA
		[NaughtyAttributes.Button()]
#endif
		public void LoadProfile() => LoadProfile(currentProfileID);

		public void LoadProfile(string profileID)
		{
			if (profileSaveLoadCoroutine == null) profileSaveLoadCoroutine = StartCoroutine(LoadProfileCoroutine(profileID));
			else if (doDebugs) Debug.LogWarning("[SM] Could not load game because a save or load is already in progress.");
		}

		public IEnumerator LoadProfileCoroutine(string profileID)
		{
			if (gatheringSaveObjects) yield return new WaitWhile(() => gatheringSaveObjects);

			if (doDebugs) Debug.Log($"[SM] Loading profile with ID: {profileID}");

			// load any saved data from a file using the data handler
			profileData = saveHandler.Load<ProfileData>(profileID, profileDataFileName);

            //Record if we made a new profiel so we can save it after loading is finished (becauese it can't save while loading is still ongoing)
            bool madeProfile = false;

			// start a new game if the data is null and we're configured to initialize data for debugging purposes
			if (profileData == null && autoInitializeDataIfNull)
			{
				NewProfile();
                madeProfile = true;
			}

			// if no data can be loaded, don't continue
			if (profileData == null)
			{
				if (doDebugs) Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
				profileSaveLoadCoroutine = null;
				yield break;
			}

			loadingProfile?.Invoke();

			//Set current profile & file to match the loaded file
			//currentProfileID = profileID; //Don't do this, it messes with dev saves, which should every be the current profile

			// push the loaded data to all other scripts that need it
			foreach (IProfileSaveable dataPersistenceObj in profileSaveableObjects)
			{
				dataPersistenceObj.LoadProfileData(profileData);
			}

			loadedProfile?.Invoke();
			profileSaveLoadCoroutine = null;

            //If we made a profile, we can save it now (so it exists for future profile load attempts)
            if (madeProfile) SaveProfile();
		}

		[ContextMenu("Delete Current Profile")]
		public void DeleteCurrentProfile() => DeleteProfile(currentProfileID);

		public void DeleteProfile(string profileId)
		{
			// delete the data for this profile id
			saveHandler.DeleteProfile(profileId);
			if (currentProfileID == profileId)
			{
				// initialize the selected profile id
				ChangeSelectedProfileToMostRecent();
				// reload the game so that our data matches the newly selected profile id
				LoadGame();
			}
		}
		#endregion

		#region Managing Game Saves

        //New save game
#if UseNA
		[NaughtyAttributes.Button("New Game")]
#endif
        public void NewGame()
        {
            saveData = new SaveData();
            string debugString = "Resetting objects:";
            foreach (ISaveable saveableObj in saveableObjects)
            {
                debugString += $"\n{saveableObj}";
                saveableObj.ResetData();
            }
			if (doDebugs) Debug.Log(debugString);
        }

        //Saving save game
#if UseNA
		[NaughtyAttributes.Button("Save Game")]
#endif
        public void SaveGame() => SaveGame(currentProfileID, currentFileName);
        public void SaveGame(string fileName) => SaveGame(currentProfileID, fileName);

        public void SaveGame(string profileID, string fileName)
        {
            if (saveLoadCoroutine == null)
            {
                saveLoadCoroutine = StartCoroutine(SaveCoroutine(profileID, fileName));
                SaveProfile(profileID);
            }
            else if (doDebugs) Debug.LogWarning("[SM] Could not save game because a save or load is already in progress.");
        }

        public IEnumerator SaveCoroutine(string profileID, string fileName)
		{
            //Wait if the save manager hasn't finished getting all the save objects in the scene
            //This may cause additional save calls to be ignored.  For example, a save made for a checkpoint could cause the save to the current file that follows it to fail
            if (gatheringSaveObjects) yield return new WaitWhile(() => gatheringSaveObjects);

            System.Text.StringBuilder debugMessage = new System.Text.StringBuilder($"[SM] Saving articy flow state with profile ID: {profileID} and file name: {fileName}");

            // if we don't have any data to save, log a warning here
            if (saveData == null)
            {
				//if (doDebugs) Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.", this);
				//return;

				saveData = new SaveData();
            }

            saving?.Invoke();

            // pass the data to other scripts so they can update it
            foreach (ISaveable saveableObj in saveableObjects)
            {
                debugMessage.Append($"\nSaving object: {saveableObj}");
                saveableObj.SaveData(saveData);
            }

			if (doDebugs) Debug.Log(debugMessage.ToString());
            // timestamp the data so we know when it was last saved
            saveData.lastUpdated = System.DateTime.Now.ToBinary();

            // save that data to a file using the data handler
            saveHandler.Save(saveData, profileID.Trim(), fileName.Trim());

            saved?.Invoke();

            saveLoadCoroutine = null;
        }

        //Load save game
#if UseNA
		[NaughtyAttributes.Button("Load Game")]
#endif
        public void LoadGame() => LoadGame(currentProfileID, currentFileName, false);
        public void LoadGame(string fileName) => LoadGame(currentProfileID, fileName, false);

        public void LoadGame(string profileID, string fileName, bool loadDataOnly)
        {
            if (saveLoadCoroutine == null)
            {
                saveLoadCoroutine = StartCoroutine(LoadCoroutine(profileID, fileName, loadDataOnly));
                LoadProfile(profileID);
            }
            else if (doDebugs) Debug.LogWarning("[SM] Could not load game because a save or load is already in progress.");
        }

        public IEnumerator LoadCoroutine(string profileID, string fileName, bool loadDataOnly)
		{
            SaveManager.loadDataOnly = loadDataOnly;

            if (gatheringSaveObjects) yield return new WaitWhile(() => gatheringSaveObjects);

			if (doDebugs) Debug.Log($"[SM] Loading articy flow state with profile ID: {profileID} and file name: {fileName}");

            // load any saved data from a file using the data handler
            saveData = saveHandler.Load<SaveData>(profileID, fileName);

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (saveData == null && autoInitializeDataIfNull)
            {
                NewGame();
            }

            // if no data can be loaded, don't continue
            if (saveData == null)
            {
				if (doDebugs) Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
				saveLoadCoroutine = null;
				yield break;
            }

            loading?.Invoke();

            //Set current profile & file to match the loaded file
            //currentProfileID = profileID; //Don't do this, it messes with dev saves, which should every be the current profile

            // push the loaded data to all other scripts that need it
            foreach (ISaveable dataPersistenceObj in saveableObjects)
            {
                dataPersistenceObj.LoadData(saveData);
            }

            SaveManager.loadDataOnly = false;
            loaded?.Invoke();
            saveLoadCoroutine = null;
        }

        //Delete save game
        [ContextMenu("Delete Current File")]
        public void DeleteCurrentFile() => DeleteFile(currentProfileID, currentFileName);

        public void DeleteFile(string profileId, string fileName)
        {
            saveHandler.DeleteFile(profileId, fileName);
            if (currentFileName == fileName && currentProfileID == profileId)
            {
                // initialize the selected profile id
                ChangeSelectedProfileToMostRecent();
                // reload the game so that our data matches the newly selected profile id
                LoadGame();
            }
        }
		#endregion

		private IEnumerator GetDataPersistenceObjects()
        {
            gatheringSaveObjects = true;

            //Wait for the end of the frame to make sure objects are settled
            //Example: Give newly loaded singletons a chance to delete themselves if there was alread an instance of their type
            yield return new WaitForEndOfFrame();

            // FindObjectsofType takes in an optional boolean to include inactive gameobjects
            IEnumerable<ISaveable> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            saveableObjects = new List<ISaveable>(dataPersistenceObjects);

			IEnumerable<IProfileSaveable> profilePersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IProfileSaveable>();
			profileSaveableObjects = new List<IProfileSaveable>(profilePersistenceObjects);

			gatheringSaveObjects = false;
        }

        public bool HasGameData()
        {
            return saveData != null;
        }

        public List<string> GetSaveNamesInProfile(string profileId)
		{
            return saveHandler.GetSaveNamesInProfile(profileId);
		}

		public string GetLastSaveDataAsString()
		{
			if (saveData != null)
			{
				return saveHandler.GetSaveString(saveData);
			}

			return "";
		}
	}
}
