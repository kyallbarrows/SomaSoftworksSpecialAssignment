using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AltEnding.Dialog;

namespace AltEnding
{
    public class ArticyDataImporter
    {
#if UNITY_EDITOR        
        [MenuItem("AltEnding Tools/Articy Data Import/Import All Data From Articy", priority = 10)]
        static public void ImportAllDataFromArticy()
        {
            ImportCharactersFromArticy();
            ImportLocationsFromArticy();
            ImportImageAssetsFromArticy();
        }

        [MenuItem("AltEnding Tools/Articy Data Import/Import Characters From Articy", priority = 30, secondaryPriority = 1)]
        static public void ImportCharactersFromArticy()
        {
            System.Text.StringBuilder message = new System.Text.StringBuilder("Getting character data from articy database.\n");
            const string dppProjectPath = "Assets/_Art/Characters";

            //Get Main Characters from Articy Database
            List<CharacterTemplate> mainCharacterTemplates = ArticyStoryHelper.Instance.GetMainCharacterTemplates();
            if (mainCharacterTemplates != null)
            {
                message.AppendLine($"\nArticy Main Character Template List (Count: {mainCharacterTemplates.Count})");
                foreach (CharacterTemplate mainCharacterTemplate in mainCharacterTemplates)
                {
                    message.AppendLine($"\t{mainCharacterTemplate.displayName}");
                }
            }

            //Get Supporting Characters from Articy Database
            List<CharacterTemplate> supportingCharacterTemplates = ArticyStoryHelper.Instance.GetSupportingCharacterTemplates();
            if (supportingCharacterTemplates != null)
            {
                message.AppendLine($"\nArticy Supporting Character Template List (Count: {supportingCharacterTemplates.Count})");
                foreach (CharacterTemplate supportingCharacterTemplate in supportingCharacterTemplates)
                {
                    message.AppendLine($"\t{supportingCharacterTemplate.displayName}");
                }
            }

            //Get Dialog Portrait Packages from Unity
            List<DialogPortraitPackage> dpps = new List<DialogPortraitPackage>();
            string[] dppGUIDs = AssetDatabase.FindAssets($"t:{typeof(DialogPortraitPackage)}", new[] { dppProjectPath });
            string dppAssetPath;
            DialogPortraitPackage dppAsset;

            foreach (string dppGUID in dppGUIDs)
            {
                dppAssetPath = AssetDatabase.GUIDToAssetPath(dppGUID);
                dppAsset = AssetDatabase.LoadAssetAtPath<DialogPortraitPackage>(dppAssetPath);
                if (dppAsset != null) dpps.Add(dppAsset);
            }

            message.AppendLine($"\nUnity Dialog Portrait Packages List (Count: {dpps.Count})");
            foreach (DialogPortraitPackage dpp in dpps)
            {
                message.AppendLine($"\t{dpp.name}");
            }

            message.AppendLine($"\nUpdating Unity DPPs for main characters");
            //Create/Update Dialog Portrait Packages for main Characters
            foreach (CharacterTemplate mainCharTemplate in mainCharacterTemplates)
            {
                dppAsset = null;

                //Look for already existing scriptable object
                foreach (DialogPortraitPackage dpp in dpps)
                {
                    if (mainCharTemplate.id == dpp.articyHexID)
                    {
                        message.AppendLine($"\tMain character {mainCharTemplate.displayName} has a corresponding DPP in Unity.  Updating it's info.");
                        dppAsset = dpp; //Store reference so code for updating info can be put after the create new object code
                        break;
                    }
                }

                //Create Dialog Portrait Package scriptable object if it doesn't exist in Unity
                if (dppAsset == null)
                {
                    message.AppendLine($"\tMain character {mainCharTemplate.displayName} does not have a corresponding DPP in Unity.  Creating one.");
                    dppAsset = ScriptableObject.CreateInstance<DialogPortraitPackage>();
                    System.IO.Directory.CreateDirectory($"{Application.dataPath}/_Art/Characters/{mainCharTemplate.displayName}"); //Create character folder if it doesn't exist
                    AssetDatabase.CreateAsset(dppAsset, $"{dppProjectPath}/{mainCharTemplate.displayName}/{mainCharTemplate.displayName}.asset");
                }

                //Update info in the stored Dialog Portrait Package
                if (dppAsset != null)
                {
                    dppAsset.UpdateArticyRef(mainCharTemplate.obj);
                    dppAsset.GetAssetsFromArticyRef();
                    EditorUtility.SetDirty(dppAsset);
                }
                else
                    message.AppendLine($"<color=red>Could not find or create Dialog Portrait Package for '{mainCharTemplate.displayName}'</color>");
            }

            message.AppendLine($"\nUpdating Unity DPPs for supporting characters");
            //Create/Update Dialog Portrait Packages for supporting Characters
            foreach (CharacterTemplate supportingCharTemplate in supportingCharacterTemplates)
            {
                dppAsset = null;

                //Look for already existing clue scriptable object
                foreach (DialogPortraitPackage dpp in dpps)
                {
                    if (supportingCharTemplate.id == dpp.articyHexID)
                    {
                        message.AppendLine($"\tSupporting character {supportingCharTemplate.displayName} has a corresponding DPP in Unity.  Updating it's info.");
                        dppAsset = dpp; //Store reference so code for updating info can be put after the code for creating new objects
                        break;
                    }
                }

                //Create clue scriptable object if it doesn't exist in Unity
                if (dppAsset == null)
                {
                    message.AppendLine($"\tSupporting character {supportingCharTemplate.displayName} does not have a corresponding DPP in Unity.  Creating one.");
                    dppAsset = ScriptableObject.CreateInstance<DialogPortraitPackage>();
                    System.IO.Directory.CreateDirectory($"{Application.dataPath}/_Art/Characters/{supportingCharTemplate.displayName}"); //Create character folder if it doesn't exist
                    AssetDatabase.CreateAsset(dppAsset, $"{dppProjectPath}/{supportingCharTemplate.displayName}/{supportingCharTemplate.displayName}.asset");
                }

                //Update info in the stored clue
                if (dppAsset != null)
                {
                    dppAsset.UpdateArticyRef(supportingCharTemplate.obj);
                    dppAsset.GetAssetsFromArticyRef();
                    EditorUtility.SetDirty(dppAsset);
                }
                else
                    message.AppendLine($"<color=red>Could not find or create Dialog Portrait Package for '{supportingCharTemplate.displayName}'</color>");
            }

            AssetDatabase.SaveAssets();
            Debug.Log(message);
        }

