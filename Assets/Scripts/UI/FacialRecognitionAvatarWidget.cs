using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class AvatarIdentifiedMessage
    {
        
    }

    public class FacialRecognitionAvatarWidget : MonoBehaviour
    {
        [SerializeField] private Image _avatarCompleteImage;
        [SerializeField] private Image _avatarFuzzyImage;
        [SerializeField] private Image _unidentifiedImageEmpty;
        [SerializeField] private Image _unidentifiedImageFilled;

        
        private float _progress = 0f;
        // top and bottom of unidentified image are border, so cut off this much of the image from the fill effect
        private const float _unidentifiedImagePadding = .1f;
        private const float _fillMin = 0f + _unidentifiedImagePadding;
        private const float _fillMax = 1f - _unidentifiedImagePadding;

        private const float _unidentifiedImageTotalFillSeconds = 1.8f;
        private const float _finalImageFillSeconds = 1.4f;

        public void Start()
        {
            _unidentifiedImageFilled.fillAmount = 0f;
            _avatarCompleteImage.fillAmount = 0f;

            _avatarCompleteImage.enabled = false;
            _avatarFuzzyImage.enabled = false;
        }

        public void ShowProgress(float newProgressAmount = 0)
        {
            var oldProgress = _progress;
            _progress = Mathf.Min(1f, _progress + newProgressAmount);
            
            var fillGoal = Mathf.Lerp(_fillMin, _fillMax, _progress);
            DOTween.To(
                    () => _unidentifiedImageFilled.fillAmount,
                    fill => _unidentifiedImageFilled.fillAmount = fill,
                    fillGoal, _unidentifiedImageTotalFillSeconds * newProgressAmount)
                .OnComplete(() =>
                {
                    if (_progress >= 1f)
                        ShowComplete();
                });
        }

        private void ShowComplete()
        {
            _unidentifiedImageEmpty.enabled = _unidentifiedImageFilled.enabled = false;
            _avatarCompleteImage.enabled = _avatarFuzzyImage.enabled = true;
            _avatarCompleteImage.fillAmount = 0f;
            
            DOTween.To(
                () => _avatarCompleteImage.fillAmount, 
                fill => _avatarCompleteImage.fillAmount = fill, 
                1, _finalImageFillSeconds)
                .OnComplete(() => { EventBetter.Raise(new AvatarIdentifiedMessage()); });
        }
    }
}