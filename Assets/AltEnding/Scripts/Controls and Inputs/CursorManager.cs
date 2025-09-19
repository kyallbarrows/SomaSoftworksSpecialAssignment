using System;
using System.Collections.Generic;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    public enum CustomCursorState
    {
        None = 0,
        Default = 1 << 1,
    }

    static class CustomCursorStateExtensions
    {

        public const int totalCustomCursorStates = 3;

        public static CustomCursorState HighestState(this CustomCursorState customCursorState)
        {
            if (customCursorState.HasFlag(CustomCursorState.Default)) return CustomCursorState.Default;

            else return CustomCursorState.None;
        }

        public static void Set<CustomCursorState>(ref CustomCursorState flags, CustomCursorState flag)
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (CustomCursorState)(object)(flagsValue | flagValue);
        }

        public static void Unset<CustomCursorState>(ref CustomCursorState flags, CustomCursorState flag)
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (CustomCursorState)(object)(flagsValue & (~flagValue));
        }
    }
    
    public class CursorManager : PersistentSingleton<CursorManager>
    {
        [Serializable]
        public class CustomCursorSettings
        {
            public CustomCursorState myState;
            public bool visible; 
#if UseNA
            [AllowNesting, OnValueChanged("SetPixelOffset"), ShowAssetPreview, ShowIf("visible")]
#endif
            public Texture2D customIcon;
#if UseNA
            [AllowNesting, ShowIf("visible"), OnValueChanged("SetRelativeOffset")]
#endif
            [Tooltip("In pixels, from top-left corner")]
            public Vector2 pixelOffset;
#if UseNA
            [AllowNesting, OnValueChanged("SetPixelOffset"), ShowIf("visible")]
#endif
            [SerializeField, Range(0f,1f), Tooltip("Relative, from top-left corner")]
            private float xOffset;
#if UseNA
            [AllowNesting, OnValueChanged("SetPixelOffset"), ShowIf("visible")]
#endif
            [SerializeField, Range(0f, 1f), Tooltip("Relative, from top-left corner")]
            private float yOffset;
#if UseNA
            [AllowNesting, ShowIf("visible")]
#endif
            public CursorMode cursorMode;
            
            private void SetPixelOffset()
            {
                if (customIcon == null) return;
                pixelOffset = new Vector2(customIcon.width * xOffset, customIcon.height * yOffset);
            }

            private void SetRelativeOffset()
            {
                if (customIcon == null) return;
                xOffset = pixelOffset.x / customIcon.width;
                yOffset = pixelOffset.y / customIcon.height;
            }

            public CustomCursorSettings(CustomCursorState newState)
            {
                myState = newState;
                customIcon = null;
                pixelOffset = Vector2.zero;
                cursorMode = CursorMode.Auto;
                visible = false;
                xOffset = 0f;
                yOffset = 0f;
            }

            public CustomCursorSettings(CustomCursorState newState, Texture2D newIcon, Vector2 newOffset, CursorMode newCursorMode = CursorMode.Auto)
            {
                myState = newState;
                customIcon = newIcon;
                cursorMode = newCursorMode;
                visible = true;
                pixelOffset = newOffset;
                xOffset = 0f;
                yOffset = 0f;
                if(customIcon != null)
                {
                    xOffset = pixelOffset.x / customIcon.width;
                    yOffset = pixelOffset.y / customIcon.height;
                }
            }

            public CustomCursorSettings(CustomCursorState newState, Texture2D newIcon, float relativeX, float relativeY, CursorMode newCursorMode = CursorMode.Auto)
            {
                myState = newState;
                customIcon = newIcon;
                cursorMode = newCursorMode;
                visible = true;
                xOffset = relativeX;
                yOffset = relativeY;
                pixelOffset = Vector2.zero;
                if(customIcon != null)
                {
                    pixelOffset = new Vector2(customIcon.width * xOffset, customIcon.height * yOffset);
                }
            }
        }
        
#if UseNA
        [EnumFlags, OnValueChanged("UpdateCursor")]
#endif
        [SerializeField]
        protected CustomCursorState _currentCursorMode;
        public CustomCursorState currentCursorMode { get { return _currentCursorMode; } }
#if UseNA
        [ArrayElementName("myState")]
#endif
        [SerializeField]
        private List<CustomCursorSettings> mySettings;
        [SerializeField]
        private List<Component> defaultComponents;

        public void RequestCursor(CustomCursorState cursorState, Component requester)
        {
            if (requester == null) return;
            switch (cursorState)
            {
                case CustomCursorState.Default:
                    defaultComponents.Add(requester);
                    break;
            }
            SetByLists();
        }

        public void ReleaseCursor(CustomCursorState cursorState, Component requester)
        {
            if (requester == null) return;
            switch (cursorState)
            {
                case CustomCursorState.Default:
                    if (defaultComponents.Contains(requester)) defaultComponents.Remove(requester);
                    break;
            }
            SetByLists();
        }

        private void SetByLists()
        {
            CleanRequestorLists();
            if (defaultComponents.Count > 0)
                CustomCursorStateExtensions.Set(ref _currentCursorMode, CustomCursorState.Default);
            else
                CustomCursorStateExtensions.Unset(ref _currentCursorMode, CustomCursorState.Default);
            UpdateCursor();
        }

        protected void UpdateCursor()
        {
            for (int c = 0; c < mySettings.Count; c++)
            {
                if (mySettings[c].myState == currentCursorMode.HighestState())
                {
                    SetCursor(mySettings[c]);
                    return;
                }
            }
            
            //We've looped through all the available settings, and none match the current highest state. Thus, set the system default.
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true;
        }

        protected void SetCursor(CustomCursorSettings cursorSettings)
        {
            Cursor.visible = cursorSettings.visible;
            if (cursorSettings.visible)
            {
                Cursor.SetCursor(cursorSettings.customIcon, cursorSettings.pixelOffset, cursorSettings.cursorMode);
            }
        }
        
        private void CleanRequestorLists()
        {

            if (defaultComponents != null)
            {
                for (int c = defaultComponents.Count - 1; c >= 0; c--)
                {
                    if (defaultComponents[c] == null) defaultComponents.RemoveAt(c);
                }
            }
        }
    }
}