        [MenuItem("AltEnding Tools/Articy Data Import/Import Locations From Articy", priority = 30, secondaryPriority = 2)]
        static public void ImportLocationsFromArticy()
        {
            System.Text.StringBuilder message = new System.Text.StringBuilder("Getting location data from articy database.\n");
            const string locationSOProjectPath = "Assets/_Data/Locations";
            
            if (!System.IO.Directory.Exists(locationSOProjectPath))
                System.IO.Directory.CreateDirectory(locationSOProjectPath);

            //Get Main Characters from Articy Database
            List<LocationTemplate> locationTemplates = ArticyStoryHelper.Instance.GetLocationTemplates();
            if (locationTemplates != null)
            {
                message.AppendLine($"\nArticy Location Template List (Count: {locationTemplates.Count})");
                foreach (LocationTemplate LocationTemplate in locationTemplates)
                    message.AppendLine($"\t{LocationTemplate.displayName}");

                //Get Locations from Unity
                List<Location> locationSOs = new List<Location>();
                string[] locationSOGUIDs = AssetDatabase.FindAssets($"t:{typeof(Location)}", new[] { locationSOProjectPath });
                string locationSOAssetPath;
                Location locationSOAsset;

                foreach (string locationSOGUID in locationSOGUIDs)
                {
                    locationSOAssetPath = AssetDatabase.GUIDToAssetPath(locationSOGUID);
                    locationSOAsset = AssetDatabase.LoadAssetAtPath<Location>(locationSOAssetPath);
                    if (locationSOAsset != null) locationSOs.Add(locationSOAsset);
                }

                message.AppendLine($"\nUnity Location SOs List (Count: {locationSOs.Count})");
                foreach (Location locationSO in locationSOs)
                {
                    message.AppendLine($"\t{locationSO.name}");
                }

                message.AppendLine($"\nUpdating Unity Location  SOs");
                //Update or add locations into Unity
                foreach (LocationTemplate locationTemplate in locationTemplates)
                {
                    locationSOAsset = null;

                    //Look for already existing location scriptable object
                    foreach (Location location in locationSOs)
                    {
                        if (locationTemplate.id == location.articyHexID)
                        {
                            message.AppendLine($"\tLocation {locationTemplate.displayName} has a corresponding SO in Unity.  Updating it's info.");
                            locationSOAsset = location; //Store reference so code for updating info can be put after the create new object code
                            break;
                        }
                    }

                    //Create location scriptable object if it doesn't exist in Unity
                    if (locationSOAsset == null)
                    {
                        message.AppendLine($"\tLocation {locationTemplate.displayName} does not have a corresponding SO in Unity.  Creating one.");
                        locationSOAsset = ScriptableObject.CreateInstance<Location>();
                        AssetDatabase.CreateAsset(locationSOAsset, $"{locationSOProjectPath}/{locationTemplate.displayName}.asset");
                    }

                    //Update info in the stored location
                    if (locationSOAsset != null)
                    {
                        locationSOAsset.ImportArticyData(locationTemplate);
                    }
                    else
                        message.AppendLine($"<color=red>Could not find or create location scriptable object for '{locationTemplate.displayName}'</color>");
                }

                AssetDatabase.SaveAssets();
            }
            else message.Append("Articy template list is null");

            Debug.Log(message);
        }

