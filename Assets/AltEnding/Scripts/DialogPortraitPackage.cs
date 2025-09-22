using System;
using UnityEngine;
using Articy.Unity;
using Articy.Unity.Utils;
using Articy.Unity.Interfaces;

namespace AltEnding
{
    [CreateAssetMenu(fileName = "New DPP", menuName = "ScriptableObjects/Dialog Portrait Package", order = 100)]
    public class DialogPortraitPackage : ScriptableObject
    {
        private const string _addressableSuffix = "_DPP";
        [Header("Articy References")]
#if UseNA
        [NaughtyAttributes.OnValueChanged("GetAssetsFromArticyRef")]
#endif
        public ArticyRef articyReference;
        public ArticyObject articyObject { get { return articyReference != null ? (ArticyObject)articyReference : null; } }
        public string articyHexID { get { return articyObject != null ? articyObject.Id.ToHex() : ""; } }
        public string addressablesAddress { get { return $"{articyHexID}{_addressableSuffix}"; } }
        [field: SerializeField]
        public string displayName { get; private set; }

        [Header("Avatar Files")]
#if UseNA
        [NaughtyAttributes.ShowAssetPreview]
#endif
        public Sprite staticAvatar;

        public SerializableDictionary<string, AnimationClip> expressionAnimations;

		private void OnValidate()
		{
            AddAllAnimationReference();
		}

		private void AddAllAnimationReference()
        {
            if (expressionAnimations.ContainsKey("Dont_Change"))
            {
                expressionAnimations.Remove("Dont_Change");
            }

            var expressionValues = ArticyStoryHelper.Instance.GetAllSpeakerExpressionDescriptions();

            if (expressionValues.Count - 1 == expressionAnimations.Count && !expressionAnimations.ContainsValue(null))
                return;
            // At this point, there is no "don't change" value, but the amount of animations still doesn't math the amount of expressions. Ensure that it does.
            // Start with neutral so that it's first.
            AnimationClip neutralAnimation = null;
            if (!expressionAnimations.TryGetValue("Neutral", out neutralAnimation))
			{
                expressionAnimations.Add("Neutral", null);
			}
            AnimationClip referenceAnimation = null;
            for (int c = 0; c < expressionValues.Count; c++)
            {
                if (expressionValues[c].Equals("Dont_Change")) continue;
                referenceAnimation = null;
                if (!expressionAnimations.TryGetValue(expressionValues[c], out referenceAnimation))
                {
                    expressionAnimations.Add(expressionValues[c], neutralAnimation);
                }
                else if(referenceAnimation == null && neutralAnimation != null)
				{
                    expressionAnimations[expressionValues[c]] = neutralAnimation;
				}
            }
		}

        public AnimationClip GetExpressionAnimation(string input)
		{
            AnimationClip animationClip;
            if (expressionAnimations.TryGetValue(input, out animationClip))
                return animationClip;
            else if (expressionAnimations.TryGetValue("Neutral", out animationClip))
                return animationClip;
            else
                return null;
        }

		public void UpdateArticyRef(ArticyRef articyRef, bool updateDisplayName = false)
		{
            articyReference = articyRef;
            if (updateDisplayName || String.IsNullOrWhiteSpace(displayName))
                displayName = articyReference != null
                    ? ArticyStoryHelper.Instance.GetDisplayNameFromObject(articyObject)
                    : "Null";
        }

        public void UpdateArticyRef(ArticyObject articyRef, bool updateDisplayName = false)
        {
            articyReference = (ArticyRef)articyRef;
            if(updateDisplayName || String.IsNullOrWhiteSpace(displayName))
                displayName = articyRef != null 
                    ? ArticyStoryHelper.Instance.GetDisplayNameFromObject(articyObject)
                    : "Null";
        }

        public DialogPortraitPackage(ArticyRef articyRef)
		{
            if (ArticyStoryHelper.Instance.HasCharacterFeature((ArticyObject)articyRef))
            {
                articyReference = articyRef;
                GetAssetsFromArticyRef();
            }
		} 

        [ContextMenu("Get Assets From Articy Reference")]
        public void GetAssetsFromArticyRef()
		{
            if (articyReference == null) {
                Debug.Log("Reference is Null");
                return;
            }
            
            if (!ArticyStoryHelper.Instance.HasCharacterFeature(articyObject))
            {
                Debug.Log("Reference is not an object with a character feature");
                return;
            }

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = ArticyStoryHelper.Instance.GetDisplayNameFromObject(articyObject);
            
            if (staticAvatar == null && articyObject is IObjectWithPreviewImage previewImage)
                staticAvatar = previewImage.PreviewImage?.Asset?.LoadAssetAsSprite();
        }

        [ContextMenu("Get Addressables Address")]
        public void GetAddressableString()
		{
            addressablesAddress.CopyToClipboard();
		}

        public static string GetAddressableAddress(ArticyObject articyObject)
        {
            if (articyObject == null)
                return null;

            return $"{articyObject.Id.ToHex()}{_addressableSuffix}";
        }

        public static string GetAddressableAddress(string articyObjectAddress)
        {
            return $"{articyObjectAddress}{_addressableSuffix}";
        }
    }
}
