using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpecialAssignment
{
    public class EvidenceGatheredMessage
    {
        public float NewEvidenceRatio;
    }


    public class EvidenceManager : MonoSingleton<EvidenceManager>
    {
        public float EvidenceRatio { get; private set; }
        private Dictionary<string, float> _evidenceItems;
        private List<string> _acquiredEvidenceItems;

        public const string EVIDENCE_MAP = "evidence_map";

        public new void Awake()
        {
            base.Awake();
            
            _evidenceItems = new Dictionary<string, float>();
            _evidenceItems.Add(EVIDENCE_MAP, .05f);
            
            _acquiredEvidenceItems = new List<string>();
        }

        public void AddEvidenceItem(string id)
        {
            if (!_evidenceItems.ContainsKey(id))
                throw new Exception("Evidence Item not found: " + id);

            if (_acquiredEvidenceItems.Contains(id))
                return;
            
            EvidenceRatio += _evidenceItems[id];
            _acquiredEvidenceItems.Add(id);
            EventBetter.Raise(new EvidenceGatheredMessage { NewEvidenceRatio = EvidenceRatio});
        }
    }
}