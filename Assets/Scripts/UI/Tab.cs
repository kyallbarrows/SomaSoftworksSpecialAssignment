using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class TabClickedMessage
    {
        public int SiblingIndex;
    }

    public class Tab : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private Image _onState;
        [SerializeField] private Image _offState;
        private TextMeshProUGUI _tabNameText;
        private Button _button;

        public void SetSelected(bool selected)
        {
            Debug.Log("Setting selected " + _name + "  " + selected);
            _onState.enabled = selected;
            _offState.enabled = !selected;
        }

        public void Start()
        {
            _tabNameText = GetComponentInChildren<TextMeshProUGUI>();
            _tabNameText.text = _name;
            
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                EventBetter.Raise(new TabClickedMessage() { SiblingIndex = transform.GetSiblingIndex() });
            });
        }
    }
}
