using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class EvidenceMeter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _evidenceAmountTF;

        void Awake()
        {
            //var evidenceManagerInstance = EvidenceManager.Instance;
        }

        void Start()
        {
            EventBetter.Listen(this, (EvidenceGatheredMessage msg) => UpdateText(msg.NewEvidenceRatio));
            UpdateText(EvidenceManager.Instance.EvidenceRatio);

            UniTask.Delay(5000)
                .ContinueWith(() => EvidenceManager.Instance.AddEvidenceItem(EvidenceManager.EVIDENCE_MAP));
        }

        private void UpdateText(float evidenceRatio)
        {
            _evidenceAmountTF.text = $"Evidence: {Mathf.Round(evidenceRatio * 100f)}%";
        }
    }
}
