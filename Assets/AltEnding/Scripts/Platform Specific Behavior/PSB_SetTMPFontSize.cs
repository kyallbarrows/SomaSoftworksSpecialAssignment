using AltEnding.Settings;
using UnityEngine;
using TMPro;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    public class PSB_SetTMPFontSize : PlatformSpecificBehavior
    {
#if UseNA
        [Required]
#endif
        [SerializeField] private TMP_Text text;

        #region Pass Variables
#if UseNA
        [ShowIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float passMinFontSize;
#if UseNA
        [ShowIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float passMaxFontSize;
#if UseNA
        [HideIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float passFontSize;
        
        private AnimationCurve passSizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();

        #endregion

        #region Fail Variables
#if UseNA
        [ShowIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float failMinFontSize;
#if UseNA
        [ShowIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float failMaxFontSize;
#if UseNA
        [HideIf(nameof(isAutoSizing))]
#endif
        [SerializeField]
        private float failFontSize;
        
        private AnimationCurve failSizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();

        #endregion

        private bool isAutoSizing => text == null ? false : text.enableAutoSizing;
#if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();
            passSizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            if (text == null && !TryGetComponent<TMP_Text>(out text))
            {
                text = GetComponentInChildren<TMP_Text>();
            }

            if (text == null) return;

            GetPassValuesFromText();
            GetFailValuesFromText();

        }
#endif
#if UseNA
        [Button("Save Pass Values")]
#endif
        [ContextMenu("Save Pass Values")]
        protected void GetPassValuesFromText()
        {
            if (text == null) return;

            passFontSize = text.fontSize;
            passMinFontSize = text.fontSizeMin;
            passMaxFontSize = text.fontSizeMax;
        }
#if UseNA
        [Button("Save Fail Values")]
#endif
        [ContextMenu("Save Fail Values")]
        protected void GetFailValuesFromText()
        {
            if (text == null) return;

            failFontSize = text.fontSize;
            failMinFontSize = text.fontSizeMin;
            failMaxFontSize = text.fontSizeMax;
        }
#if UseNA
        [Button("Test Pass")]
#endif
        protected override void PlatformCheckPass()
        {
            base.PlatformCheckPass();
            currentPassState = true;
            if (text == null) return;

            text.fontSize = passFontSize;
            text.fontSizeMin = passMinFontSize;
            text.fontSizeMax = passMaxFontSize;
        }
#if UseNA
        [Button("Test Fail")]
#endif
        protected override void PlatformCheckFail()
        {
            base.PlatformCheckFail();
            currentPassState = false;
            if (text == null) return;

            text.fontSize = failFontSize;
            text.fontSizeMin = failMinFontSize;
            text.fontSizeMax = failMaxFontSize;
        }

        #region Font Size Setting

        #region Font Size Variables
#if UseNA
        [OnValueChanged(nameof(UpdateFontSizeInspector))]
#endif
        [SerializeField, Range(0f, 1f)]
        private float currentSettingValue = 0.5f;

        private AnimationCurve sizeModifierCurve
        {
            get { return currentPassState ? ref passSizeModifierCurve : ref failSizeModifierCurve; }
            set
            {
                if (currentPassState)
                    passSizeModifierCurve = value;
                else
                    failSizeModifierCurve = value;
            }
        }

        private float originalFontSize
        {
            get { return currentPassState ? ref passFontSize : ref failFontSize; }
            set
            {
                if (currentPassState)
                    passFontSize = value;
                else
                    failFontSize = value;
            }
        }

        private Vector2 originalFontSizeRange
        {
            get
            {
                return currentPassState
                    ? new Vector2(passMinFontSize, passMaxFontSize)
                    : new Vector2(failMinFontSize, failMaxFontSize);
            }
            set
            {
                if (currentPassState)
                {
                    passMinFontSize = value.x;
                    passMaxFontSize = value.y;
                }
                else
                {
                    failMinFontSize = value.x;
                    failMinFontSize = value.y;
                }
            }
        }

        #endregion

        #region Font Size Functions

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
            UpdateFontSize(SettingsManager.FontSizeFloat(setting.intValue));
        }

        private void UpdateFromSettings()
        {
            if (SettingsManager.instance_Initialised)
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
        }

        private void UpdateFontSize(float samplePoint)
        {
            if (text == null || sizeModifierCurve == null) return;

            float currentModifier = sizeModifierCurve.Evaluate(Mathf.Clamp01(samplePoint));
            text.fontSize = originalFontSize * currentModifier;
            text.fontSizeMin = originalFontSizeRange.x * currentModifier;
            text.fontSizeMax = originalFontSizeRange.y * currentModifier;
        }

#if UNITY_EDITOR
        public void GetDefaultFontSize()
        {
            if (text == null) return;
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3)
                sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            originalFontSize = text.fontSize;
            originalFontSizeRange = new Vector2(text.fontSizeMin, text.fontSizeMax);
        }

        public void MoveDefaultFontSize()
        {
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3)
                sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
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
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3)
                sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();
            Keyframe[] keys = sizeModifierCurve.keys;
            keys[0].value = size / originalFontSize;
            sizeModifierCurve.keys = keys;
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(sizeModifierCurve, 0,
                UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(sizeModifierCurve, 1,
                UnityEditor.AnimationUtility.TangentMode.Linear);
        }

        public void GetMaximumSize()
        {
            if (text == null) return;
            GetMaximumSize(text.fontSize);
        }

        public void GetMaximumSize(float size)
        {
            if (sizeModifierCurve == null || sizeModifierCurve.length < 3)
                sizeModifierCurve = SettingsManager.DefaultSizeModifierCurve();

            Keyframe[] keys = sizeModifierCurve.keys;
            keys[2].value = size / originalFontSize;
            sizeModifierCurve.keys = keys;
            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(sizeModifierCurve, 2,
                UnityEditor.AnimationUtility.TangentMode.Linear);
            UnityEditor.AnimationUtility.SetKeyRightTangentMode(sizeModifierCurve, 1,
                UnityEditor.AnimationUtility.TangentMode.Linear);
        }

        private void SetDefaultSize() => UpdateFontSize(0.5f);

        private void SetMinimumSize() => UpdateFontSize(0f);

        private void SetMaximumSize() => UpdateFontSize(1f);
#endif

        #endregion

        #endregion
    }
}