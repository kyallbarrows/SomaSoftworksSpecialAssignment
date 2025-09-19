using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UseNA
using NaughtyAttributes;
#endif
using UnityEngine.Events;
using System;

//Including these here for easy Copy/Paste
#if ENABLE_INPUT_SYSTEM
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
#endif

public class EventSystemHelper : MonoBehaviour
{
    public static EventSystemHelper instance;
    [SerializeField]
    protected EventSystem myEventSystem;
#if ENABLE_INPUT_SYSTEM
#if UseNA
    [Label("My Player Input")]
#endif
    [field: SerializeField]
    public PlayerInput myPlayerInput { get; protected set; }
    public InputActionMap storyActionMap => controls.StoryNavigation;
    string storyActionMapName => controls.StoryNavigation.ToString();
#endif
#if UseNA
    [ReadOnly]
#endif
    [SerializeField]
    private string currentActionMap;
#if UseNA
    [ReadOnly]
#endif
    [SerializeField]
    protected Selectable[] allSelectables;
#if UseNA
    [ReadOnly]
#endif
    [SerializeField]
    protected List<Selectable> potentialSelectables;
    
#if ENABLE_INPUT_SYSTEM
    public RedemptionControls controls { get; protected set; }
#endif

    protected void OnValidate()
    {
        if (myEventSystem == null) myEventSystem = GetComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        if (myPlayerInput == null) myPlayerInput = GetComponent<PlayerInput>();
        if (myPlayerInput != null) currentActionMap = myPlayerInput.currentActionMap?.name;
#endif
    }

    private void OnEnable()
    {
        if (instance == null) instance = this;
        else if(instance != this) Destroy(this);
#if ENABLE_INPUT_SYSTEM
        if (controls == null) controls = new RedemptionControls();
#endif
    }
#if ENABLE_INPUT_SYSTEM
    public void OnNavigate(InputAction.CallbackContext context)
    {
        Debug.Log($"[ESH] Navigate Called");
        if (myEventSystem == null) return;
        if (myEventSystem.currentSelectedGameObject != null && IsInteractable(myEventSystem.currentSelectedGameObject))
        {
            return;
        }

        GetCurrentSelectables();
        if (potentialSelectables.Count > 0)
        {
            myEventSystem.SetSelectedGameObject(GetTopSelectable().gameObject);
        }

    }
#endif
    
#if UseNA
	[Button("Get All Selectables")]
#endif
    public void GetCurrentSelectables()
    {
        allSelectables = Selectable.allSelectablesArray;

        if (potentialSelectables == null)
            potentialSelectables = new List<Selectable>();
        else
            potentialSelectables.Clear();

        foreach(Selectable pSelectable in allSelectables)
        {
            if (pSelectable.IsInteractable()) potentialSelectables.Add(pSelectable);
        }
    }

    public Selectable GetTopSelectable()
    {
        if (potentialSelectables == null || potentialSelectables.Count <= 0) return null;
        Selectable result = potentialSelectables[0];
        for(int c = 1; c < potentialSelectables.Count; c++)
        {
            if (potentialSelectables[c]?.transform.position.y > result.transform.position.y) result = potentialSelectables[c];
        }
        return result;
    }

    public bool IsInteractable(GameObject potential)
    {
        if (potential == null) return false;
        Selectable selectable = potential.GetComponent<Selectable>();
        if (selectable == null) return false;
        return selectable.IsInteractable();
    }

#if ENABLE_INPUT_SYSTEM
    public void ToggleStoryNavigation(BooleanStateChange change)
    {
        switch (change)
        {
            case BooleanStateChange.False:
                SetActionMapEnabledState(storyActionMap.name, false);
                break;
            case BooleanStateChange.True:
                SetActionMapEnabledState(storyActionMap.name, true);
                break;
            case BooleanStateChange.Toggle:
                SetActionMapEnabledState(storyActionMap.name, !(storyActionMap != null ? storyActionMap.enabled : false));
                break;
            case BooleanStateChange.LeaveAsIs:
            default:
                break;
        
        }
    }

    private void SetActionMapEnabledState(string actionMapName, bool doEnable)
    {
        InputActionMap map = myPlayerInput?.actions?.FindActionMap(actionMapName);
        SetActionMapEnabledState(map, doEnable);
    }

    private void SetActionMapEnabledState(InputActionMap map, bool doEnable)
    {
        if (map == null) return;
        if (doEnable)
        {
            map.Enable();
        }
        else
        {
            map.Disable();
        }
    }
#endif
}
