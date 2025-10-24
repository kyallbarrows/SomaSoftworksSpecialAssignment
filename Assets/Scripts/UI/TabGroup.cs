using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpecialAssignment
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private List<Tab> _tabs;
        [SerializeField] private List<GameObject> _contentPanels;

        public void Start()
        {
            HideEverything();

            EventBetter.Listen(this, (TabClickedMessage msg) => SelectTab(msg.SiblingIndex));
            
            SelectTab(0);
        }

        private void HideEverything()
        {
            foreach (var tab in _tabs) { tab.SetSelected(false); }
            foreach (var panel in _contentPanels) { panel.SetActive(false); }
        }

        private void SelectTab(int index)
        {
            HideEverything();
            _tabs[index].SetSelected(true);
            _contentPanels[index].SetActive(true);
        }
    }
}
