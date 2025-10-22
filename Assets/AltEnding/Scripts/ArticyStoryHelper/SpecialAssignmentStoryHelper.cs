using System.Collections.Generic;
using Articy.Special_Assignment;
using Articy.Special_Assignment.Features;
using Articy.Special_Assignment.GlobalVariables;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Unity.Utils;

namespace AltEnding
{
    public class SpecialAssignmentStoryHelper : ArticyStoryHelper
    {
        public override string GetSpeakerExpressionDescription(IFlowObject flowObject)
        {
            string expression = string.Empty;

            // TODO: If we're using expressions, find out how Nolan is storing them and plug in here.
            // Previous projects have used a template for dialogue fragments to add extra data like this.
            
            return expression;
        }
        
        public override List<string> GetAllSpeakerExpressionDescriptions()
        {
            List<string> expressions = new();
            
            // TODO: If we're using expressions, we should be able to get a list of all defined in Articy here.
            // In past projects, this ended up being an enum in the generated code from the import.
            // Then you can do something like:
            // Dialog_Expression[] expressionValues = (Dialog_Expression[])Enum.GetValues(typeof(Dialog_Expression));
            // foreach (Dialog_Expression expression in expressionValues)
            //     expressions.Add(expression.ToString());

            return expressions;
        }
        
        public override string GetBackgroundDescription(IFlowObject flowObject)
        {
            string background = "Dont_Change";
            
            // TODO: This gets used for loading location scenes based on this background name, so we'll need
            // to make sure this data is available through Articy as a dialogue template or something.
            
            return background;
        }
        
        public override string GetChoiceTooltip(IFlowObject flowObject)
        {
            string tooltip = string.Empty;
            
            // Probably optional for us, especially since we're targeting mobile. I don't think the
            // functionality is properly built out in the AltEnding package yet anyway.
            
            return tooltip;
        }
        
        public override string GetImageFlavorText(IFlowObject flowObject)
        {
            string flavorText = string.Empty;
            
            // I'm not sure if we plan to show images during dialogue or not, but I'm guessing this
            // could be optional.
            
            return flavorText;
        }
        
        public override List<string> ParseArticyAnalyticsEvents(IFlowObject flowObject)
        {
            List<string> events = new List<string>();
            
            // TODO: If we want to include analytics triggers based on Articy dialogue, we'll want to do
            // something with this.
            
            return events;
        }
        
        public override string GetSpecialActionType(IFlowObject flowObject)
        {
            string actionType = string.Empty;

            if (flowObject is IObjectWithFeatureCinematic_Dialogue_Features objectWithFeature)
            {
                var feature = objectWithFeature.GetFeatureCinematic_Dialogue_Features();
                var cameraAngle = feature.Camera_Angle_01;
                var dialogue = flowObject as DialogueFragment;
                var entity = dialogue?.Speaker as Entity;
                string assetId = $"{feature.SceneValue}_{cameraAngle}_{entity?.DisplayName}_{feature.LineValue}";
                return $"CameraAngle|{cameraAngle}|AssetID|{assetId}";
            }
            
            return actionType;
        }
        
        public override string GetDisplayNameFromObject(ArticyObject articyObject)
        {
            if (articyObject is Entity entity)
                return entity.DisplayName;
            
            return string.Empty;
        }
        
        public override string GetDisplayNameFromObject(IFlowObject flowObject)
        {
            if (flowObject is DialogueFragment fragment)
                return GetDisplayNameFromObject(fragment.Speaker);
            
            return string.Empty;
        }
        
        public override ArticyObject GetImageFromObject(IFlowObject flowObject)
        {
            // If we get image asset features in Articy for showing images during dialogue,
            // we'll want something here. Otherwise, I think we can pass.
            return null;
        }
        
        public override string GetLocationGalleryDescriptionFromObject(IFlowObject flowObject)
        {
            // TODO: When the gallery data is set up in Articy, we can fetch the location name for a
            // gallery entry here.
            return string.Empty;
        }
        
        public override bool ShouldSkipFlow(IFlowObject flowObject)
        {
            // As far as I know, we don't have reason to skip a flow object currently.
            return false;
        }
        
        public override CheckpointFeature GetCheckpointFeature(IFlowObject flowObject)
        {
            // TODO: When checkpoint features are added to Articy, we'll want to fetch them here.
            CheckpointFeature checkpointFeature = new();
            
            return checkpointFeature;
        }
        
