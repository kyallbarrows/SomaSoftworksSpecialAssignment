using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Articy.Unity;
using UnityEngine;

namespace AltEnding
{
    /// <summary>
    /// Boilerplate class with function overrides for you to copy/paste and make your own Articy interface.
    /// Replace the Instance in ArticyStoryHelper with an instance of your derived class.
    /// </summary>
    public class BoilerplateStoryHelper : ArticyStoryHelper
    {
        public override string GetSpeakerExpressionDescription(IFlowObject flowObject)
        {
            return base.GetSpeakerExpressionDescription(flowObject);
        }
        
        public override List<string> GetAllSpeakerExpressionDescriptions()
        {
            return base.GetAllSpeakerExpressionDescriptions();
        }
        
        public override string GetBackgroundDescription(IFlowObject flowObject)
        {
            return base.GetBackgroundDescription(flowObject);
        }
        
        public override string GetChoiceTooltip(IFlowObject flowObject)
        {
            return base.GetChoiceTooltip(flowObject);
        }
        
        public override string GetImageFlavorText(IFlowObject flowObject)
        {
            return base.GetImageFlavorText(flowObject);
        }
        
        public override List<string> ParseArticyAnalyticsEvents(IFlowObject flowObject)
        {
            return base.ParseArticyAnalyticsEvents(flowObject);
        }
        
        public override string GetSpecialActionType(IFlowObject flowObject)
        {
            return base.GetSpecialActionType(flowObject);
        }
        
        public override string GetDisplayNameFromObject(ArticyObject articyObject)
        {
            return base.GetDisplayNameFromObject(articyObject);
        }
        
        public override string GetDisplayNameFromObject(IFlowObject flowObject)
        {
            return base.GetDisplayNameFromObject(flowObject);
        }
        
        public override ArticyObject GetImageFromObject(IFlowObject flowObject)
        {
            return base.GetImageFromObject(flowObject);
        }
        
        public override string GetLocationGalleryDescriptionFromObject(IFlowObject flowObject)
        {
            return base.GetLocationGalleryDescriptionFromObject(flowObject);
        }
        
        public override bool ShouldSkipFlow(IFlowObject flowObject)
        {
            return base.ShouldSkipFlow(flowObject);
        }
        
        public override CheckpointFeature GetCheckpointFeature(IFlowObject flowObject)
        {
            return base.GetCheckpointFeature(flowObject);
        }
        
        public override StoryFeature GetStoryFeature(IFlowObject flowObject)
        {
            return base.GetStoryFeature(flowObject);
        }
        
        public override GalleryFeature GetGalleryFeature(IFlowObject flowObject)
        {
            return base.GetGalleryFeature(flowObject);
        }
        
        public override ImageAsset GetImageAsset(IFlowObject flowObject)
        {
            return base.GetImageAsset(flowObject);
        }
        
        public override bool IsCheckpoint(IFlowObject flowObject)
        {
            return base.IsCheckpoint(flowObject);
        }
        
        public override bool HasCharacterFeature(ArticyObject articyObject)
        {
            return base.HasCharacterFeature(articyObject);
        }
        
        public override string ConvertSceneNameToArticyID(string sceneName)
        {
            string id = base.ConvertSceneNameToArticyID(sceneName);
            if (!string.IsNullOrEmpty(id))
                return id;
            
            // Fill this out with mappings of sceneName to Articy ID for your project
            switch (sceneName)
            {
                default:
                    return string.Empty;
            }
        }
        
        public override List<CharacterTemplate> GetMainCharacterTemplates()
        {
            return base.GetMainCharacterTemplates();
        }
        
        public override List<CharacterTemplate> GetSupportingCharacterTemplates()
        {
            return base.GetSupportingCharacterTemplates();
        }
        
        public override List<LocationTemplate> GetLocationTemplates()
        {
            return base.GetLocationTemplates();
        }
        
        public override List<ImageAsset> GetImageAssets()
        {
            return base.GetImageAssets();
        }
        
        protected override HashSet<string> GetGlobalVariableNames()
        {
            return base.GetGlobalVariableNames();
        }
    }
}