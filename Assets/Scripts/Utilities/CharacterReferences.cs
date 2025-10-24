using AltEnding;
using UnityEngine;

namespace SpecialAssignment
{
    public class CharacterReferences : MonoBehaviour
    {
        public string characterName;
        public Transform audioTransform;
        public Animator animator;

        private void Awake()
        {
            SampleUIController.AddCharacterReferences(this);
        }
    }
}