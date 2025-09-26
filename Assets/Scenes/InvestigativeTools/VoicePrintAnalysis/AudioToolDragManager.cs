using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SpecialAssignment
{
    public class AudioToolDragManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _evidenceAmountTF;
        private int _evidenceAmount = 0;

        [SerializeField] private GameObject _matchIndicator;
        [SerializeField] private GameObject _noMatchIndicator;
        
        private AudioFile _leftFile;
        private AudioFile _rightFile;
        
        [SerializeField]
        private RectTransform _defaultLayer = null;

        [SerializeField]
        private RectTransform _dragLayer = null;

        [SerializeField] private RectTransform _leftDropArea;
        [SerializeField] private RectTransform _rightDropArea;

        private Rect _boundingBox;

        private AudioFile _currentDraggedObject = null;
        public AudioFile CurrentDraggedObject => _currentDraggedObject;

        private void Awake()
        {
            _matchIndicator.SetActive(false);
            _noMatchIndicator.SetActive(false);
            SetBoundingBoxRect(_dragLayer);
            
            _evidenceAmountTF.text = $"Evidence: 0%";
        }

        public void RegisterDraggedObject(AudioFile drag)
        {
            _currentDraggedObject = drag;
            drag.transform.SetParent(_dragLayer);
        }

        public void UnregisterDraggedObject(AudioFile drag)
        {
            drag.transform.SetParent(_defaultLayer);
            _currentDraggedObject = null;
        }

        public bool IsWithinBounds(Vector2 position)
        {
            return _boundingBox.Contains(position);
        }

        private void SetBoundingBoxRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var position = corners[0];

            Vector2 size = new Vector2(
                rectTransform.lossyScale.x * rectTransform.rect.size.x,
                rectTransform.lossyScale.y * rectTransform.rect.size.y);

            _boundingBox = new Rect(position, size);
        }

        public bool SnapToTarget(AudioFile file)
        {
            Debug.Log("Left area " + _leftDropArea.rect);
            Debug.Log("Local pos " + file.transform.localPosition);

            if (file.IsLeftColumn &&
                (_leftDropArea.transform.localPosition - file.transform.localPosition).magnitude <= 120f)
            {
                Debug.Log("In left target area, going to ");
                file.GoTo(_leftDropArea.transform.localPosition);
                _leftFile?.GoHome();
                _leftFile = file;

                CheckForMatches();
                return true;
            }

            if (!file.IsLeftColumn && 
                (_rightDropArea.transform.localPosition - file.transform.localPosition).magnitude <= 120f)
            {
                file.GoTo(_rightDropArea.transform.localPosition);
                _rightFile?.GoHome();
                _rightFile = file;

                CheckForMatches();
                return true;
            }

            file.GoHome();
            return false;
        }

        private void CheckForMatches()
        {
            if (_leftFile == null || _rightFile == null)
                return;

            if (_leftFile.Group == _rightFile.Group)
            {
                _evidenceAmount += Random.Range(5, 21);
                _evidenceAmount = Mathf.RoundToInt(Mathf.Min(_evidenceAmount, 100f));
                _evidenceAmountTF.text = $"Evidence: {_evidenceAmount}%";
                _matchIndicator.SetActive(true);
                
                UniTask.Delay(2000).ContinueWith(() =>
                {
                    _matchIndicator.SetActive(false);
                    _leftFile.GoHome();
                    _rightFile.GoHome();
                    
                    _leftFile = null;
                    _rightFile = null;
                });
            }
            else
            {
                _noMatchIndicator.SetActive(true);
                UniTask.Delay(1200).ContinueWith(() =>
                {
                    _noMatchIndicator.SetActive(false);
                });
            }
        }
    }
}
