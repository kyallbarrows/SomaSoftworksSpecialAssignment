using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.GUI
{
    public static class OpenStateExtensions
    {
        public static bool InTransition(this OpenState oS)
        {
            switch (oS)
            {
                case OpenState.Closing:
                case OpenState.Opening:
                    return true;
                default:
                    return false;
            }
        }

        public static bool ClosedOrClosing(this OpenState oS)
		{
			switch (oS)
			{
                case OpenState.Closed:
                case OpenState.Closing:
                    return true;
                default: 
                    return false;
			}
		}

        public static bool OpenOrOpening(this OpenState oS)
        {
            switch (oS)
            {
                case OpenState.Open:
                case OpenState.Opening:
                    return true;
                default:
                    return false;
            }
        }

        public static bool InTransition(this TwoLevelOpenState oS)
        {
            switch (oS)
            {
                case TwoLevelOpenState.Closing:
                case TwoLevelOpenState.SubClosing:
                case TwoLevelOpenState.Opening:
                case TwoLevelOpenState.SubOpening:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class SubCanvasManager : GenericCanvasManager
    {
        public System.Action<int> NewSubMenuIndex;
        
#if UseNA
        [Foldout("Transition Animations")]
#endif
        [SerializeField]
        protected Animation animationComponent;
#if UseNA
        [Foldout("Transition Animations"), Label("Opening Transition")]
#endif
        [SerializeField]
        protected AnimationClip clip_OpenMenu;
#if UseNA
        [Foldout("Transition Animations"), Label("Closing Transition")]
#endif
        [SerializeField]
        protected AnimationClip clip_CloseMenu;

        [Header("SubMenu Canvas Managers")]
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("subMenuManagers"),]
        protected List<GenericCanvasManager> subCanvasManagers;
        public int subCanvasCount { get { return subCanvasManagers != null ? subCanvasManagers.Count : 0; } }
        [SerializeField, Tooltip("Allow the current subcanvas to be 'None' or blank")]
        protected bool allowNone;
        [SerializeField]
        protected bool canWrap;

#if UseNA
        [ReadOnly, Label("Current SubMenu Index")]
#endif
        [SerializeField]
        private int _csmi;
        public int currentSubCanvasIndex
        {
            get { return _csmi; }
            protected set
            {
                _csmi = value;
                NewSubMenuIndex?.Invoke(_csmi);
            }
        }

        #region Helper Variables
        protected bool animationPlaying { get { return animationComponent != null && animationComponent.isPlaying; } }
        public bool currentIndexIsValid { get { return isValidIndex(currentSubCanvasIndex); } }
        [SerializeField] public bool isInTransition
        {
            get
            {
                switch (currentOpenState)
                {
                    case OpenState.Closing:
                    case OpenState.Opening:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public GenericCanvasManager currentSubCanvas
		{
			get
			{
                if (!currentIndexIsValid) return null;
                else return subCanvasManagers[currentSubCanvasIndex];
			}
		}
        public bool subCanvasInTransition
		{
			get
			{
                if (currentIndexIsValid && subCanvasManagers[currentSubCanvasIndex] != null)
                {
                    return true;
                }
                else return false;
			}
		}
        public bool isValidIndex(int index, bool checkCanvasIsNull = false)
		{
            return subCanvasManagers != null && index >= 0 && index < subCanvasManagers.Count && (!checkCanvasIsNull || subCanvasManagers[index] != null);
        }
		#endregion

#if UseNA
        [ReadOnly]
#endif
		[SerializeField]
        protected List<string> postTransitionInvokeQueue;
        protected Coroutine delayedInvoker;
        protected Coroutine swappingCoroutine;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (animationComponent == null) animationComponent = GetComponent<Animation>();
            if(animationComponent != null)
			{
                if (clip_OpenMenu != null && animationComponent.GetClip(clip_OpenMenu.name) == null) animationComponent.AddClip(clip_OpenMenu, clip_OpenMenu.name);
                if (clip_CloseMenu != null && animationComponent.GetClip(clip_CloseMenu.name) == null) animationComponent.AddClip(clip_CloseMenu, clip_CloseMenu.name);
            }
        }
#endif

        protected override void Start()
        {
            ClearAll(false);
            TurnOff();
            NewSubMenuIndex?.Invoke(currentSubCanvasIndex);
        }

        public override void TurnOff()
        {
            SetSubcanvasActive(currentSubCanvasIndex, false);
            base.TurnOff();
            //currentSubMenuIndex = -1;
        }

		public override void TurnOn()
		{
			base.TurnOn();
            SetSubcanvasActive(currentSubCanvasIndex, true);
		}

		protected void LateUpdate()
        {
            if (isInTransition)
			{
				switch (currentOpenState) 
                {
                    case OpenState.Closing:
                        if (!animationPlaying) currentOpenState = OpenState.Closed;
                        break;
                    case OpenState.Opening:
                        if (!animationPlaying) currentOpenState = OpenState.Open;
                        break;
                    case OpenState.Open:
                        //Do something about sub menus
                        break;
                    default:
                        Debug.LogError($"Invalid open state({currentOpenState})! Expected {OpenState.Closing} or {OpenState.Opening}, because 'isInTransition' returned true. Please handle the unexpected state.");
                        currentOpenState = canvas.enabled ? OpenState.Open : OpenState.Closed;
                        break;
				}

            }
        }

        public virtual void OpenRootCanvas()
        {
            if (currentOpenState == OpenState.Closed)
            {
                if (animationComponent != null && clip_OpenMenu != null)
                {
                    animationComponent.Play(clip_OpenMenu.name);
                    currentOpenState = OpenState.Opening;
				}
				else
				{
                    TurnOn();
                    currentOpenState = OpenState.Open;
				}
            }
        }

        public virtual void CloseRootCanvas()
        {
            if (!isInTransition)
            {
                if (animationComponent != null && clip_CloseMenu != null)
                {
                    animationComponent.Play(clip_CloseMenu.name);
                    currentOpenState = OpenState.Closing;
                }
                else
                {
                    TurnOff();
                    currentOpenState = OpenState.Closed;
                }
            }
        }

        public virtual void SetSubcanvasActive(int index, bool status)
        {
            Debug.Log($"Set {gameObject.name}'s Subcanvas at index {index} to {(status ? new string("Active") : new string("Inactive"))}. Current Index is {currentSubCanvasIndex}");
            if (isValidIndex(index, true))
            {
                if (status)
                {
                    if (index == currentSubCanvasIndex || currentSubCanvas == null)
                    {
                        Debug.Log($"Set the subcanvas to active. Either new index is same as current ({index == currentSubCanvasIndex}) or current subcvanvas is null ({currentSubCanvas == null})\nIs the current sibcanvasindex invalid?");
                        subCanvasManagers[index].TurnOn();
                    }
                    else
                    {
                        Debug.Log($"Swap to new subcanvas");
                        if (swappingCoroutine != null) StopCoroutine(swappingCoroutine);
                        swappingCoroutine = StartCoroutine(SwapToSubCanvas(index));
                    }
                }
                else
                {
                    subCanvasManagers[index].TurnOff();
                    if (allowNone)
                    {
                        currentSubCanvas?.TurnOff(new System.Action(TurnOff));
                        //animationComponent.PlayQueued(clip_CloseMenu?.name);
                    }
                    else
                    {
                        currentSubCanvas?.TurnOff();
                    }
                }
            }
        }

        protected IEnumerator SwapToSubCanvas(int newIndex)
        {
            Debug.Log($"Swap to new subcanvas {newIndex} from {currentSubCanvasIndex}");
            if (isValidIndex(newIndex))
            {
                currentSubCanvas.TurnOff();
                while (currentSubCanvas.currentOpenState != OpenState.Closed)
                {
                    yield return new WaitForEndOfFrame();
                }
                currentSubCanvasIndex = newIndex;
                currentSubCanvas.TurnOn();
            }
            yield return new WaitForEndOfFrame();
            swappingCoroutine = null;
        }

        public void NextPage()
        {
            IterateSubCanvas(1);
        }

        public void PreviousPage()
        {
            IterateSubCanvas(-1);
        }

        public void IterateSubCanvas(int increment)
        {
            int newIndex = currentSubCanvasIndex + increment;
            if (canWrap) newIndex = (int)Mathf.Repeat(newIndex, subCanvasManagers.Count - 1);
            SetSubcanvasActive(newIndex, true);
        }

        protected virtual void InvokePostTransition(string method)
        {
            if (postTransitionInvokeQueue.Count == 0 || postTransitionInvokeQueue[postTransitionInvokeQueue.Count - 1] != method)
            {
                postTransitionInvokeQueue.Add(method);
            }

            if (delayedInvoker == null)
            {
                delayedInvoker = StartCoroutine(InvokeAfterTransition(0f));
            }
        }

        protected virtual IEnumerator InvokeAfterTransition(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (isInTransition)
            {
                yield return new WaitForEndOfFrame();
            }

            for(int c = postTransitionInvokeQueue.Count-1; c > 0; c--)
			{
                if (string.IsNullOrWhiteSpace(postTransitionInvokeQueue[c])) postTransitionInvokeQueue.RemoveAt(c);
			}

            if (postTransitionInvokeQueue.Count > 0)
            {
                Invoke(postTransitionInvokeQueue[0], 0f);
                postTransitionInvokeQueue.RemoveAt(0);
            }

            if (postTransitionInvokeQueue.Count > 0)
            {
                delayedInvoker = StartCoroutine(InvokeAfterTransition(0.25f));
            }
            else
            {
                delayedInvoker = null;
            }
        }

        public void ClearAll(bool resetIndex = false)
		{
            for(int c = 0; c < subCanvasManagers.Count; c++)
			{
                SetSubcanvasActive(c, false);
			}
            if (resetIndex) currentSubCanvasIndex = -1;
		}
        
#if UseNA
        public DropdownList<int> GetPanelList()
		{
            DropdownList<int> panelList = new NaughtyAttributes.DropdownList<int>();
            for(int c = 0; c < subCanvasManagers.Count; c++)
			{
                panelList.Add($"({c}) {subCanvasManagers[c].name}", c);
			}
            return panelList;
		}
#endif
    }
}
