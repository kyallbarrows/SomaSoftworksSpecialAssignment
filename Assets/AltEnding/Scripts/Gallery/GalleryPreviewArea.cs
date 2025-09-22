using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AltEnding.GUI;

namespace AltEnding.Gallery
{
    public class GalleryPreviewArea : MonoBehaviour
    {
        [SerializeField] protected AnimatedCanvasManager canvasManager;
        [SerializeField] protected GalleryGUIController galleryGUIController;

        [Header("Spine Character Preview")] [SerializeField]
        private GameObject spineCharacterPreviewRoot;

        [SerializeField] private TMP_Text spineCharacterPreviewName;
        [SerializeField] private GallerySpeakerVisualsInstance myGSVI;

        [Header("Location Preview")] [SerializeField]
        private GameObject locationPreviewRoot;

        [SerializeField] private Image locationPreviewImage;
        [SerializeField] private TMP_Text locationPreviewName;
        [SerializeField] private TMP_Text locationPreviewDescription;
        [SerializeField] private Button locationLoadButton;
#if UseNA
    [NaughtyAttributes.ReadOnly]
#endif
        [SerializeField] Location selectedLocation;


        private void OnEnable()
        {
            if (canvasManager != null) canvasManager.TurnOnEvent.AddListener(TurnOn);
            GalleryUIEntry_Location.locationSelected += LocationSelected;
            GalleryUIEntry_Character.characterSelected += CharacterSelected;
        }

        private void OnDisable()
        {
            if (canvasManager != null) canvasManager.TurnOnEvent.RemoveListener(TurnOn);
            GalleryUIEntry_Location.locationSelected -= LocationSelected;
            GalleryUIEntry_Character.characterSelected -= CharacterSelected;
        }

        private void TurnOn()
        {
            spineCharacterPreviewRoot.SetActive(false);
            locationPreviewRoot.SetActive(false);
        }

        private void LocationSelected(Location location)
        {
            spineCharacterPreviewRoot.SetActive(false);

            locationPreviewImage.sprite = location.backgroundIcon;
            locationPreviewName.text = location.displayName;
            locationPreviewDescription.text = location.GetGalleryDescription();
            locationPreviewRoot.SetActive(true);

            selectedLocation = location;
        }

        public void LoadSelectedLocation()
        {
            //Because the scene name is set through a NaughtyAttributes 'Scene' attribute, the default value is the first scene in the build settings: "MainMenu"
            if (selectedLocation != null && galleryGUIController != null)
            {
                galleryGUIController.LoadLocation(selectedLocation);
            }
        }
        
        private void CharacterSelected(DialogPortraitPackage character)
        {
            locationPreviewRoot.SetActive(false);

            spineCharacterPreviewName.text = character.displayName;
            if (character.articyObject != null)
            {
                myGSVI.LoadCharacter(character.articyObject);
            }
            else
            {
                myGSVI.DisplayCharacterWithManualAvatar(character.staticAvatar);
            }

            spineCharacterPreviewRoot.SetActive(true);
        }
    }
}