using Game.Core.Constants;
using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Recipes;
using Game.Models.Stages;
using Modules.Event.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Controllers
{
    public static class HandController
    {
        private static readonly List<IngredientScriptable> _currentHand = new List<IngredientScriptable>();
        private static readonly List<IngredientScriptable> _selectedCards = new List<IngredientScriptable>();
        private static RecipeScriptable _previewedRecipe;

        private static bool _isSubscribed;

        public static void Init()
        {
            EnsureSubscribed();
        }

        public static IReadOnlyList<IngredientScriptable> GetCurrentHand() => _currentHand;
        public static IReadOnlyList<IngredientScriptable> GetSelectedCards() => _selectedCards;

        public static void DiscardSelectedCards()
        {
            ResolveSelectedCards(isPlayAction: false);
        }

        public static void PlaySelectedCards()
        {
            ResolveSelectedCards(isPlayAction: true);
        }

        private static void EnsureSubscribed()
        {
            if (_isSubscribed) return;

            EventManager.Subscribe<AddSelectedCardEvent>(OnAddSelectedCard);
            EventManager.Subscribe<RemoveSelectedCardEvent>(OnRemoveSelectedCard);
            EventManager.Subscribe<DiscardSelectedCardsRequestEvent>(OnDiscardSelectedCardsRequested);
            EventManager.Subscribe<PlaySelectedCardsRequestEvent>(OnPlaySelectedCardsRequested);
            EventManager.Subscribe<PreviewRecipeRequestEvent>(OnPreviewRecipeRequested);
            EventManager.Subscribe<RoundDeckCreatedEvent>(OnRoundDeckCreated);
            EventManager.Subscribe<StageStartedEvent>(OnStageStarted);

            _isSubscribed = true;
        }

        private static void OnAddSelectedCard(AddSelectedCardEvent e)
        {
            if (e.Card == null || !_currentHand.Contains(e.Card)) return;
            if (_selectedCards.Count >= GameConstants.DEFAULT_SELECTED_CARDS_LIMIT) return;
            if (_selectedCards.Contains(e.Card)) return;

            if (_previewedRecipe != null)
            {
                ClearRecipePreview();
            }

            _selectedCards.Add(e.Card);
            EventManager.Delegate(new CardSelectedEvent(e.Card));
        }

        private static void OnRemoveSelectedCard(RemoveSelectedCardEvent e)
        {
            if (e.Card == null || !_selectedCards.Remove(e.Card)) return;

            if (_previewedRecipe != null)
            {
                ClearRecipePreview();
            }

            EventManager.Delegate(new CardDeselectedEvent(e.Card));
        }

        private static void OnDiscardSelectedCardsRequested(DiscardSelectedCardsRequestEvent e)
        {
            DiscardSelectedCards();
        }

        private static void OnPlaySelectedCardsRequested(PlaySelectedCardsRequestEvent e)
        {
            PlaySelectedCards();
        }

        private static void OnRoundDeckCreated(RoundDeckCreatedEvent e)
        {
            CreateOpeningHand(e.Round);
        }

        private static void OnPreviewRecipeRequested(PreviewRecipeRequestEvent e)
        {
            if (e.Recipe == null)
            {
                ClearSelection();
                ClearRecipePreview();
                return;
            }

            if (_previewedRecipe == e.Recipe)
            {
                ClearSelection();
                ClearRecipePreview();
                return;
            }

            _previewedRecipe = e.Recipe;
            var previewCards = GetPreviewCardsForRecipe(_previewedRecipe);
            ReplaceSelection(previewCards);
            EventManager.Delegate(new RecipePreviewChangedEvent(_previewedRecipe, previewCards));
        }

        private static void OnStageStarted(StageStartedEvent e)
        {
            _currentHand.Clear();
            ClearSelection();
            ClearRecipePreview();
            DispatchHandUpdated();
        }

        private static void CreateOpeningHand(RoundScriptable round)
        {
            _currentHand.Clear();
            ClearSelection();
            ClearRecipePreview();

            if (round == null)
            {
                DispatchHandUpdated();
                return;
            }

            _currentHand.AddRange(DeckController.DrawCards(round.GetHandCardCount()));
            DispatchHandUpdated();
        }

        private static void ResolveSelectedCards(bool isPlayAction)
        {
            if (_selectedCards.Count == 0) return;

            var currentRound = StageController.GetCurrentRound();
            if (currentRound == null) return;

            var affectedCards = new List<IngredientScriptable>(_selectedCards);
            var playedRecipe = isPlayAction ? _previewedRecipe : null;
            var removedCardIndexes = new List<int>(affectedCards.Count);

            for (int i = 0; i < affectedCards.Count; i++)
            {
                int cardIndex = _currentHand.IndexOf(affectedCards[i]);
                if (cardIndex < 0) continue;

                removedCardIndexes.Add(cardIndex);
            }

            removedCardIndexes.Sort();

            for (int i = removedCardIndexes.Count - 1; i >= 0; i--)
            {
                _currentHand.RemoveAt(removedCardIndexes[i]);
            }

            ClearSelection();
            ClearRecipePreview();

            int cardsToDraw = Mathf.Max(0, currentRound.GetHandCardCount() - _currentHand.Count);
            var drawnCards = DeckController.DrawCards(cardsToDraw, _currentHand);

            for (int i = 0; i < drawnCards.Count; i++)
            {
                int targetIndex = i < removedCardIndexes.Count ? removedCardIndexes[i] : _currentHand.Count;
                targetIndex = Mathf.Clamp(targetIndex, 0, _currentHand.Count);
                _currentHand.Insert(targetIndex, drawnCards[i]);
            }

            if (isPlayAction)
            {
                EventManager.Delegate(new SelectedCardsPlayedEvent(affectedCards, drawnCards, playedRecipe));
            }
            else
            {
                EventManager.Delegate(new SelectedCardsDiscardedEvent(affectedCards, drawnCards));
            }

            DispatchHandUpdated();
        }

        private static void ClearSelection()
        {
            if (_selectedCards.Count == 0) return;

            var deselectedCards = new List<IngredientScriptable>(_selectedCards);
            _selectedCards.Clear();

            for (int i = 0; i < deselectedCards.Count; i++)
            {
                EventManager.Delegate(new CardDeselectedEvent(deselectedCards[i]));
            }
        }

        private static void ReplaceSelection(IReadOnlyList<IngredientScriptable> cardsToSelect)
        {
            ClearSelection();

            if (cardsToSelect == null) return;

            for (int i = 0; i < cardsToSelect.Count; i++)
            {
                var card = cardsToSelect[i];
                if (card == null) continue;
                if (!_currentHand.Contains(card)) continue;
                if (_selectedCards.Count >= GameConstants.DEFAULT_SELECTED_CARDS_LIMIT) break;
                if (_selectedCards.Contains(card)) continue;

                _selectedCards.Add(card);
                EventManager.Delegate(new CardSelectedEvent(card));
            }
        }

        private static void ClearRecipePreview()
        {
            _previewedRecipe = null;
            EventManager.Delegate(new RecipePreviewChangedEvent(null, new List<IngredientScriptable>()));
        }

        private static List<IngredientScriptable> GetPreviewCardsForRecipe(RecipeScriptable recipe)
        {
            var previewCards = new List<IngredientScriptable>();
            if (recipe == null) return previewCards;

            var availableCards = new List<IngredientScriptable>(_currentHand);
            var requiredIngredients = recipe.GetIngredients();
            for (int i = 0; i < requiredIngredients.Count; i++)
            {
                var matchingCard = availableCards.Find(card => card.GetIngredientType() == requiredIngredients[i]);
                if (matchingCard == null) continue;

                previewCards.Add(matchingCard);
                availableCards.Remove(matchingCard);
            }

            return previewCards;
        }

        private static void DispatchHandUpdated()
        {
            var handSnapshot = new List<IngredientScriptable>(_currentHand);
            var matchingRecipes = new List<RecipeScriptable>(DeckController.EvaluateCardsWithRecipe(handSnapshot));
            EventManager.Delegate(new HandUpdatedEvent(handSnapshot, matchingRecipes));
        }
    }
}