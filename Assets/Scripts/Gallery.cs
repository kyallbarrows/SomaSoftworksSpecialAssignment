using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class Gallery : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI characterName;
        
        [SerializeField]
        private TextMeshProUGUI characterDescription;
        
        [SerializeField]
        private List<string> names;
        
        [SerializeField]
        private List<string> descriptions;
        
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnClickCharacter(int index)
        {
            if (index < names.Count && index < descriptions.Count)
            {
                characterName.text = names[index];
                characterDescription.text = descriptions[index];
            }
        }
    }
}
