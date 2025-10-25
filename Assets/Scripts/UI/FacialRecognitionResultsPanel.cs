using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SpecialAssignment
{
    [System.Serializable]
    public class FilterConfigurationTarget
    {
        public int Group1Id;
        public int Group2Id;
        public int AvatarId;
        public float Progress;
    }

    public class FacialRecognitionResultsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _analyzePanel;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private Button _reconstructButton;
        [SerializeField] private List<FacialRecognitionAvatarWidget> _avatars;
        [SerializeField] private List<FilterConfigurationTarget> _filters;
        
        private List<string> _evaluatedFilters = new List<string>();
        
        private int _group1Id;
        private int _group2Id;

        public void Start()
        {
            _analyzePanel.SetActive(false);
            _reconstructButton.onClick.AddListener(() => ProcessSelectedFilters() );
            
            EventBetter.Listen(this, (FileAssetSelectedMessage msg) =>
            {
                if (msg.Group == 1) _group1Id = msg.Id;
                if (msg.Group == 2) _group2Id = msg.Id;
            });

            _progressSlider.value = 0;
        }
        
        private async void ProcessSelectedFilters()
        {
            // store these now in case they change it during the animation time
            var f1 = _group1Id;
            var f2 = _group2Id;

            var filterString = $"{f1}-{f2}";
            if (_evaluatedFilters.Contains(filterString))
                return;

            _evaluatedFilters.Add(filterString);

            _progressSlider.value = 0;
            _analyzePanel.SetActive(true);
            await DOTween.To(
                () => _progressSlider.value,
                (value) => _progressSlider.value = value,
                1, 1.5f).AsyncWaitForCompletion();
            _analyzePanel.SetActive(false);

            var matchingTargets = _filters.Where(
                f => f.Group1Id == f1 && f.Group2Id == f2).ToList();

            if (!matchingTargets.Any())
                return;

            // we might have multiple hits: a configuration might boost avatar 0 by .13f, and avatar 2 by .24f
            foreach (var target in matchingTargets)
            {
                _avatars[target.AvatarId].ShowProgress(target.Progress);
            }
        }

        public async void ShowAnalyzing()
        {
        }
    }
}