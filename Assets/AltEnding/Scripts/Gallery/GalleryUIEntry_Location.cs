using System;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Gallery
{
    public class GalleryUIEntry_Location : GalleryUIEntry
    {
		public static Action<Location> locationSelected;

#if UNITY_EDITOR && UseNA
		[OnValueChanged(nameof(MyLocationChanged))]
#endif
		[SerializeField] protected Location myLocation;

		protected void Start()
		{
			previewImage.sprite = myLocation.backgroundIcon;
		}

		protected override bool ShouldShow()
		{
			if (!GalleryManager.instance_Initialised) return false;
			if (myLocation == null) return false;
			return GalleryManager.instance.IsLocationUnlocked(myLocation.articyHexID);
		}

		public override void Selected()
		{
			if (showing) locationSelected?.Invoke(myLocation);
		}

#if UNITY_EDITOR
		private void MyLocationChanged()
		{
			if (myLocation != null)
			{
				gameObject.name = myLocation.name;
			}
		}
#endif
	}
}
