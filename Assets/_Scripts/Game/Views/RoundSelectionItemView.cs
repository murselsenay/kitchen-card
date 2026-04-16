using Game.Core.Events;
using Game.Models.Stages;
using Modules.Event.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class RoundSelectionItemView : MonoBehaviour
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private DifficultyStarsView _difficultyStarsView;
        [SerializeField] private TMP_Text _difficultyNameText;
        [SerializeField] private TMP_Text _targetScoreText;

        private RoundSelectionData _data;

        public void Init(RoundSelectionData data)
        {
            _data = data;

            if (_difficultyNameText != null)
                _difficultyNameText.text = data.DifficultyName;

            if (_targetScoreText != null)
                _targetScoreText.text = data.TargetScore.ToString();

            if (_difficultyStarsView != null)
                _difficultyStarsView.SetStarCount(data.DifficultyStarCount);

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnSelectButtonClicked);
            }
        }

        private void OnSelectButtonClicked()
        {
            if (_data == null || string.IsNullOrEmpty(_data.RoundId)) return;

            EventManager.Delegate(new RoundSelectedEvent(_data.RoundId));
        }
    }
}