        public override StoryFeature GetStoryFeature(IFlowObject flowObject)
        {
            StoryFeature storyFeature = new();

            // TODO: I'm not sure if this is the actual feature we need to grab for this. We'll have to check
            // with Nolan on how he wants to do it.
            if (flowObject is DefaultBasicCharacterFeatureFeature feature)
            {
                storyFeature.ownerId = feature.OwnerId.ToString();
                storyFeature.speakerExpression = string.Empty;
                storyFeature.listenerExpression = string.Empty;
                storyFeature.removeAllDialogParticipants = false;
            }
            
            return storyFeature;
        }
        
        public override GalleryFeature GetGalleryFeature(IFlowObject flowObject)
        {
            GalleryFeature galleryFeature = new();
            
            // TODO: When gallery features are in Articy
            
            return galleryFeature;
        }
        
        public override ImageAsset GetImageAsset(IFlowObject flowObject)
        {
            ImageAsset imageAsset = new();
            
            // TODO: If we want image assets displayed during dialogue
            
            return imageAsset;
        }
        
        public override bool IsCheckpoint(IFlowObject flowObject)
        {
            // TODO: When checkpoint features are implemented in Articy
            return false;
        }
        
        public override bool HasCharacterFeature(ArticyObject articyObject)
        {
            return articyObject is IObjectWithFeatureDefaultBasicCharacterFeature;
        }
        
        public override string ConvertSceneNameToArticyID(string sceneName)
        {
            string id = base.ConvertSceneNameToArticyID(sceneName);
            if (!string.IsNullOrEmpty(id))
                return id;
            
            // Fill this out with mappings of sceneName to Articy ID for your project
            // The Articy ID should be for the location that this scene represents
            switch (sceneName)
            {
                default:
                    return string.Empty;
            }
        }
        
        public override List<CharacterTemplate> GetMainCharacterTemplates()
        {
            List<CharacterTemplate> characterTemplates = new();
            
            List<DefaultMainCharacterTemplate> articyCharacterTemplates = ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>();
            foreach (DefaultMainCharacterTemplate defaultMainCharacterTemplate in articyCharacterTemplates)
            {
                CharacterTemplate newTemplate = new();
                newTemplate.obj = defaultMainCharacterTemplate;
                newTemplate.displayName = defaultMainCharacterTemplate.DisplayName;
                newTemplate.id = defaultMainCharacterTemplate.Id.ToHex();
                characterTemplates.Add(newTemplate);
            }

            return characterTemplates;
        }
        
        public override List<CharacterTemplate> GetSupportingCharacterTemplates()
        {
            List<CharacterTemplate> characterTemplates = new();
            List<DefaultSupportingCharacterTemplate> articyCharacterTemplates = ArticyDatabase.GetAllOfType<DefaultSupportingCharacterTemplate>();
            foreach (DefaultSupportingCharacterTemplate defaultSupportingCharacterTemplate in articyCharacterTemplates)
            {
                CharacterTemplate newTemplate = new();
                newTemplate.obj = defaultSupportingCharacterTemplate;
                newTemplate.displayName = defaultSupportingCharacterTemplate.DisplayName;
                newTemplate.id = defaultSupportingCharacterTemplate.Id.ToHex();
                characterTemplates.Add(newTemplate);
            }

            return characterTemplates;
        }
        
        public override List<LocationTemplate> GetLocationTemplates()
        {
            List<LocationTemplate> locationTemplates = new();
            List<Articy.Special_Assignment.Location> articyTemplates = ArticyDatabase.GetAllOfType<Articy.Special_Assignment.Location>();
            foreach (Articy.Special_Assignment.Location location in articyTemplates)
            {
                LocationTemplate newTemplate = new();
                newTemplate.obj = location;
                newTemplate.displayName =  location.DisplayName;
                newTemplate.id =  location.Id.ToHex();
                newTemplate.backgroundIcon = location.PreviewImage.Asset?.LoadAssetAsSprite();
                locationTemplates.Add(newTemplate);
            }

            return locationTemplates;
        }
        
        public override List<ImageAsset> GetImageAssets()
        {
            List<ImageAsset> imageAssets = new();
            
            // TODO: When and if we have image assets defined in Articy
            
            return imageAssets;
        }
        
        protected override HashSet<string> GetGlobalVariableNames()
        {
            return ArticyGlobalVariables.VariableNames;
        }
    }
}