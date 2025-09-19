using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding
{
    public class ScenePanning_ScrollRects : ScenePanningBase
    {
        [SerializeField]
        [ContextMenuItem("Sort Scroll Rects", nameof(CleanSortScrollRects))]
        private List<ScrollRect> scrollRects;

        [SerializeField, Tooltip("X is sort order (Back->Front)\nY is Width, normalized from largest to smallest")]
        [ContextMenuItem("Refresh Size Info", nameof(CleanGetSizeInfoFromRects))]
        [ContextMenuItem("Flip Curve", nameof(FlipSizeCurve))]
#if UseNA
        [OnValueChanged(nameof(SetSizesFromCurve))]
#endif
        private AnimationCurve sizeCurve = AnimationCurve.Linear(0f,0f,1f,1f);

#if UseNA
        [MinMaxSlider(1000f, 6000f), OnValueChanged(nameof(SetSizesFromCurve))]
#endif
        [SerializeField]
        [ContextMenuItem("Refresh Size Info", nameof(CleanGetSizeInfoFromRects))]
        private Vector2 sizes = new Vector2(2000f, 3000f);


        #region During Gameplay

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeToPanningEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnsubscribeToPanningEvents();
        }

        private void SubscribeToPanningEvents()
        {
            if (!ValidateScrollRectsList()) return;

            foreach(ScrollRect scrollRect in scrollRects)
            {
                scrollRect.onValueChanged.AddListener(RecieveScrollWindowValue);
            }
        }

        private void UnsubscribeToPanningEvents()
        {
            if (!ValidateScrollRectsList()) return;

            foreach (ScrollRect scrollRect in scrollRects)
            {
                scrollRect.onValueChanged.RemoveListener(RecieveScrollWindowValue);
            }
        }

        private void RecieveScrollWindowValue(Vector2 newValue)
        {
            SetNewPosition(newValue.x);
        }

        protected override void Start()
        {
            base.Start();
            if(overlay != null && !overlay.gameObject.activeInHierarchy)
            {
                overlay.sprite = null;
                overlay.enabled = false;
            }
        }

        protected override void UpdatePosition()
        {
            if (!enabled) return;
            //if (!ValidateScrollRectsList())
            //{
            //    enabled = false;
            //    return;
            //}
            //base.UpdatePosition();
            for (int c = 0; c < scrollRects.Count; c++)
            {
                scrollRects[c].horizontalNormalizedPosition = panningPosition;
            }
        }

        #endregion

        #region During Setup/Editing

        protected bool showEditVariables => !Application.isPlaying;
        protected bool hideEditVariables => !showEditVariables;

        #region CanvasType
#if UNITY_EDITOR
#if UseNA
        [ReadOnly]
#endif
        [SerializeField]
        private List<Canvas> myCanvases;
        private Camera myCamera;
        private Vector2 canvasPlaneRange;
        private bool hasCanvas => myCanvases != null && myCanvases.Count > 0 && myCanvases[0] != null;
        private bool isCameraSpace => myRenderMode == RenderMode.ScreenSpaceCamera;

#if UseNA
        [ShowIf(EConditionOperator.And, nameof(hasCanvas), nameof(showEditVariables)), OnValueChanged(nameof(ChangeRenderMode)), ReadOnly]
#endif
        [SerializeField]
        private RenderMode myRenderMode;

#if UseNA
        [ShowIf(EConditionOperator.And, nameof(hasCanvas), nameof(showEditVariables), nameof(isCameraSpace)), OnValueChanged(nameof(SetCameraSpaceCanvasePlaneDistances))]
#endif
        [SerializeField]
        private bool cameraPlaneViaXAxis;

        [ContextMenu("Get Canvases")]
        private void GetCanvases()
        {
            if (scrollRects == null || scrollRects.Count < 1) return;
            
            myCanvases = new List<Canvas>();
            Canvas tempCanvas;
            for(int c = 0; c < scrollRects.Count; c++)
            {
                tempCanvas = null;
                if (scrollRects[c] != null) tempCanvas = scrollRects[c].GetCanvas();
                if (tempCanvas != null && !myCanvases.Contains(tempCanvas)) myCanvases.Add(tempCanvas);
            }
        }

        [ContextMenu("Get Canvas Camera")]
        private bool GetCamera()
        {
            if (!hasCanvas)
            {
                if (myCamera == null) myCamera = Camera.main;
            }
            else
            {
                myCamera = myCanvases[0].worldCamera;
            }
            GetCanvasPlaneRange();
            return myCamera != null;
        }

        private void GetCanvasPlaneRange()
        {
            if (myCamera == null) return;

            canvasPlaneRange.x = Mathf.Clamp(Mathf.FloorToInt(myCamera.nearClipPlane) + 10f, myCamera.nearClipPlane, Mathf.Lerp(myCamera.nearClipPlane, myCamera.farClipPlane, 0.5f));
            canvasPlaneRange.y = Mathf.Clamp(Mathf.CeilToInt(myCamera.farClipPlane) - 10f, Mathf.Lerp(myCamera.nearClipPlane, myCamera.farClipPlane, 0.5f), myCamera.farClipPlane);

        }

        private void ChangeRenderMode()
        {
            if (!hasCanvas) return;
            switch (myRenderMode)
            {
                case RenderMode.ScreenSpaceCamera:
                    return;
                case RenderMode.ScreenSpaceOverlay:
                    return;
                default:
                    return;
            }
        }

        private float CanvasPlane(float position)
        {
            if (myCamera == null && !GetCamera()) return 10f;

            position = Mathf.Clamp01(position);

            if (cameraPlaneViaXAxis)
            {
                return Mathf.Lerp(canvasPlaneRange.x, canvasPlaneRange.y, position);
            }
            else
            {
                return Mathf.Lerp(canvasPlaneRange.x, canvasPlaneRange.y, Mathf.Clamp01(sizeCurve.Evaluate(position)));
            }
        }

#endif
        private void SetCameraSpaceCanvasePlaneDistances()
        {

#if UNITY_EDITOR
            if (hasCanvas && myRenderMode == RenderMode.ScreenSpaceCamera && myCanvases.Count == scrollRects.Count)
            {
                for (int c = 0; c < myCanvases.Count; c++)
                {
                    myCanvases[c].planeDistance = CanvasPlane(1f - ((float)c / (float)(myCanvases.Count - 1)));
                }
            }
#endif
        }
        #endregion

#if UseNA
        [HideIf(EConditionOperator.Or, nameof(overlayAssigned), nameof(hideEditVariables))]
#endif
        [SerializeField]
        [Tooltip("Used as a template to align props during editing")]
        protected Image overlay;
        protected bool overlayAssigned => overlay != null;
        
#if UseNA
        [ShowIf(EConditionOperator.And, nameof(overlayAssigned), nameof(myLocationAssigned)), OnValueChanged(nameof(UpdateOverlayImageAlpha))]
#endif
        [SerializeField, Range(0f,1f)]
        protected float overlayAlpha;

        #region Location

#if UseNA
        [HideIf(nameof(hideEditVariables)), OnValueChanged(nameof(UpdateOverlayImage))]
#endif
        [SerializeField]
        [Tooltip("Used to help with editing")]
        protected UnityEngine.AddressableAssets.AssetReferenceT<Location> myLocation;
        protected bool myLocationAssigned => myLocation!.RuntimeKeyIsValid();

        protected bool GetOverlayReference()
        {
            if (overlay != null) return true;

            Image[] childImages = transform.GetComponentsInChildren<Image>();
            foreach(Image child in childImages)
            {
                if(string.Equals("Overlay", child.gameObject.name, System.StringComparison.OrdinalIgnoreCase)){
                    overlay = child;
                    break;
                }
            }
            if (overlay != null) overlayAlpha = overlay.color.a;
            return overlay != null;
        }

        protected void UpdateOverlayImage()
        {
            if (overlay == null) return;
            overlayAlpha = overlay.color.a;
#if UNITY_EDITOR
            if (myLocationAssigned)
            {
                UnityEditor.Undo.RecordObject(overlay, "Assign Overlay Image");
                overlay.sprite = myLocation.editorAsset.backgroundIcon;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
        }

        protected void UpdateOverlayImageAlpha()
        {
            if (!overlayAssigned) return;
            if(overlayAlpha <= 0f)
            {
                if(overlay.gameObject.activeSelf) overlay.gameObject.SetActive(false);
            }
            else
            {
                if (!overlay.gameObject.activeSelf) overlay.gameObject.SetActive(true);
                if (!overlay.enabled) overlay.enabled = true;
                Color tempColor = overlay.color;
                tempColor.a = overlayAlpha;
                overlay.color = tempColor;
            }

        }

        #endregion

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            int componentCount = gameObject.GetComponents<MonoBehaviour>().Length;
            for (int c = 0; c < componentCount; c++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
            }
#endif
            GetAllScrollRects();
            if (!GetOverlayReference())
            {
                overlay = new GameObject("Overlay").AddComponent<Image>();
                overlay.transform.SetParent(transform, false);
                overlay.rectTransform.anchorMin = Vector2.zero;
                overlay.rectTransform.anchorMax = Vector2.one;
                overlay.rectTransform.anchoredPosition = new Vector2(05f, 05f);
                overlay.rectTransform.sizeDelta = Vector2.zero;
                overlay.rectTransform.SetSiblingIndex(transform.childCount - 1);
            }
            UpdateOverlayImage();
        }

        private void OnValidate()
        {
            
        }

        private bool ValidateScrollRectsList()
        {
            scrollRects.CleanList();

            for (int c = scrollRects.Count-1; c >= 0; c--)
            {
                if(scrollRects[c].content == null)
                {
                    scrollRects[c].onValueChanged.RemoveListener(RecieveScrollWindowValue);
                    scrollRects.RemoveAt(c);
                }
            }

            return scrollRects.Count > 0;
        }

        private void CleanGetSizeInfoFromRects()
        {
            if (!ValidateScrollRectsList()) return;

            if (scrollRects.Count > 0) GetSizeInfoFromRects();
        }

        private void GetSizeInfoFromRects()
        {
            if (scrollRects.Count <= 0) return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Get Curve Info from rects");
#endif

            sizes.x = scrollRects[0].content.rect.width;
            sizes.y = scrollRects[scrollRects.Count - 1].content.rect.width;
            if (sizes.y < sizes.x)
            {
                sizes = new Vector2(sizes.y, sizes.x);
            }
            sizeCurve.ClearKeys();
            for(int c = 0; c < scrollRects.Count; c++)
            {
                sizeCurve.AddKey((float)c / (float)(scrollRects.Count-1), Mathf.InverseLerp(sizes.x, sizes.y, scrollRects[c].content.rect.width));
            }
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        private void SetSizesFromCurve()
        {
            if (!ValidateScrollRectsList()) return;

#if UNITY_EDITOR
            RectTransform[] contents = new RectTransform[scrollRects.Count];
            for (int c = 0; c < scrollRects.Count; c++)
            {
                contents[c] = scrollRects[c].content;
            }
            UnityEditor.Undo.RecordObjects(contents, "Set Sizes of windows from curve");
            UnityEditor.Undo.RecordObjects(myCanvases.ToArray(), "Set Sizes of windows from curve");
#endif
            for (int c = 0; c < scrollRects.Count; c++)
            {
                scrollRects[c].content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetSizeFromCurve(c));
#if UNITY_EDITOR
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(scrollRects[c].content);
#endif
            }
            SetCameraSpaceCanvasePlaneDistances();
        }

        private float GetSizeFromCurve(int listIndex)
        {
            if (scrollRects == null) return 0f;
            return GetSizeFromCurve((float)listIndex / (float)scrollRects.Count);
        }

        private float GetSizeFromCurve(float samplePoint)
        {
            if (sizeCurve == null) return 0f;
            return Mathf.Lerp(sizes.x, sizes.y, sizeCurve.Evaluate(samplePoint));
        }


        private void FlipSizeCurve()
        {
            if (sizeCurve == null) AnimationCurve.Linear(0f, 0f, 1f, 1f);

            if (sizeCurve.length < 2) return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Flip Curve");
#endif

            Keyframe[]  keyframeList = sizeCurve.keys;
            float curveLength = sizeCurve[sizeCurve.length - 1].time;

            string debugString = $"Flip Keyframe list:";
            for (int c = 0; c < keyframeList.Length; c++)
            {
                Keyframe sourceFrame = sizeCurve.keys[keyframeList.Length - 1 - c];
                
                if (doDebugText) 
                   debugString += $"\nConvert frame {c}({keyframeList[c].time}, {keyframeList[c].value}, {keyframeList[c].inTangent}, {keyframeList[c].outTangent}) \nto frame {keyframeList.Length - 1 - c}({sourceFrame.time}, {sourceFrame.value}, {sourceFrame.inTangent}, {sourceFrame.outTangent})";
                
                keyframeList[c] = new Keyframe(curveLength-sourceFrame.time, sourceFrame.value, -sourceFrame.inTangent, -sourceFrame.outTangent);
                
                if (doDebugText)
                    debugString += $"\nNew frame {c}: {keyframeList[c].time}, {keyframeList[c].value}, {keyframeList[c].inTangent}, {keyframeList[c].outTangent}";
                
            }

            sizeCurve.keys = keyframeList;
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif

            if (doDebugText)
            {
                debugString += "\nFinal List:";
                for (int c = 0; c < sizeCurve.length; c++)
                {
                    debugString += $"\nframe {c}({sizeCurve.keys[c].time}, {sizeCurve.keys[c].value}, {sizeCurve.keys[c].inTangent}, {sizeCurve.keys[c].outTangent})";
                }
                Debug.Log(debugString, this);
            }
            SetSizesFromCurve();
        }
        
#if UseNA
        [Button]
#endif
        private void GetAllScrollRects()
        {
            scrollRects = GetComponentsInChildren<ScrollRect>().ToList();
            if (scrollRects.Count <= 0) return;
            SortScrollRects();
            GetSizeInfoFromRects();
#if UNITY_EDITOR
            GetCanvases();
            if (hasCanvas) myRenderMode = myCanvases[0].renderMode;
#endif
        }

        private void CleanSortScrollRects()
        {
            scrollRects.CleanList();
            SortScrollRects();
        }

        /// <summary>
        /// Only call if you've already cleaned the list; Sorts the 'scrollRects' list by Sibling index.
        /// </summary>
        private void SortScrollRects()
        {
            string debugString = $"{gameObject.name} sorting {scrollRects.Count} Scroll Rects. Before (List Index/Sibling Index): ";
            if (doDebugText)
            {
                for(int c = 0; c < scrollRects.Count; c++)
                {
                    debugString += $"\n {c}/{scrollRects[c].transform.GetSiblingIndex()}";
                }
            }

            if (scrollRects.Count <= 1)
            {
                if (doDebugText) Debug.Log(debugString, this);
                return;
            }

            scrollRects.Sort(ListExtensions.CompareScrollRectsBySiblingIndex);
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif

            if (doDebugText)
            {
                debugString += "\nAfter:";
                for (int c = 0; c < scrollRects.Count; c++)
                {
                    debugString += $"\n {c}/{scrollRects[c].transform.GetSiblingIndex()}";
                }
                Debug.Log(debugString, this);
            }
        }

        #endregion

    }
}
