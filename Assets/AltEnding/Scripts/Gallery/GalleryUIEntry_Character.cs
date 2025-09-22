using System;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Gallery
{
    public class GalleryUIEntry_Character : GalleryUIEntry
    {
		public static Action<DialogPortraitPackage> characterSelected;

#if UNITY_EDITOR && UseNA
		[OnValueChanged(nameof(MyCharacterChanged))]
#endif
		[SerializeField] protected DialogPortraitPackage myContact;

		protected void Start()
		{
			previewImage.sprite = myContact.staticAvatar;
		}

		protected override bool ShouldShow()
		{
			if (!GalleryManager.instance_Initialised) return false;
			if (myContact == null) return false;
			return GalleryManager.instance.IsCharacterUnlocked(myContact.articyHexID);
		}

		public override void Selected()
		{
			if (showing) characterSelected?.Invoke(myContact);
		}

#if UNITY_EDITOR
		private void MyCharacterChanged()
		{
			if (myContact != null)
			{
				gameObject.name = myContact.name;
			}
		}
#endif
	}
}
