using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class DialogueTextUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private TextMeshProUGUI textBody;

        public void SetName(string newName)
        {
            characterName.SetText(newName.ToUpper());
        }

        public void SetText(string newText)
        {
            textBody.SetText(newText.ToUpper());
        }
    }
}
