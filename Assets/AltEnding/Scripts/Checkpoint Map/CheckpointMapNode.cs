using Articy.Unity;
using Articy.Unity.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.CheckpointMap
{
    public class CheckpointMapNode : CheckpointMapPoint
    {
        public static Action<CheckpointMapNode> CheckpointMapNodeSelected;
        
        /// <summary>
        /// Should only point to an articy object that contains checkpoint feature data
        /// </summary>
#if UseNA
        [OnValueChanged(nameof(UpdateDataFromArticyObject)), ValidateInput(nameof(ArticyObjectSet), "Required Field")]
#endif
		public ArticyRef articyObject;
		public string articyObjectId { get { return articyObject != null ? articyObject.id.ToHex() : null; } }
		public bool ArticyObjectSet() => articyObject != null && articyObject.HasReference;


		[field: SerializeField
#if UseNA
        ,AllowNesting
#endif
        ] public MapNodeData myMapNodeData { get; private set; }
        public Toggle myToggle;
#if UseNA
        [Required]
#endif
        [SerializeField]
        private Image myImage;

        [field: SerializeField
#if UseNA
        ,ShowAssetPreview
#endif
        ]
        public Sprite myForegroundImage { get; protected set; }
        [TextArea] public string myDescription;
        public string myLocationSceneName;

        public bool sizeSet = false;

        private void OnEnable()
        {
            RegisterNodeWithUIController();
        }

        private void OnDisable()
        {
            DeRegisterNodeWithUIController();
        }

        private void RegisterNodeWithUIController()
        {
            if (CheckpointMapUIController.Instance == null)
            {
                CheckpointMapUIController.WhenInitialized(RegisterNodeWithUIController);
                return;
            }
            CheckpointMapUIController.Instance.RegisterNode(this);
        }

        private void DeRegisterNodeWithUIController()
        {
            if (CheckpointMapUIController.Instance == null)
            {
                CheckpointMapUIController.WhenInitialized(DeRegisterNodeWithUIController);
                return;
            }
            CheckpointMapUIController.Instance.DeregisterNode(this);
        }

        private void Start()
		{
            UpdateVisualStateFromArticyObject();
		}

        void SetInteractableState()
        {
            if (myToggle != null) myToggle.interactable = currentState == VisitedState.VisitedInPlaythrough;
        }

		public void ToggleSetExternally(bool isOn, bool notify = false)
        {
            if (myToggle == null) return;
            if (notify)
            {
                myToggle.isOn = isOn;
            }
            else
            {
                myToggle.SetIsOnWithoutNotify(isOn);
            }
        }

        public void OnValueChanged(bool isOn)
        {
            if (isOn)
            {
                CheckpointMapNodeSelected?.Invoke(this);
            }
        }

        public void Setup(MapNodeData newData, ToggleGroup group)
		{
			NewNodeData(newData);
			if (!EvaluateVisitedState(articyObjectId, ref currentState))
			{
				currentState = VisitedState.NotVisited;
			}
			SetVisitedVisuals();
			if (myToggle != null) myToggle.group = group;
		}

		public void NewNodeData(MapNodeData newData)
		{
            if (newData == null) return;
            myMapNodeData = newData;
            articyObject = (ArticyRef) ArticyDatabase.GetObject(newData.articyObjectID);
            myDescription = newData.description;
            myLocationSceneName = newData.locationSceneName;

            if (myImage == null) return;

            if (myMapNodeData.FindForegroundSprite())
            {
                myImage.sprite = myMapNodeData.foregroundSprite;
            }
		}

        [ContextMenu("Update Data From Articy Object")]
        protected void UpdateDataFromArticyObject()
        {
            //If null do nothing
            if (articyObject == null) return;
            //If it's the same then don't worry about it
            if (myMapNodeData?.articyObjectID == articyObject.id) return;
            
            ArticyObject aObject = ArticyDatabase.GetObject(articyObject.id);
            var checkpointFeature = ArticyStoryHelper.Instance.GetCheckpointFeature(aObject);
            myMapNodeData = new MapNodeData(articyObject.id, checkpointFeature);

            NewNodeData(myMapNodeData);
        }

        public void UpdateVisualStateFromArticyObject()
        {
            EvaluateVisitedState(articyObjectId);
            SetVisitedVisuals();
            SetInteractableState();
        }
	}
}
