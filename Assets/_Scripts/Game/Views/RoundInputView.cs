using Game.Controllers;
using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Recipes;
using Modules.Event.Managers;
using System.Collections.Generic;
using UnityEngine;
using Utilities.UI;

namespace Game.Views
{
    public class RoundInputView : MonoBehaviour
    {
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private CustomButton _playCardsButton;
        [SerializeField] private CustomButton _discardButton;
        [SerializeField] private CustomButton[] _recipeButtons = new CustomButton[6];

        private readonly List<RecipeScriptable> _currentRecipes = new List<RecipeScriptable>();
        private bool _hasVisibleHand;

        private void OnEnable()
        {
            BindButtons();
            EventManager.Subscribe<HandUpdatedEvent>(OnHandUpdated);
            EventManager.Subscribe<HandRenderedEvent>(OnHandRendered);
            EventManager.Subscribe<StageStartedEvent>(OnStageStarted);
            _hasVisibleHand = false;
            ApplyContentVisibility();
            ClearRecipeButtons();
            RefreshFromCurrentState();
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
            EventManager.Unsubscribe<HandRenderedEvent>(OnHandRendered);
            EventManager.Unsubscribe<StageStartedEvent>(OnStageStarted);
        }

        private void BindButtons()
        {
            if (_playCardsButton != null)
            {
                _playCardsButton.onClick.RemoveAllListeners();
                _playCardsButton.onClick.AddListener(OnPlayCardsClicked);
            }

            if (_discardButton != null)
            {
                _discardButton.onClick.RemoveAllListeners();
                _discardButton.onClick.AddListener(OnDiscardCardsClicked);
            }

            for (int i = 0; i < _recipeButtons.Length; i++)
            {
                if (_recipeButtons[i] == null) continue;

                int buttonIndex = i;
                _recipeButtons[i].onClick.RemoveAllListeners();
                _recipeButtons[i].onClick.AddListener(() => OnRecipeButtonClicked(buttonIndex));
            }
        }

        private void OnHandUpdated(HandUpdatedEvent e)
        {
            _currentRecipes.Clear();
            if (e.MatchingRecipes != null)
            {
                for (int i = 0; i < e.MatchingRecipes.Count; i++)
                {
                    _currentRecipes.Add(e.MatchingRecipes[i]);
                }
            }

            RefreshRecipeButtons();
        }

        private void RefreshFromCurrentState()
        {
            var currentHand = HandController.GetCurrentHand();
            if (currentHand == null || currentHand.Count == 0)
            {
                _hasVisibleHand = false;
                ApplyContentVisibility();
                return;
            }

            var handSnapshot = new List<IngredientScriptable>(currentHand.Count);
            for (int i = 0; i < currentHand.Count; i++)
            {
                handSnapshot.Add(currentHand[i]);
            }

            var matchingRecipes = DeckController.EvaluateCardsWithRecipe(handSnapshot);
            OnHandUpdated(new HandUpdatedEvent(handSnapshot, matchingRecipes));
            _hasVisibleHand = true;
            ApplyContentVisibility();
        }

        private void OnHandRendered(HandRenderedEvent e)
        {
            _hasVisibleHand = e.HandCardCount > 0;
            ApplyContentVisibility();
        }

        private void OnStageStarted(StageStartedEvent e)
        {
            _hasVisibleHand = false;
            ApplyContentVisibility();
            _currentRecipes.Clear();
            ClearRecipeButtons();
        }

        private void OnPlayCardsClicked()
        {
            EventManager.Delegate<PlaySelectedCardsRequestEvent>();
        }

        private void OnDiscardCardsClicked()
        {
            EventManager.Delegate<DiscardSelectedCardsRequestEvent>();
        }

        private void OnRecipeButtonClicked(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= _currentRecipes.Count) return;

            EventManager.Delegate(new PreviewRecipeRequestEvent(_currentRecipes[buttonIndex]));
        }

        private void RefreshRecipeButtons()
        {
            for (int i = 0; i < _recipeButtons.Length; i++)
            {
                if (_recipeButtons[i] == null) continue;

                bool hasRecipe = i < _currentRecipes.Count;
                _recipeButtons[i].gameObject.SetActive(hasRecipe);

                if (!hasRecipe)
                {
                    _recipeButtons[i].SetText(string.Empty);
                    continue;
                }

                _recipeButtons[i].SetText(_currentRecipes[i].GetName());
                _recipeButtons[i].Enable();
            }
        }

        private void ClearRecipeButtons()
        {
            for (int i = 0; i < _recipeButtons.Length; i++)
            {
                if (_recipeButtons[i] == null) continue;

                _recipeButtons[i].SetText(string.Empty);
                _recipeButtons[i].gameObject.SetActive(false);
            }
        }

        private void ApplyContentVisibility()
        {
            if (_contentRoot == null)
            {
                return;
            }

            _contentRoot.SetActive(_hasVisibleHand);
        }

        private GameObject GetContentRoot()
        {
            if (_contentRoot != null)
                return _contentRoot;

            return gameObject;
        }
    }
}