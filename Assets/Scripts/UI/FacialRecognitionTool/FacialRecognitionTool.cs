using UnityEngine;

namespace SpecialAssignment
{
    public class FacialRecognitionTool : MonoBehaviour
    {
        [SerializeField] private GameObject _partOne;
        [SerializeField] private GameObject _partTwo;

        public void Start()
        {
            _partOne.SetActive(true);
            _partTwo.SetActive(false);
            
            EventBetter.Listen(this, (SwitchToFacialRecognitionPartTwoMessage msg) =>
            {
                _partOne.SetActive(false);
                _partTwo.SetActive(true);
            });
        }
    }
}