        [MenuItem("AltEnding Tools/Articy Data Import/Import Image Assets From Articy", priority = 30, secondaryPriority = 3)]
        static public void ImportImageAssetsFromArticy()
        {
            System.Text.StringBuilder message = new System.Text.StringBuilder("Getting Image Asset data from articy database.\n");
            const string imageAssetSOProjectPath = "Assets/_Data/Image Assets";
            
            if (!System.IO.Directory.Exists(imageAssetSOProjectPath))
                System.IO.Directory.CreateDirectory(imageAssetSOProjectPath);

            //Get Image Assets from Articy Database
            List<ImageAsset> imageAssetTemplates = ArticyStoryHelper.Instance.GetImageAssets();
            if (imageAssetTemplates != null)
            {
                message.AppendLine($"\nArticy Location Template List (Count: {imageAssetTemplates.Count})");
                foreach (ImageAsset iaTemplate in imageAssetTemplates)
                {
                    message.AppendLine($"\t{iaTemplate.displayName}");
                }

                //Get Image Assets from Unity
                List<DialogImageAsset> diaSOs = new List<DialogImageAsset>();
                string[] diaSOGUIDs = AssetDatabase.FindAssets($"t:{typeof(DialogImageAsset)}", new[] { imageAssetSOProjectPath });
                string diaSOAssetPath;
                DialogImageAsset diaSOAsset;

                foreach (string diaSOGUID in diaSOGUIDs)
                {
                    diaSOAssetPath = AssetDatabase.GUIDToAssetPath(diaSOGUID);
                    diaSOAsset = AssetDatabase.LoadAssetAtPath<DialogImageAsset>(diaSOAssetPath);
                    if (diaSOAsset != null) diaSOs.Add(diaSOAsset);
                }

                message.AppendLine($"\nUnity DialogImageAsset SOs List (Count: {diaSOs.Count})");
                foreach (DialogImageAsset diaSO in diaSOs)
                    message.AppendLine($"\t{diaSO.name}");

                message.AppendLine($"\nUpdating Unity DialogImageAsset SOs");
                
                //Update or add DIA's into Unity
                foreach (ImageAsset imageAssetTemplate in imageAssetTemplates)
                {
                    diaSOAsset = null;

                    //Look for already existing Image Asset scriptable object
                    foreach (DialogImageAsset dia in diaSOs)
                    {
                        if (imageAssetTemplate.id == dia.articyHexID)
                        {
                            message.AppendLine($"\tImage Asset {imageAssetTemplate.displayName} has a corresponding SO in Unity.  Updating it's info.");
                            diaSOAsset = dia; //Store reference so code for updating info can be put after the code for creating new objects
                            break;
                        }
                    }

                    //Create Image Asset scriptable object if it doesn't exist in Unity
                    if (diaSOAsset == null)
                    {
                        message.AppendLine($"\tImage Asset {imageAssetTemplate.displayName} does not have a corresponding SO in Unity.  Creating one.");
                        diaSOAsset = ScriptableObject.CreateInstance<DialogImageAsset>();
                        AssetDatabase.CreateAsset(diaSOAsset, $"{imageAssetSOProjectPath}/{imageAssetTemplate.displayName}.asset");
                    }

                    //Update info in the stored Image Assets
                    if (diaSOAsset != null)
                    {
                        diaSOAsset.ImportArticyData(imageAssetTemplate);
                    }
                    else
                        message.AppendLine($"<color=red>Could not find or create DialogImageAsset scriptable object for '{imageAssetTemplate.displayName}'</color>");
                }

                AssetDatabase.SaveAssets();
            }
            else message.Append("Articy template list is null");

            Debug.Log(message);
        }
#endif
    }
}