using UnityEngine;
using TMPro;

namespace AltEnding.Dialog
{
    public class DialogSnippetGUI : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI bodyLabel;
        [SerializeField]
        protected TextMeshProUGUI nameLabel;
#if UseMasterAudio
#if UseNA
        [NaughtyAttributes.InfoBox("If assigned, will trigger 'CodeTriggeredEvent1' when content is set.")]
#endif
        [SerializeField]
        protected DarkTonic.MasterAudio.EventSounds audioReference;
#endif

        public string bodyText { get { return bodyLabel ? bodyLabel.text : ""; } }
        public string nameText { get { return nameLabel ? nameLabel.text : ""; } }

        public void SetContentOnly(string bodyString)
		{
            SetContent(bodyString, nameText);
		}

        public void SetNameOnly(string nameString)
		{

            SetContent(bodyText, nameString);
        }

        public void SetContent(string bodyString, string nameString)
		{
            Debug.Log($"[DSGUI] Set Content: {(nameString != null ? nameString : "null")}, {(bodyString != null ? bodyString : "null")}");
            if (bodyLabel != null) bodyLabel.text = !string.IsNullOrWhiteSpace(bodyString) ? bodyString : "...";
            if (nameLabel != null) nameLabel.text = !string.IsNullOrWhiteSpace(nameString) ? nameString : "???";
#if UseMasterAudio
            if (audioReference != null) audioReference.ActivateCodeTriggeredEvent1();
#endif
		}

        public void ClearAll()
		{
            if (bodyLabel != null) bodyLabel.text = "";
            if (nameLabel != null) nameLabel.text = "";
        }
    }
}
