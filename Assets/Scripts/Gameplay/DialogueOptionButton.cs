using System;
using AltEnding;
using Articy.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class DialogueOptionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        private Image buttonImage;
        private bool active;
        private Branch branch;

        private void Awake()
        {
            buttonImage = GetComponent<Image>();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!active)
            {
                active = true;
                buttonImage.sprite = activeSprite;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (active)
            {
                active = false;
                buttonImage.sprite = inactiveSprite;
                if (branch != null)
                    ArticyFlowController.Instance.PlayBranch(branch);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (active)
            {
                active = false;
                buttonImage.sprite = inactiveSprite;
            }
        }
        
        public void SetText(string newText)
        {
            buttonText.SetText(newText.ToUpper());
        }

        public void SetBranch(Branch branch)
        {
            this.branch = branch;
        }

        public void Reset()
        {
            branch = null;
            buttonText.SetText(String.Empty);
        }
    }
}