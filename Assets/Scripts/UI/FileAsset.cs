using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class FileAssetSelectedMessage
    {
        public int Group;
        public int Id;
    }

    public class FileAsset : MonoBehaviour
    {
        [SerializeField] private string _label;
        [SerializeField] private int _group;
        [SerializeField] private int _id;
        [SerializeField] private GameObject _onState;
        [SerializeField] private GameObject _offState;
        private Button _button;

        void Start()
        {
            GetComponentInChildren<TextMeshProUGUI>().SetText(_label);
            
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                EventBetter.Raise(new FileAssetSelectedMessage()
                {
                    Group = _group,
                    Id = _id
                });

                _onState.SetActive(true);
                _offState.SetActive(false);
            });
        }

        public void Deselect()
        {
            _onState.SetActive(false);
            _offState.SetActive(true);
        }
    }
}
