using AltEnding;
using UnityEngine;

namespace SpecialAssignment
{
    public class CharacterReferences : MonoBehaviour
    {
        public string characterName;
        public Transform audioTransform;
        public Animator animator;
        public string idleName;

        private void Awake()
        {
            DialogueMediaPlayer.AddCharacterReferences(this);
        }
    }
}