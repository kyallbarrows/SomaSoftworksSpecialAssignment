using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    public class SwitchToFacialRecognitionPartTwoMessage
    {
    }

    public class AnalysisCompletePopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _popupFrame;
        [SerializeField] private Button _continueButton;

        public void Start()
        {
            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);

            _continueButton.onClick.AddListener(() =>
            {
                EventBetter.Raise(new SwitchToFacialRecognitionPartTwoMessage());
            });
            
            EventBetter.Listen(this, (FileAssetSelectedMessage msg) =>
            {
                Show();
            });

        }

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _popupFrame.transform.localScale = new Vector3(.1f, .1f, 1f);
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x,
                    1, 0.1f));
            sequence.Append(_popupFrame.transform.DOScaleX(1f, .06f).SetEase(Ease.OutQuad));
            sequence.Append(_popupFrame.transform.DOScaleY(1f, .06f).SetEase(Ease.OutQuad));
        }

        public void Hide()
        {
            
        }
    }
}