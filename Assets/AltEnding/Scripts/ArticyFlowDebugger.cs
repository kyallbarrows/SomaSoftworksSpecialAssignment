using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using TMPro;
using Articy.Unity;
#if UseNA
using NaughtyAttributes;
#endif
using System.IO;
using AltEnding.SaveSystem;

namespace AltEnding
{
    public class ArticyFlowDebugger : MonoBehaviour
    {
        [SerializeField] private ArticyFlowPlayer flowPlayer;

#if UseNA
        [ReadOnly]
#endif
        [SerializeField] bool showingDebugMenu;
        public Canvas baseCavas;
        [Header("Flow Jumping")]
        public TMP_InputField sceneIDInput;

        [Header("Variable Editing")]
        public Transform variablesParent;
        public GameObject boolVariablePrefab;
        public GameObject intVariablePrefab;
        public GameObject stringVariablePrefab;

        [Header("Dev Saves")]
        public Transform devSaveListParent;
        public GameObject devSaveListEntryPrefab;
        public TMP_InputField devSaveNameInput;
        public static string devSaveProfileName = "DevSaves";
        
        void Start()
        {
            HideDebugMenu();
            boolVariablePrefab.SetActive(false);
            intVariablePrefab.SetActive(false);
            stringVariablePrefab.SetActive(false);
        }
        
        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.f1Key.wasPressedThisFrame)
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(KeyCode.F1))
#endif
            {
                if (showingDebugMenu)
                {
                    HideDebugMenu();
                }
                else
                {
                    ShowDebugMenu();
                }
            }

        }

        void ShowDebugMenu()
		{
			showingDebugMenu = true;
			baseCavas.enabled = true;

			PopulateVariablesList();
            PopulateDevSavesList();

            SaveManager.loaded += SaveManager_loaded;
		}

		void HideDebugMenu()
        {
            showingDebugMenu = false;
            baseCavas.enabled = false;

            ClearVariablesList();
            ClearDevSavesList();

            SaveManager.loaded -= SaveManager_loaded;
        }

        #region Flow Jumping
        public void JumpToArticyObject()
        {
            if (flowPlayer != null && sceneIDInput != null)
            {
                string articyObjectHexID = ArticyFlowController.ValidateArticyObjectID(sceneIDInput.text);
                ulong articyObjectID = Articy.Unity.Utils.ArticyUtility.FromHex(articyObjectHexID);
                Debug.Log($"Trying to jump to articy object with hex ID '{articyObjectHexID}' converted to ID: '{articyObjectID}'");
                ArticyObject articyObject = ArticyDatabase.GetObject(articyObjectID);
                if (articyObject != null)
                {
                    flowPlayer.StartOn = articyObject;
                }
                else Debug.Log($"Could not get articy object with id '{articyObjectID}'");
            }
        }
        #endregion

        #region Variable Editing
        private void PopulateVariablesList()
        {
            ClearVariablesList();

            System.Text.StringBuilder message = new System.Text.StringBuilder("[AFD] Populating Variables List\n");

            Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
            List<string> keys = new List<string>(variables.Keys);
            GameObject prefabObject = null;
            TMP_InputField prefabInputField = null;
            Toggle prefabToggle = null;
            TMP_Text prefabLabel = null;
            foreach (KeyValuePair<string, object> kvp in variables)
            {
                if (flowPlayer.GlobalVariables.IsVariableOfTypeBoolean(kvp.Key))
                {
                    prefabObject = Instantiate(boolVariablePrefab, variablesParent);
                    prefabToggle = prefabObject.GetComponentInChildren<Toggle>();
                    if (prefabToggle != null)
                    {
                        prefabToggle.isOn = (bool)kvp.Value;
                        prefabToggle.onValueChanged.AddListener((data) => { SetBoolVariable(kvp.Key, data); });
                    }
                    message.Append($"Making option for setting bool '{kvp.Key}', got toggle: {prefabToggle != null}");
                }
                else if (flowPlayer.GlobalVariables.IsVariableOfTypeInteger(kvp.Key))
                {
                    prefabObject = Instantiate(intVariablePrefab, variablesParent);
                    prefabInputField = prefabObject.GetComponentInChildren<TMP_InputField>();
                    if (prefabInputField != null)
                    {
                        prefabInputField.text = ((int)kvp.Value).ToString();
                        prefabInputField.onSubmit.AddListener((data) => { SetIntVariable(kvp.Key, data); });
                    }
                    message.Append($"Making option for setting int '{kvp.Key}', got input: {prefabToggle != null}");
                }
                else if (flowPlayer.GlobalVariables.IsVariableOfTypeString(kvp.Key))
                {
                    prefabObject = Instantiate(stringVariablePrefab, variablesParent);
                    prefabInputField = prefabObject.GetComponentInChildren<TMP_InputField>();
                    if (prefabInputField != null)
                    {
                        prefabInputField.text = kvp.Value.ToString();
                        prefabInputField.onSubmit.AddListener((data) => { SetStringVariable(kvp.Key, data); });
                    }
                    message.Append($"Making option for setting string '{kvp.Key}', got input: {prefabToggle != null}");
                }

                if (prefabObject != null)
                {
                    prefabObject.SetActive(true);
                    prefabLabel = prefabObject.GetComponentInChildren<TMP_Text>();
                    string varName = kvp.Key;
                    string[] varNameParts = kvp.Key.Split(".");
                    if (varNameParts.Length > 1)
					{
                        varName = varNameParts[0].Replace("DialogueEpisode", "Episode ") + ":\n" + varNameParts[1];
					}
                    if (prefabLabel != null) prefabLabel.SetText(varName);
                    message.AppendLine($", got label: {prefabLabel != null}");
                }
            }

            Debug.Log(message);
        }

        private void ClearVariablesList()
        {
            for (int i = variablesParent.childCount - 1; i >= 0; i--)
            {
                Destroy(variablesParent.GetChild(i).gameObject);
            }
        }

        void SetBoolVariable(string key, bool value)
        {
            Debug.Log($"Set variable '{key}' to '{value}'");
            if (flowPlayer != null && flowPlayer.GlobalVariables.Variables.ContainsKey(key))
            {
                Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
                variables[key] = value;
                flowPlayer.GlobalVariables.Variables = variables;
            }
        }

        void SetIntVariable(string key, string value)
        {
            Debug.Log($"Set variable '{key}' to '{value}'");
            int parsedInt;
            if (flowPlayer != null && flowPlayer.GlobalVariables.Variables.ContainsKey(key) && int.TryParse(value, out parsedInt))
            {
                Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
                variables[key] = parsedInt;
                flowPlayer.GlobalVariables.Variables = variables;
            }
        }

        void SetStringVariable(string key, string value)
        {
            Debug.Log($"Set variable '{key}' to '{value}'");
            if (flowPlayer != null && flowPlayer.GlobalVariables.Variables.ContainsKey(key))
            {
                Dictionary<string, object> variables = flowPlayer.GlobalVariables.Variables;
                variables[key] = value;
                flowPlayer.GlobalVariables.Variables = variables;
            }
        }
		#endregion

		#region Dev Saves
        private void PopulateDevSavesList()
		{
            ClearDevSavesList();
            List<string> saveList = SaveManager.Instance.GetSaveNamesInProfile(devSaveProfileName);
            System.Text.StringBuilder message = new System.Text.StringBuilder($"Dev Saves ({saveList.Count}):\n");
            foreach (string saveName in saveList)
            {
                message.AppendLine($"\t{saveName}");

                GameObject prefabObject = Instantiate(devSaveListEntryPrefab, devSaveListParent);

                TMP_Text prefabLabel = prefabObject.GetComponentInChildren<TMP_Text>();
                if (prefabLabel != null) prefabLabel.SetText(saveName);

                Button prefabButton = prefabObject.GetComponentInChildren<Button>();
                if (prefabButton != null)
                {
                    prefabButton.onClick.AddListener(() => { LoadDevSave(saveName); });
                }

                prefabObject.SetActive(true);
            }
            Debug.Log(message);
		}

        private void ClearDevSavesList()
        {
            for (int i = devSaveListParent.childCount - 1; i >= 0; i--)
            {
                Destroy(devSaveListParent.GetChild(i).gameObject);
            }
        }

        public void MakeDevSave()
		{
            if (devSaveNameInput != null && !string.IsNullOrWhiteSpace(devSaveNameInput.text))
			{
                SaveManager.Instance.SaveGame(devSaveProfileName, devSaveNameInput.text);
                PopulateDevSavesList();
			}
            else Debug.LogError("[AFD] Could not save dev save: save name is null or white space");
        }

        public void LoadDevSave(string saveName)
		{
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                //Try to load the save
                SaveManager.Instance.LoadGame(devSaveProfileName, saveName, false);
            }
            else Debug.LogError("[AFD] Could not load dev save: save name is null or white space");
        }

        private void SaveManager_loaded()
        {
            PopulateVariablesList();
        }

        public void OpenDevSavesFolder()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, devSaveProfileName);
            Directory.CreateDirectory(fullPath);
            Application.OpenURL(fullPath);
        }
        #endregion
    }
}
