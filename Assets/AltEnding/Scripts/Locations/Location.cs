using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.Unity.Utils;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    [CreateAssetMenu(fileName = "Location", menuName = "ScriptableObjects/Location", order = 300)]
    public class Location : ScriptableObject
    {
        public const string _addressableSuffix = "_LOC";
        [Header("Articy Info")]
        [SerializeField]
        private ArticyRef articyReference;
        public ArticyObject articyObject { get { return articyReference != null ? (ArticyObject)articyReference : null; } }
        public string articyHexID { get { return articyObject != null ? articyObject.Id.ToHex() : ""; } }
        public string addressablesAddress { get { return $"{articyHexID}{_addressableSuffix}"; } }

        [Header("Location Info")]
        public string displayName;
#if UseNA
        [Scene]
#endif
        public string sceneName;
#if UseNA
        [ShowAssetPreview]
#endif
        public Sprite backgroundIcon;

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

        public string GetGalleryDescription()
        {
            if (articyReference != null)
                return ArticyStoryHelper.Instance.GetLocationGalleryDescriptionFromObject(articyObject);

            return null;
        }

#if UNITY_EDITOR
		#region Resetting values changed in play mode
		bool initialUnlocked;
        bool initialUnreadUpdate;

        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
        }

        private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                StoreInitialRuntimeValues();
            }
            else if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                RestoreInitialRuntimeValues();
            }
        }

        public void StoreInitialRuntimeValues()
        {

        }

        public void RestoreInitialRuntimeValues()
        {

        }
        #endregion

        #region Import Data From Articy
        public void ImportArticyData(LocationTemplate locationTemplate)
        {
            articyReference = (ArticyRef)locationTemplate.obj;
            displayName = locationTemplate.displayName.Trim();
            if (backgroundIcon == null)
                backgroundIcon = locationTemplate.backgroundIcon;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endregion
#endif
    }
}
