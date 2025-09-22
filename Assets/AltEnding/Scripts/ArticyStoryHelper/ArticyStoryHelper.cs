using System.Collections.Generic;
using System.Linq;
using Articy.Unity;
using Random = UnityEngine.Random;

namespace AltEnding
{
    /// <summary>
    /// Helper class for accessing various parts of an Articy story from its generated code
    /// </summary>
    public class ArticyStoryHelper
    {
        protected ArticyStoryHelper() { }
        
        /// <summary>
        /// Assign a subclass to customize for your project using your specific Articy generated code
        /// </summary>
        public static ArticyStoryHelper Instance = new SpecialAssignmentStoryHelper();
        
        /// <summary>
        /// Get a string representation of a speaker's facial expression defined for the given flow object
        /// </summary>
        public virtual string GetSpeakerExpressionDescription(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get all of the string representations of speaker facial expressions defined in Articy
        /// </summary>
        public virtual List<string> GetAllSpeakerExpressionDescriptions()
        {
            return new();
        }

        /// <summary>
        /// Get a string representation of a background defined for the given flow object
        /// </summary>
        public virtual string GetBackgroundDescription(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get a button tooltip associated with the given flow object if defined
        /// </summary>
        public virtual string GetChoiceTooltip(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get flavor text associated with an image in Articy
        /// </summary>
        public virtual string GetImageFlavorText(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get a list of strings representing analytic events defined in Articy for the given flow object
        /// </summary>
        public virtual List<string> ParseArticyAnalyticsEvents(IFlowObject flowObject)
        {
            return new();
        }

        /// <summary>
        /// Get a string representing any kind of special action defined in Articy
        /// </summary>
        public virtual string GetSpecialActionType(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get a display name for the given Articy object
        /// </summary>
        public virtual string GetDisplayNameFromObject(ArticyObject articyObject)
        {
            return string.Empty;
        }
        
        /// <summary>
        /// Get a display name for the given Articy flow
        /// </summary>
        public virtual string GetDisplayNameFromObject(IFlowObject flowObject)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get an articy object representing an image from a flow object
        /// </summary>
        public virtual ArticyObject GetImageFromObject(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Get the gallery description from a location feature
        /// </summary>
        public virtual string GetLocationGalleryDescriptionFromObject(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Checks for any special cases that a flow object should be skipped
        /// </summary>
        public virtual bool ShouldSkipFlow(IFlowObject flowObject)
        {
            return false;
        }

        /// <summary>
        /// Get checkpoint feature data from the given flow object
        /// </summary>
        public virtual CheckpointFeature GetCheckpointFeature(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Get story feature properties from the given flow object
        /// </summary>
        public virtual StoryFeature GetStoryFeature(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Get Gallery feature data from the given flow object
        /// </summary>
        public virtual GalleryFeature GetGalleryFeature(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Get image asset data from the given flow object
        /// </summary>
        public virtual ImageAsset GetImageAsset(IFlowObject flowObject)
        {
            return null;
        }

        /// <summary>
        /// Check if the given flow object is a checkpoint
        /// </summary>
        public virtual bool IsCheckpoint(IFlowObject flowObject)
        {
            return false;
        }

        /// <summary>
        /// Check if this articy object contains character feature data
        /// </summary>
        public virtual bool HasCharacterFeature(ArticyObject articyObject)
        {
            return false;
        }
        
        /// <summary>
        /// We have a problem of locations being loaded through data in the dialog that is separate from the location entities used to check if they are unlocked.
        /// This is an unideal solution, but we need a way to convert from the scene name that is loaded to the appropriate Articy location object.
        /// </summary>
        public virtual string ConvertSceneNameToArticyID(string sceneName)
        {
            switch (sceneName)
            {
                default:
                    if (sceneName.StartsWith("0x"))
                        return sceneName; //This seems to be an Articy ID, just use it as is
                    return string.Empty;
            }
        }

        /// <summary>
        /// Loads main character templates from the Articy database and returns a list with their data
        /// </summary>
        public virtual List<CharacterTemplate> GetMainCharacterTemplates()
        {
            return new();
        }

        /// <summary>
        /// Loads supporting character templates from the Articy database and returns a list with their data
        /// </summary>
        public virtual List<CharacterTemplate> GetSupportingCharacterTemplates()
        {
            return new();
        }

        /// <summary>
        /// Loads location templates from the Articy database and returns a list with their data
        /// </summary>
        public virtual List<LocationTemplate> GetLocationTemplates()
        {
            return new();
        }

        /// <summary>
        /// Loads image assets from the Articy database and returns a list with their data
        /// </summary>
        public virtual List<ImageAsset> GetImageAssets()
        {
            return new();
        }

        /// <summary>
        /// Get the global variable names from Articy's generated code
        /// </summary>
        protected virtual HashSet<string> GetGlobalVariableNames()
        {
            return new();
        }
        
        /// <summary>
        /// Get a list of all global variable names defined in Articy
        /// </summary>
        public List<string> GenerateArticyVariableNamesLists(List<string> variablesList)
        {
            List<string> newVariablesList = new();
            HashSet<string> globalVariables = GetGlobalVariableNames();
            
            if (ArticyDatabase.IsDatabaseAvailable() && ArticyDatabase.DefaultGlobalVariables.IsInitialized && globalVariables.Count > 0)
            {
                int randomTest = Random.Range(0, globalVariables.Count - 1);
                if (variablesList == null 
                    || variablesList.Count != globalVariables.Count 
                    || variablesList[randomTest] != globalVariables.ElementAt(randomTest))
                {
                    newVariablesList = globalVariables.ToList();
                    GenerateArticyBooleanNamesList(newVariablesList);
                }
            }
            else
            {
                newVariablesList = new List<string>() { "NULL" };
            }
            
            return newVariablesList;
        }

        /// <summary>
        /// Get a list of specifically global boolean variables defined in Articy
        /// </summary>
        public List<string> GenerateArticyBooleanNamesList(List<string> variablesList, List<string> booleansList = null)
        {
            List<string> newBooleansList = new();
            if (booleansList == null)
                newBooleansList = new List<string>() { "No Boolean Variables"};
            
            if (variablesList == null 
                || variablesList.Count <= 0 
                || !ArticyDatabase.IsDatabaseAvailable() 
                || !ArticyDatabase.DefaultGlobalVariables.IsInitialized) 
                return new();

            foreach(string vName in variablesList)
            {
                if (ArticyDatabase.DefaultGlobalVariables.IsVariableOfTypeBoolean(vName))
                {
                    newBooleansList.Add(vName);
                }
            }
            while (newBooleansList.Count > 1 && newBooleansList.Contains("No Boolean Variables"))
            {
                newBooleansList.Remove("No Boolean Variables");
            }
            
            return newBooleansList;
        }
    }
}