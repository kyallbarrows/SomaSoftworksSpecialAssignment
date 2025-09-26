using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace SpecialAssignment
{
    [RequireComponent(typeof(RectTransform))]
    public class AudioFile : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private string filename;

        [SerializeField]
        private string group;
        public string Group => group;

        [SerializeField] private bool isLeftColumn;
        public bool IsLeftColumn => isLeftColumn;

        [SerializeField]
        private TextMeshProUGUI filenameTF;

        private bool _snapping = false;
        private Vector2 _destination;
        private Vector2 _homePos; 

        private AudioToolDragManager _manager = null;

        private Vector2 _centerPoint;
        private Vector2 _worldCenterPoint => transform.TransformPoint(_centerPoint);

        private void Awake()
        {
            _manager = GetComponentInParent<AudioToolDragManager>();
            _centerPoint = (transform as RectTransform).rect.center;

            filenameTF.text = filename;

            _homePos = this.transform.localPosition;
        }

        public void GoHome()
        {
            GoTo(_homePos);
        }

        public void GoTo(Vector2 destination)
        {
            _snapping = true;
            _destination = destination;
        }

        private void Update(){
            if (_snapping)
            {
                // go a 1/4 of the way home
                transform.localPosition = (_destination + 20 * (Vector2)transform.localPosition) / 21f;
                
                if ((_destination - (Vector2)this.transform.localPosition).sqrMagnitude < .5f)
                {
                    transform.localPosition = _destination;
                    _snapping = false;
                }
                
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if ((Vector2)transform.localPosition != _homePos)
                GoHome();
                
            _manager.RegisterDraggedObject(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_manager.IsWithinBounds(_worldCenterPoint + eventData.delta))
            {
                transform.Translate(eventData.delta);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _manager.UnregisterDraggedObject(this);
            if (_manager.SnapToTarget(this))
            {
                Debug.Log("It's in the target area");
            }
        }
    }
}
