using System.Collections;
using UnityEngine;
#if UseNA
using NaughtyAttributes;
#endif

namespace AltEnding.Dialog
{
    public class RenderTexturePrefab : MonoBehaviour
    {
#if UseNA
        [Required]
#endif
        [SerializeField]
        private Camera myCamera;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField]
        private RenderTexture myRenderTexture;
#if UseNA
        [ReadOnly]
#endif
        [SerializeField]
        private Vector2Int size;
#if UseNA
        [OnValueChanged(nameof(SetSize))]
#endif
        [SerializeField, Range(2,12)]
        private int width;
#if UseNA
        [OnValueChanged(nameof(SetSize))]
#endif
        [SerializeField, Range(2, 12)]
        private int height;

        private System.Action<RenderTexture> renderTextureCallback;
        private Coroutine initializationCoroutine;

        private void Reset()
        {
            if(myCamera == null && !TryGetComponent<Camera>(out myCamera))
            {
                myCamera = GetComponentInChildren<Camera>();
                if (myCamera == null)
                {
                    myCamera = new Camera();
                    myCamera.transform.SetParent(transform);
#if UNITY_EDITOR
                    UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(myCamera);
#endif
                }
#if UNITY_EDITOR
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }
            width = 8;
            height = 8;
            SetSize();
        }

        private void OnEnable()
        {
            if (myCamera == null && !TryGetComponent<Camera>(out myCamera)) {
                gameObject.SetActive(false);
                return;
            }
            SetSize();
        }

        private void InitializeRenderTexture()
        {
            if (initializationCoroutine != null) return;

            initializationCoroutine = StartCoroutine(CreateRenderTexture());
        }

        private IEnumerator CreateRenderTexture()
        {
            if (myCamera == null)
            {
                initializationCoroutine = null;
                yield break;
            }
            if (myRenderTexture == null)
            {
                myRenderTexture = new RenderTexture(size.x, size.y, 0);
            }
            yield return null;
            myRenderTexture.Create();
            myCamera.targetTexture = myRenderTexture;
            yield return null;
            renderTextureCallback?.Invoke(myRenderTexture);
            initializationCoroutine = null;
        }

        public void GetRenderTexture(System.Action<RenderTexture> callback)
        {
            if(myRenderTexture != null && myRenderTexture.IsCreated() && myCamera != null && myCamera.targetTexture == myRenderTexture)
            {
                callback.Invoke(myRenderTexture);
                return;
            }

            renderTextureCallback += callback;
            InitializeRenderTexture();
        }

        private void SetSize()
        {
            if (size == null) size = new Vector2Int();
            size.x = (int)Mathf.Pow(2, width);
            size.y = (int)Mathf.Pow(2, height);
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        public void SetRenderTexture(RenderTexture newRT)
        {
            myRenderTexture = newRT;
            myCamera.targetTexture = newRT;
        }

        private void OnDestroy()
        {
            if(myRenderTexture != null)
            {
                myRenderTexture.Release();
                Destroy(myRenderTexture);
            }
        }
    }
}
