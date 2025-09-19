using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif
using AltEnding.GUI;
using AltEnding.SaveSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AltEnding.CheckpointMap
{
    public class CheckpointMapUIController : GenericCanvasManager
	{
        enum UpdateStyle
		{
            DoNotUpdate = 0,
            WhenTurnedOn = 1,
            LiveUpdate = 2
		}

        #region Instance Management
        public static CheckpointMapUIController Instance { get; private set; }

        private static event Action instanceInitializedAction;

        public static void WhenInitialized(Action executeWhenInitialized)
        {
            if (Instance != null && !applicationIsQuitting)
            {
                executeWhenInitialized?.Invoke();
            }
            else
            {
                instanceInitializedAction += executeWhenInitialized;
            }
        }

        private static bool applicationIsQuitting = false;
        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
        #endregion
        
        [SerializeField] protected bool isInMainMenu;

        [Header("Map")]
        [SerializeField] private UpdateStyle updateStyle;
        [SerializeField] private List<CheckpointMapNode> nodes;
        public ToggleGroup nodesToggleGroup;
        [SerializeField] private RectTransform nodeListParent;
        [SerializeField] protected CheckpointMapNode selectedNode;
        
        [field: SerializeField
#if UseNA
        ,ReadOnly
#endif
        ] public bool isOnMapNode { get; protected set; }

        [Header("Description")]
        public TMP_Text descriptionLabel;
        public TMP_Text locationLabel;
        public Image descriptionForegroundImage;
        [SerializeField]
        private AspectRatioFitter foregroundRatioFitter;
        public Image descriptionBackgroundImage;
        [SerializeField]
        private AspectRatioFitter backgroundRatioFitter;
        [SerializeField] protected OpenState detailPanelState;
        [SerializeField] protected Animation detailsPanelAnimator;
        [SerializeField] protected AnimationClip detailsClip_Open;
        [SerializeField] protected AnimationClip detailsClip_Close;
        [SerializeField, Min(0f)] protected float closeDetailsDelayLength;
        protected Coroutine detailsAnimating;
        protected Coroutine closeDetailsDelay;

        [Header("Map Scrolling")]
        public ScrollRect myScrollRect;
        [Range(0, 1), Tooltip("0 = Left, 1 = Right")]
        public float startingScrollAmount = 1;


        private AsyncOperationHandle<IList<IResourceLocation>> m_lOCLocationsOpHandle;
        private IList<IResourceLocation> lOCLocations;
        private AsyncOperationHandle<Location> _lOCLoadOpHandle;

        private void OnEnable()
        {
            CheckpointMapNode.CheckpointMapNodeSelected += CheckpointMapNode_CheckpointMapNodeSelected;
            ArticyFlowHistoryTracker.NewCheckpointLogged += ArticyFlowHistoryTracker_NewCheckpointLogged;
        }

        private void OnDisable()
        {
            CheckpointMapNode.CheckpointMapNodeSelected -= CheckpointMapNode_CheckpointMapNodeSelected;
            ArticyFlowHistoryTracker.NewCheckpointLogged -= ArticyFlowHistoryTracker_NewCheckpointLogged;
            if (_lOCLoadOpHandle.IsValid()) _lOCLoadOpHandle.Completed -= OnLOCLoadComplete;
            if (m_lOCLocationsOpHandle.IsValid()) m_lOCLocationsOpHandle.Completed -= M_lOCLocationsOpHandle_Completed;
        }

        #region LocationAddressLoading
        private void CheckLocationAddress(string address)
        {
            Debug.Log($"Check Location Address\nAddress: {address}");
            m_lOCLocationsOpHandle = Addressables.LoadResourceLocationsAsync(address);
            m_lOCLocationsOpHandle.Completed += M_lOCLocationsOpHandle_Completed;
        }

        private void M_lOCLocationsOpHandle_Completed(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            Debug.Log($"Locations check completed. \nValid: {obj.IsValid()}. \nStatus: {obj.Status.ToString()}");
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                lOCLocations = obj.Result;
                Debug.Log($"Locations check Succeeded. \nLength: {lOCLocations.Count}");
                if (lOCLocations.Count > 0)
                {
                    _lOCLoadOpHandle = Addressables.LoadAssetAsync<Location>(lOCLocations[0]);
                    _lOCLoadOpHandle.Completed += OnLOCLoadComplete;
                }
                else
                {
                    Debug.LogWarning($"Failed to load LOC from addressables.  No resource locations found.");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load LOC from addressables.  Get Resource location operation failed.\n{obj.OperationException.Message}");
            }
        }

        private void OnLOCLoadComplete(AsyncOperationHandle<Location> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                UpdateDetailsBackground(obj.Result.backgroundIcon);
            }
            else
            {
                Debug.LogWarning($"Failed to load LOC from addressables.  Load resource from location failed.\n{obj.OperationException.Message}");
            }
        }
        #endregion

        private void CheckpointMapNode_CheckpointMapNodeSelected(CheckpointMapNode node)
        {
            selectedNode = node;
            if (nodesToggleGroup != null) nodesToggleGroup.allowSwitchOff = false;
            UpdateDetailsPanel(node);
        }

        private void ArticyFlowHistoryTracker_NewCheckpointLogged(MapNodeData newData)
		{
            if (updateStyle != UpdateStyle.LiveUpdate || newData == null) return;
            for(int c = 0; c < nodes.Count; c++)
            {
                if(nodes[c].ArticyObjectSet() && nodes[c].articyObject.id == newData.articyObjectID)
                {
                    nodes[c].UpdateVisualStateFromArticyObject();
                }
            }
		}

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Found more than one BranchMapController in the scene. Destroying the newest one.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (nodes == null) nodes = new List<CheckpointMapNode>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        protected override void Start()
        {
            base.Start();
            if (myScrollRect != null) myScrollRect.horizontalNormalizedPosition = startingScrollAmount;
            HideBranchMap();
            if(updateStyle == UpdateStyle.LiveUpdate) SafeUpdateFromHistoryTracker();
        }

        public override void TurnOn()
        {
            UpdateVisuals();
            base.TurnOn();
            if(selectedNode == null) 
                SelectByIndex(0);
        }

        public void HideBranchMap()
        {
            TurnOff();
        }

		[ContextMenu("Update Visuals")]
		protected void UpdateVisuals()
		{
			//Reset carryover data
			selectedNode = null;
			descriptionLabel?.SetText("");
			locationLabel?.SetText("");

            if (updateStyle == UpdateStyle.WhenTurnedOn && (ArticyFlowHistoryTracker.instanceInitialized && nodes.Count != ArticyFlowHistoryTracker.Instance.MapNodeHistory.Count))
            {
                SafeUpdateFromHistoryTracker();
            }
		}

        public void SafeUpdateFromHistoryTracker()
        {
            if (cr_UpdateFromHistoryTracker != null) StopCoroutine(cr_UpdateFromHistoryTracker);
            cr_UpdateFromHistoryTracker = StartCoroutine(UpdateFromHistoryTracker());
        }

        private Coroutine cr_UpdateFromHistoryTracker;

        protected IEnumerator UpdateFromHistoryTracker()
		{
            while (!ArticyFlowHistoryTracker.instanceInitialized)
            {
                yield return new WaitForEndOfFrame();
            }
            if (ArticyFlowHistoryTracker.Instance.MapNodeHistory != null)
            {
                if(nodes == null || nodes.Count == 0)
                {
                    nodes = GetComponentsInChildren<CheckpointMapNode>().ToList<CheckpointMapNode>();
                }
                foreach(CheckpointMapNode node in nodes)
                {
                    node.UpdateVisualStateFromArticyObject();
                }
            }

            cr_UpdateFromHistoryTracker = null;
        }

        public bool RegisterNode(CheckpointMapNode newNode)
        {
            if (nodes == null) nodes = new List<CheckpointMapNode>();
            if (nodes.Contains(newNode)) return false;
            nodes.Add(newNode);
            if (nodesToggleGroup != null) newNode.myToggle.group = nodesToggleGroup;
            return true;
        }

        public bool DeregisterNode(CheckpointMapNode node)
        {
            if(nodes == null)
            {
                nodes = new List<CheckpointMapNode>();
                return false;
            }
            if (!nodes.Contains(node)) return false;
            for(int c = nodes.Count-1; c >= 0; c--)
            {
                if(node == nodes[c])
                {
                    nodes.RemoveAt(c);
                }
            }
            return true;
        } 

        #region Details panel
        public void UpdateDetailsPanel(CheckpointMapNode node)
		{
            if(node != null)
			{
                if (descriptionLabel != null) descriptionLabel.text = node.myDescription;
                if (locationLabel != null)
                {
                    string localizedLocation = node.myLocationSceneName;
                    locationLabel.text = localizedLocation; 
                }
                if (descriptionForegroundImage != null)
                {
                    descriptionForegroundImage.sprite = node.myMapNodeData?.foregroundSprite;
                    descriptionForegroundImage.enabled = !(descriptionForegroundImage.sprite == null);
                }
                UpdateDetailsBackground(null);
                CheckLocationAddress($"{node.myMapNodeData.backgroundArticyHexID}{Location._addressableSuffix}");
                OpenDetailsPanel();
			}
			else
			{
                if (descriptionLabel != null) descriptionLabel.text = "";
                if (locationLabel != null) locationLabel.text = "";
                if (descriptionForegroundImage != null) descriptionForegroundImage.enabled = false;
                if (descriptionBackgroundImage != null) descriptionBackgroundImage.enabled = false;
                CloseDetailsPanel();
            }
		}

        public void OpenDetailsPanel()
        {
            OpenDetailsPanel(true);
        }

        public void CloseDetailsPanel()
        {

            switch (detailPanelState)
            {
                case OpenState.Open:
                case OpenState.Opening:
                    if (closeDetailsDelay != null) StopCoroutine(closeDetailsDelay);
                    StartCoroutine(CloseDetailsDelayCoroutine());
                    return;
                case OpenState.Closed:
                case OpenState.Closing:
                default:
                    return;
            }
        }

        protected IEnumerator CloseDetailsDelayCoroutine()
        {
            yield return new WaitForSeconds(closeDetailsDelayLength);
            //If we haven't cut this coroutine short yet, then go ahead and close. It should be cut short anytime the panel is told to "open".
            if (detailsAnimating != null) StopCoroutine(detailsAnimating);
            detailsAnimating = StartCoroutine(PlayDetailsTransitionAnimation(false, true));
            closeDetailsDelay = null;
        }

        public void OpenDetailsPanel(bool interupt)
        {
            if (closeDetailsDelay != null) StopCoroutine(closeDetailsDelay);

            switch (detailPanelState)
            {
                case OpenState.Open:
                case OpenState.Opening:
                    return;
                case OpenState.Closed:
                    if (detailsAnimating != null) StopCoroutine(detailsAnimating);
                    detailsAnimating = StartCoroutine(PlayDetailsTransitionAnimation(true, interupt));
                    return;
                case OpenState.Closing:
                    if (detailsAnimating != null) StopCoroutine(detailsAnimating);
                    detailsAnimating = StartCoroutine(PlayDetailsTransitionAnimation(true, true));
                    return;
                default:
                    return;
            }
        }

        protected IEnumerator PlayDetailsTransitionAnimation(bool open, bool interupt)
        {
            if(detailsPanelAnimator == null || (open ? detailsClip_Open == null : detailsClip_Close == null))
            {
                detailsAnimating = null;
                yield break;
            }
            detailPanelState = open ? OpenState.Opening : OpenState.Closing;
            if (interupt)
            {
                detailsPanelAnimator.Play(open ? detailsClip_Open.name : detailsClip_Close.name);
            }
            else
            {
                detailsPanelAnimator.PlayQueued(open ? detailsClip_Open.name : detailsClip_Close.name);
            }
            while (detailsPanelAnimator.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            detailPanelState = open ? OpenState.Open : OpenState.Closed;
            detailsAnimating = null;
        }

        public void UpdateDetailsBackground(Sprite newBGImage)
        {
            if (descriptionBackgroundImage == null || backgroundRatioFitter == null) return;

            descriptionBackgroundImage.sprite = newBGImage;

            if(newBGImage == null)
            {
                descriptionBackgroundImage.enabled = false;
            }
            else
            {
                backgroundRatioFitter.aspectRatio = newBGImage.rect.size.AspectRatio();
                descriptionBackgroundImage.enabled = true;
                //Also add some sort of transition animation
            }
        }
#endregion

        public void SelectByIndex(int index)
        {
            if(doDebugs) Debug.Log($"CheckpointMapUIController.SelectByIndex(index = {index})");
            if (nodes == null || index >= nodes.Count || nodes[index] == null) return;

            nodes[index].ToggleSetExternally(true, true);
        }

        public void LoadSelectedNodeScene()
        {
            if (selectedNode != null)
            {
                if (isInMainMenu)
                {
                    //We need to tell the SceneManagementSingleton to load the dialog scene to the current node
                    SceneManagementSingleton.instance.PlayFromMapNode(selectedNode.articyObjectId);
                }
                else
                {

                    //Load from within the dialog scene
                    SaveManager.Instance.LoadGame(selectedNode.articyObjectId);

                    foreach (CheckpointMapNode node in nodes)
                    {
                        if (node.articyObject.id == selectedNode.articyObject.id)
                        {
                            node.ToggleSetExternally(true);
                            break;
                        }
                    }
                    
                    HideBranchMap();
                }
            }
        }
    }
}
