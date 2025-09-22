using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AltEnding.SaveSystem
{
	public class JSONFileSaveHandler
	{
		private readonly string dataDirPath = "";
		private readonly bool useEncryption = false;
		private const string encryptionCodeWord = "SomaSecure28?encryption";
		private const string backupExtension = ".bak";
		private const string defaultProfileName = "default_profile";

		public JSONFileSaveHandler(string dataDirPath, bool useEncryption)
		{
			this.dataDirPath = dataDirPath;
			this.useEncryption = useEncryption;
		}

		public T Load<T>(string profileId, string fileName, bool allowRestoreFromBackup = true) where T : class
		{
			// base case - if the profileId is null, return right away
			if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(fileName))
			{
				Debug.LogError($"Error occured when trying to load file. Invalid data provided.\nProfile ID: {(profileId != null ? profileId : "null")}\nFile Name: {(fileName != null ? fileName : "null")}");
				return null;
			}

			if (fileName == "steam_autocloud.vdf")
			{
				Debug.Log($"Ignoring attempt to load steam autocloud file.");
				return null;
			}

			//Remove illegal characters from profile ids and save file names
			profileId = string.Join("", profileId.Split(Path.GetInvalidFileNameChars()));
			fileName = string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));

			// use Path.Combine to account for different OS's having different path separators
			string fullPath = Path.Combine(dataDirPath, profileId, fileName);
			T loadedData = null;
			if (File.Exists(fullPath))
			{
				try
				{
					// load the serialized data from the file
					string dataToLoad = "";
					using (FileStream stream = new FileStream(fullPath, FileMode.Open))
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							dataToLoad = reader.ReadToEnd();
						}
					}

					// optionally decrypt the data
					if (useEncryption)
					{
						dataToLoad = EncryptDecrypt(dataToLoad);
					}

					// deserialize the data from Json back into the C# object
					loadedData = JsonUtility.FromJson<T>(dataToLoad);
				}
				catch (Exception e)
				{
					// since we're calling Load(..) recursively, we need to account for the case where
					// the rollback succeeds, but data is still failing to load for some other reason,
					// which without this check may cause an infinite recursion loop.
					if (allowRestoreFromBackup)
					{
						Debug.LogWarning("Failed to load data file. Attempting to roll back.\n" + e);
						bool rollbackSuccess = AttemptRollback(fullPath);
						if (rollbackSuccess)
						{
							// try to load again recursively
							loadedData = Load<T>(profileId, fileName, false);
						}
					}
					// if we hit this else block, one possibility is that the backup file is also corrupt
					else
					{
						Debug.LogError("Error occured when trying to load file at path: "
							+ fullPath + " and backup did not work.\n" + e);
					}
				}
			}
			else
			{
				Debug.LogError($"Error occured when trying to load file at path: {fullPath}.\nThe directory does not exist.");
			}
			return loadedData;
		}

		//It's not necessary to enforce typing for saving (could have the data have a type of 'object'), but I'm adding it to mirror loading, where a type is required
		public void Save<T>(T data, string profileId, string fileName) where T : class
		{
			// base case - if the profileId is null, set it to a default value
			if (profileId == null)
			{
				profileId = defaultProfileName;
			}

			//Remove illegal characters from profile ids and save file names
			profileId = string.Join("", profileId.Split(Path.GetInvalidFileNameChars()));
			fileName = string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));

			// use Path.Combine to account for different OS's having different path separators
			string fullPath = Path.Combine(dataDirPath, profileId, fileName);
			string backupFilePath = fullPath + backupExtension;
			try
			{
				// create the directory the file will be written to if it doesn't already exist
				Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

				// serialize the C# game data object into Json
				string dataToStore = JsonUtility.ToJson(data, true);

				// optionally encrypt the data
				if (useEncryption)
				{
					dataToStore = EncryptDecrypt(dataToStore);
				}

				// write the serialized data to the file
				using (FileStream stream = new FileStream(fullPath, FileMode.Create))
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.Write(dataToStore);
					}
				}

				// verify the newly saved file can be loaded successfully
				SaveData verifiedGameData = Load<SaveData>(profileId, fileName);
				// if the data can be verified, back it up
				if (verifiedGameData != null)
				{
					File.Copy(fullPath, backupFilePath, true);
				}
				// otherwise, something went wrong and we should throw an exception
				else
				{
					throw new Exception("Save file could not be verified and backup could not be created.");
				}

			}
			catch (Exception e)
			{
				Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
			}
		}

		public string GetSaveString(SaveData data)
		{
			return JsonUtility.ToJson(data, true);
		}

		public void DeleteProfile(string profileId)
		{
			// base case - if the profileId is null, return right away
			if (string.IsNullOrWhiteSpace(profileId))
			{
				return;
			}

			string fullPath = Path.Combine(dataDirPath, profileId);
			try
			{
				// ensure the directory exists at this path before deleting the directory
				if (Directory.Exists(fullPath))
				{
					// delete the profile folder and everything within it
					Directory.Delete(fullPath, true);
				}
				else
				{
					Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to delete profile data for profileId: "
					+ profileId + " at path: " + fullPath + "\n" + e);
			}
		}

		public void DeleteFile(string profileId, string fileName)
		{
			// base case - if the profileId is null, return right away
			if (string.IsNullOrWhiteSpace(profileId) || string.IsNullOrWhiteSpace(fileName))
			{
				return;
			}

			string fullPath = Path.Combine(dataDirPath, profileId, fileName);
			try
			{
				// ensure the data file exists at this path before deleting the directory
				if (File.Exists(fullPath))
				{
					// delete the profile folder and everything within it
					File.Delete(fullPath);
				}
				else
				{
					Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to delete profile data for profileId: "
					+ profileId + " at path: " + fullPath + "\n" + e);
			}
		}

		// the below is a simple implementation of XOR encryption
		private string EncryptDecrypt(string data)
		{
			string modifiedData = "";
			for (int i = 0; i < data.Length; i++)
			{
				modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
			}
			return modifiedData;
		}

		private bool AttemptRollback(string fullPath)
		{
			bool success = false;
			string backupFilePath = fullPath + backupExtension;
			try
			{
				// if the file exists, attempt to roll back to it by overwriting the original file
				if (File.Exists(backupFilePath))
				{
					File.Copy(backupFilePath, fullPath, true);
					success = true;
					Debug.LogWarning("Had to roll back to backup file at: " + backupFilePath);
				}
				// otherwise, we don't yet have a backup file - so there's nothing to roll back to
				else
				{
					throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error occured when trying to roll back to backup file at: "
					+ backupFilePath + "\n" + e);
			}

			return success;
		}

		public string GetMostRecentlyUpdatedProfileID()
		{
			string mostRecentProfileId = null;
			DateTime mostRecentDateTime = DateTime.MinValue;

			// loop over all directory names in the data directory path
			IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
			foreach (DirectoryInfo dirInfo in dirInfos)
			{
				if (dirInfo.Name == AltEnding.ArticyFlowDebugger.devSaveProfileName) continue;  //Don't count the dev save folder

				string profileId = dirInfo.Name;
				string profilePath = Path.Combine(dataDirPath, profileId);
				IEnumerable<FileInfo> fileInfos = new DirectoryInfo(profilePath).EnumerateFiles();
				foreach (FileInfo fileInfo in fileInfos)
				{
					string fileName = fileInfo.Name;

					// defensive programming - check if the data file exists
					// if it doesn't, then this folder isn't a profile and should be skipped
					string filePath = Path.Combine(dataDirPath, profileId, fileName);
					if (!File.Exists(filePath))
					{
						Debug.LogWarning($"Skipping directory when loading all profiles because it does not contain data: {profileId}");
						continue;
					}

					// load the game data for this profile and put it in the dictionary
					SaveData saveData = Load<SaveData>(profileId, fileName);
					// defensive programming - ensure the profile data isn't null,
					// because if it is then something went wrong and we should let ourselves know
					if (saveData != null)
					{
						if (DateTime.FromBinary(saveData.lastUpdated) > mostRecentDateTime)
						{
							mostRecentProfileId = profileId;
							mostRecentDateTime = DateTime.FromBinary(saveData.lastUpdated);
						}
					}
					else
					{
						Debug.LogError($"Tried to load save data but something went wrong.\nProfileId: {profileId}\nFile Name: {fileName}");
					}
				}
			}

			return mostRecentProfileId;
		}

		/*
		public string GetMostRecentFileNameForProfile(string profileId)
		{
			string mostRecentFileName = null;
			DateTime mostRecentDateTime = DateTime.MinValue;
			string profilePath = Path.Combine(dataDirPath, profileId);

			IEnumerable<FileInfo> fileInfos = new DirectoryInfo(profilePath).EnumerateFiles();
			foreach (FileInfo fileInfo in fileInfos)
			{
				string fileName = fileInfo.Name;

				// defensive programming - check if the data file exists
				// if it doesn't, then this folder isn't a profile and should be skipped
				string filePath = Path.Combine(dataDirPath, profileId, fileName);
				if (!File.Exists(filePath))
				{
					Debug.LogWarning($"Skipping file when loading all files for profile {profileId} because it does not exist: {fileName}");
					continue;
				}

				// load the game data for this profile and put it in the dictionary
				SaveData saveData = Load(profileId, fileName);
				// defensive programming - ensure the profile data isn't null,
				// because if it is then something went wrong and we should let ourselves know
				if (saveData != null)
				{
					if (DateTime.FromBinary(saveData.lastUpdated) > mostRecentDateTime)
					{
						mostRecentFileName = fileName;
						mostRecentDateTime = DateTime.FromBinary(saveData.lastUpdated);
					}
				}
				else
				{
					Debug.LogError($"Tried to load save data but something went wrong.\nProfileId: {profileId}\nFile Name: {fileName}");
				}
			}

			return mostRecentFileName;
		}
		*/

		public List<string> GetSaveNamesInProfile(string profileId)
		{
			List<string> returnList = new List<string>();
			string profilePath = Path.Combine(dataDirPath, profileId);
			if (!Directory.Exists(profilePath)) return returnList; //If the profile doesn't exist, return an empty list
			IEnumerable<FileInfo> fileInfos = new DirectoryInfo(profilePath).EnumerateFiles();
			foreach (FileInfo fileInfo in fileInfos)
			{
				if (!fileInfo.Name.EndsWith(".bak")) //Ignore the backup files
					returnList.Add(fileInfo.Name);
			}

			return returnList;
		}
	}
}
