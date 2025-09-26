using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class ArticyGallery : MonoBehaviour
    {
        [SerializeField]
        private Button buttonPrefab;

        [SerializeField]
        private Transform buttonsHolder;

        [SerializeField]
        private List<string> articyTechnicalNames;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var atn in articyTechnicalNames) {
                var btnGO = GameObject.Instantiate(buttonPrefab, buttonsHolder);
                var btnScript = btnGO.GetComponent<ArticyGalleryButton>();
                btnScript.TechnicalName = atn;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
