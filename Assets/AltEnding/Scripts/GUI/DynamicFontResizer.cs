using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AltEnding.Settings;
#if UseNA
using NaughtyAttributes;
#endif

namespace SomaGUI
{
    public class DynamicFontResizer : MonoBehaviour
    {
#if UseNA
        [Required]
#endif
        [SerializeField]
        private TMP_Text text;
#if UseNA
        [ReadOnly, Foldout("Debug Info")]
#endif
        [SerializeField]
        private float originalFontSize;
#if UseNA
        [ReadOnly, Foldout("Debug Info")]
#endif
        [SerializeField]
        private Vector2 originalFontSizeRange;
#if UseNA
        [OnValueChanged(nameof(UpdateFontSizeInspector))]
#endif
        [SerializeField, Range(0f, 1f), ]
        private float currentSettingValue = 0.5f;
        [SerializeField]
        private AnimationCurve sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();

#if UNITY_EDITOR
        protected void Reset()
        {
            sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            if (text == null && !TryGetComponent<TMP_Text>(out text))
            {
                text = GetComponentInChildren<TMP_Text>();
            }

            if (text == null) return;
            GetDefaultFontSize();
        }
#endif

        private void Awake()
        {
            if (text == null && !TryGetComponent<TMP_Text>(out text))
            {
                text = GetComponentInChildren<TMP_Text>();
            }

            if (text == null)
            {
                enabled = false;
                return;
            }
            GetDefaultFontSize();
        }

        private void OnEnable()
        {
            SettingsManager.WhenLoaded(UpdateFromSettings);
            SettingsManager.SettingChanged += SettingsManagerSettingChanged;
        }

        private void OnDisable()
        {
            SettingsManager.SettingChanged -= SettingsManagerSettingChanged;
        }

        private void SettingsManagerSettingChanged(Setting setting)
        {
            if (setting.mySettingType != SettingType.FontSize) return;

            currentSettingValue = SettingsManager.FontSizeFloat(setting.intValue);
            UpdateFontSize(SettingsManager.FontSizeFloat(setting.intValue));
        }

        private void UpdateFromSettings()
        {
            if (!SettingsManager.instance_Initialised) return;

            currentSettingValue = SettingsManager.instance.FontSizeValue();
                UpdateFontSize(SettingsManager.instance.FontSizeValue());
        }

        private void UpdateFontSizeInspector()
        {
            UpdateFontSize(currentSettingValue);
        }

        public void UpdateFontSizeExternal(float samplePoint)
        {
            currentSettingValue = samplePoint;
            UpdateFontSize(samplePoint);
            text?.ForceMeshUpdate();
        }

        private void UpdateFontSize(float samplePoint)
        {
            if (text == null || sizeModifierCurve == null) return;

            float currentModifier = sizeModifierCurve.Evaluate(Mathf.Clamp01(samplePoint));
            text.fontSize = originalFontSize * currentModifier;
            text.fontSizeMin = originalFontSizeRange.x * currentModifier;
            text.fontSizeMax = originalFontSizeRange.y * currentModifier;
        }

		public void GetDefaultFontSize()
		{
			if (text == null) return;
			if (sizeModifierCurve == null || sizeModifierCurve.length < 3) sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
			originalFontSize = text.fontSize;
			originalFontSizeRange = new Vector2(text.fontSizeMin, text.fontSizeMax);
		}

#if UNITY_EDITOR
        public void MoveDefaultFontSize()
        {
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3) sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            float maxSize = originalFontSize * sizeModifierCurve.Evaluate(1f);
            float minSize = originalFontSize * sizeModifierCurve.Evaluate(0f);
            GetDefaultFontSize();
            GetMinimumSize(minSize);
            GetMaximumSize(maxSize);
        }

        public void GetMinimumSize()
        {
            if (text == null) return;
            GetMinimumSize(text.fontSize);
        }

        public void GetMinimumSize(float size)
        {
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3) sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            Keyframe[] keys = sizeModifierCurve.keys;
            keys[0].value = size / originalFontSize;
            sizeModifierCurve.keys = keys;
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(sizeModifierCurve, 0, UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(sizeModifierCurve, 1, UnityEditor.AnimationUtility.TangentMode.Linear);
        }

        public void GetMaximumSize()
        {
            if (text == null) return;
            GetMaximumSize(text.fontSize);
        }

        public void GetMaximumSize(float size)
        {
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3) sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();

            Keyframe[] keys = sizeModifierCurve.keys;
            keys[2].value = size / originalFontSize;
            sizeModifierCurve.keys = keys;
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(sizeModifierCurve, 2, UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(sizeModifierCurve, 1, UnityEditor.AnimationUtility.TangentMode.Linear);
        }

        private void SetDefaultSize() => UpdateFontSize(0.5f);

        private void SetMinimumSize() => UpdateFontSize(0f);

        private void SetMaximumSize() => UpdateFontSize(1f);
#endif

	}
}
