using UnityEngine;
using UnityEditor;

namespace AltEnding
{
    public class PSB_EditorTools
    {
        [MenuItem("CONTEXT/TMP_Text/Add Multiplatform Size Changer")]
        public static void AddTMPFontSizeChanger(MenuCommand command)
        {
            TMPro.TMP_Text text = (TMPro.TMP_Text)command.context;
            PSB_SetTMPFontSize fontChanger;
            if (text.TryGetComponent<PSB_SetTMPFontSize>(out fontChanger))
            {
                Debug.LogWarning(
                    $"{text.gameObject.name} already had a font changing component on it; Cancelling add component function.",
                    fontChanger);
                return;
            }

            fontChanger = Undo.AddComponent<PSB_SetTMPFontSize>(text.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(fontChanger);
        }
    }
}