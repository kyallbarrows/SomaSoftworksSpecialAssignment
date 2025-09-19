using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets;

namespace AltEnding.Dialog
{
    public class DialogImageGUI : MonoBehaviour
    {
		[SerializeField]
        protected bool doDebugs;
        [SerializeField]
        protected Image bodyImage;
		[SerializeField]
		protected RawImage rawImage;
		[SerializeField]
		protected LayoutElement rawImageElement;

		[field: SerializeField
#if UseNA
        ,NaughtyAttributes.ReadOnly
#endif
        ]
		public bool isLoading { get; protected set; }

		[field: SerializeField
#if UseNA
        ,NaughtyAttributes.ReadOnly
#endif
        ]
		public string imageAddress { get; protected set; }
		[SerializeField]
        protected TextMeshProUGUI nameLabel;
#if UseMasterAudio
#if UseNA
        [NaughtyAttributes.InfoBox("If assigned, will trigger 'CodeTriggeredEvent1' when content is set.")]
#endif
        [SerializeField]
        protected DarkTonic.MasterAudio.EventSounds audioReference;
#endif

		private AsyncOperationHandle<IList<IResourceLocation>> imageLocationsOpHandle;
		private IList<IResourceLocation> imageLocations;
		private AsyncOperationHandle<DialogImageAsset> imageLoadOpHandle;

		[field: SerializeField] public DialogImageAsset myDIA { get; private set; }

		public Sprite bodySprite { get { return bodyImage ? bodyImage.sprite : null; } }
		public string nameText { get { return nameLabel ? nameLabel.text : ""; } }

		public void SetContent(string imageAddress, string nameString)
        {
			EasyDebug($"[DIGUI] Set Content: {(nameString != null ? nameString : "null")}, {(bodyImage != null ? bodyImage.name : "null")} -> Image at address: {imageAddress}; Current state: {gameObject.activeInHierarchy}");

			if (!gameObject.activeInHierarchy) return;
			if (nameLabel != null) nameLabel.text = !string.IsNullOrWhiteSpace(nameString) ? nameString : "???";
#if UseMasterAudio
            if (audioReference != null) audioReference.ActivateCodeTriggeredEvent1();
#endif
			CheckLocationAddress(imageAddress);
        }

		public void SetContent(Sprite image, string nameString)
		{
			EasyDebug($"[DIGUI] Set Content: {(nameString != null ? nameString : "null")}, {(bodyImage != null ? bodyImage.name : "null")} -> {(image != null ? image.name : "null")}; Current state: {gameObject.activeInHierarchy}");
			if (!gameObject.activeInHierarchy) return;
			UpdateImage(image);
            if (nameLabel != null) nameLabel.text = !string.IsNullOrWhiteSpace(nameString) ? nameString : "???";
#if UseMasterAudio
            if (audioReference != null) audioReference.ActivateCodeTriggeredEvent1();
#endif
			isLoading = false;
		}

		public void SetContent(DialogImageAsset newDIA, string nameString)
        {
			EasyDebug($"[DIGUI] Set Content: {(nameString != null ? nameString : "null")},  -> {(newDIA != null ? newDIA.name : "null")}; Current state: {gameObject.activeInHierarchy}");
			if (!gameObject.activeInHierarchy) return;
			if (nameLabel != null) nameLabel.text = !string.IsNullOrWhiteSpace(nameString) ? nameString : "???";
			SetContent(newDIA);
#if UseMasterAudio
			if (audioReference != null) audioReference.ActivateCodeTriggeredEvent1();
#endif
			isLoading = false;
		}

		private void UpdateImage(Sprite image)
		{
			if (rawImage != null)
			{
				rawImage.texture = null;
				if (rawImageElement != null)
				{
					rawImageElement.enabled = false;
				}
				else
				{
					rawImage.gameObject.SetActive(false);
				}
			}

			if (bodyImage != null)
			{
				bodyImage.enabled = true;
				bodyImage.sprite = image;
			}
		}

		private void UpdateRenderTexture(RenderTexture renderTexture)
        {
			if (myDIA != null) myDIA.renderTexture = renderTexture;
			UpdateImage(renderTexture);
        }

		private void UpdateImage(Texture image)
		{
			Debug.Log("[RT] Update Image. ID: " + image.GetInstanceID(), rawImage);
			if (rawImage != null)
			{
				if (rawImageElement != null)
				{
					rawImageElement.enabled = true;
				}
				else
				{
					rawImage.gameObject.SetActive(true);
				}
				rawImage.texture = (RenderTexture)image;
			}

			if (bodyImage != null)
			{
				bodyImage.enabled = false;
				bodyImage.sprite = null;
			}
		}

		public void ClearAll()
		{
            if (bodyImage != null) bodyImage.sprite = null;
            if (nameLabel != null) nameLabel.text = "";
        }

		private void CheckLocationAddress(string address)
		{
			isLoading = true;
			imageAddress = address;
			EasyDebug($"[DIGUI] Dialog image locations check started. \nAddress: {address}");
			imageLocationsOpHandle = Addressables.LoadResourceLocationsAsync(address);
			imageLocationsOpHandle.Completed += ImageLocationsOpHandle_Completed;
		}

		private void ImageLocationsOpHandle_Completed(AsyncOperationHandle<IList<IResourceLocation>> obj)
		{
			EasyDebug($"[DIGUI] Dialog image locations check completed. \nValid: {obj.IsValid()} \nStatus: {obj.Status.ToString()}");
			if (obj.Status == AsyncOperationStatus.Succeeded)
			{
				imageLocations = obj.Result;
				EasyDebug($"[DIGUI] Dialog image locations check Succeeded. \nLength: {imageLocations.Count}");
				if (imageLocations.Count > 0)
				{
					imageLoadOpHandle = Addressables.LoadAssetAsync<DialogImageAsset>(imageLocations[0]);
					imageLoadOpHandle.Completed += OnImageLoadComplete;
				}
            }
            else
            {
				isLoading = false;
			}
		}

		private void OnImageLoadComplete(AsyncOperationHandle<DialogImageAsset> obj)
		{
			if (obj.Status == AsyncOperationStatus.Succeeded)
			{
				SetContent(obj.Result);
			}
			isLoading = false;
		}

		private void SetContent(DialogImageAsset newDIA)
        {
			if (!gameObject.activeInHierarchy) return;
			myDIA = newDIA;

			if(myDIA.renderTexture != null)
            {
				UpdateRenderTexture(myDIA.renderTexture);
            }
			else if (myDIA.renderTexturePrefab != null)
			{
				RequestRenderTexture();
			}
			else
			{
				UpdateImage(myDIA.displaySprite);
			}
			isLoading = false;
		}

		private void RequestRenderTexture()
		{
			if (myDIA == null || myDIA.renderTexturePrefab == null) return;

			if (myDIA.renderTexture != null)
			{
				//use the already made Render Texture
				UpdateRenderTexture(myDIA.renderTexture);
				return;
			}

			if (!SpeakerVisualsManager.instance_Initialised) {
				SpeakerVisualsManager.WhenInitialized(RequestRenderTexture);
				return;
			}

			if (SpeakerVisualsManager.instance.myDialogCanvasManager == null) return;

			SpeakerVisualsManager.instance.myDialogCanvasManager.InstatiateNewRenderTexturePrefabClone(myDIA.renderTexturePrefab, UpdateRenderTexture);
		}

		private void EasyDebug(string debugMessage)
        {
			if (doDebugs) Debug.Log(debugMessage, this);
        }
	}
}
