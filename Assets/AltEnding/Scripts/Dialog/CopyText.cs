using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif
using TMPro;

namespace AltEnding.Dialog
{
    [DisallowMultipleComponent]
    public class CopyText : MonoBehaviour
    {
#if UseNA
        [Required]
#endif
        [SerializeField] protected TextMeshProUGUI myTMPText;
#if UseNA
        [Required]
#endif
        [SerializeField] protected TextMeshProUGUI targetTMPText;
        
        [SerializeField] protected bool copyOnEnable;
        [SerializeField] protected bool copyOnStart;

        private void OnValidate()
        {
            if (myTMPText == null) myTMPText = GetComponent<TextMeshProUGUI>();
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
            if (myTMPText == null || targetTMPText == null) return;
            myTMPText.text = targetTMPText.text;
        }
    }
}
