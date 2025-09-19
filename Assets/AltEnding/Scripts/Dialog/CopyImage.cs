using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Dialog
{
    [DisallowMultipleComponent]
    public class CopyImage : MonoBehaviour
    { 
#if UseNA
        [Required]
#endif
        [SerializeField] protected Image myImage;
#if UseNA
        [Required]
#endif
        [SerializeField] protected Image targetImage;
        [SerializeField] protected bool copyOnEnable;
        [SerializeField] protected bool copyOnStart;

        private void OnValidate()
        {
            if (myImage == null) myImage = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (copyOnEnable) CopyNow();
        }

        private void Start()
        {
            if (copyOnStart) CopyNow();
        }

        public void CopyNow()
        {
            if (myImage == null || targetImage == null) return;
            myImage.sprite = targetImage.sprite;
        }
    }
}