using UnityEngine;
using Articy.Unity;
using Articy.Unity.Utils;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Dialog
{
    public class DialogImageAsset : ScriptableObject
    {
        private const string _addressableSuffix = "_DIA";
        [Header("Articy Info")]
        [SerializeField]
        private ArticyRef articyImageAssetReference;
        public ArticyObject articyObject { get { return articyImageAssetReference != null ? (ArticyObject)articyImageAssetReference : null; } }
        public string articyHexID { get { return articyObject != null ? articyObject.Id.ToHex() : ""; } }
        public string addressablesAddress { get { return $"{articyHexID}{_addressableSuffix}"; } }

#if UseNA
        [Label("Flavor Text (from articy)"), ReadOnly]
#endif
        [SerializeField, TextArea(1, 10), Tooltip("Additional text, translated and potentially shown to the player in game along with (or instead of) the image, like in a gallery.")]
        private string flavorText;
        
        [SerializeField,  TextArea(1, 10)]
        private string developerNotes;
#if UseNA
        [ShowAssetPreview, HideIf(nameof(UsingRenderTexture))]
#endif
        public Sprite displaySprite;
#if UseNA
        [HideIf(nameof(UsingSprite))]
#endif
		[SerializeField]
		public RenderTexturePrefab renderTexturePrefab;
#if UseNA
        [ShowAssetPreview, ReadOnly, ShowIf(nameof(UsingRenderTexture))]
#endif
		public RenderTexture renderTexture;

        bool UsingRenderTexture { get { return renderTexturePrefab != null; } }
        bool UsingSprite { get { return displaySprite != null; } }
        
        public string GetFlavorText()
        {
            if (articyImageAssetReference.HasReference)
                return ArticyStoryHelper.Instance.GetImageFlavorText(articyImageAssetReference.GetObject());

            return flavorText;
        }

#if UNITY_EDITOR
        #region Import Data From Articy
        public void ImportArticyData(ImageAsset imageAssetTemplate)
        {
            //Update this to reference new entity values
            articyImageAssetReference = (ArticyRef)imageAssetTemplate.obj;
            if (displaySprite == null) displaySprite = imageAssetTemplate.displaySprite;
            developerNotes = imageAssetTemplate.developerNotes;
            flavorText = imageAssetTemplate.flavorText;
            
            //Is there a way to easily set addressable addresses?
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Reload Articy Data")]
        private void ReloadArticyData()
        {
            if (articyImageAssetReference.HasReference)
            {
                ImageAsset imageAsset = ArticyStoryHelper.Instance.GetImageAsset(articyImageAssetReference.GetObject());
                if (imageAsset != null)
                    ImportArticyData(imageAsset);
            }
        }
        #endregion
#endif
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
    }
}
