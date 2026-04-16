using Cysharp.Threading.Tasks;
using Modules.Event.Managers;
using Game.Core.Enums;
using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Recipes;
using Modules.Logger;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Controllers
{
    public static class DeckController
    {
        private static List<IngredientScriptable> _deck;
        private static bool _isSubscribed;

        public static async UniTask Init()
        {
            EventSubscribe();

            if (StageController.GetCurrentRound() != null)
            {
                CreateDeck(StageController.GetCurrentStage(), StageController.GetCurrentRound());
            }

            await UniTask.CompletedTask;
        }

        public static void EventSubscribe()
        {
            if (_isSubscribed) return;

            EventManager.Subscribe<StageRoundSelectedEvent>(OnStageRoundSelected);
            _isSubscribed = true;
        }

        private static void OnStageRoundSelected(StageRoundSelectedEvent e)
        {
            CreateDeck(e.Stage, e.Round);
        }

        private static void CreateDeck(Game.Models.Stages.StageScriptable stage, Game.Models.Stages.RoundScriptable round)
        {
            _deck = CardController.GetStageCardScriptables();
            DebugLogger.Log(_deck.Count.ToString());
            EventManager.Delegate(new RoundDeckCreatedEvent(stage, round, _deck.Count));
        }

        public static IngredientScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None)
        {
            return GetRandomCard(category, null);
        }
        public static IngredientScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None, List<IngredientScriptable> excludeCards = null)
        {
            if (_deck == null || _deck.Count == 0) return null;

            int totalWeight = 0;
            foreach (var card in _deck)
            {
                if (category != EIngredientCategory.None && card.GetCardCategory() != category) continue;
                if (excludeCards != null && excludeCards.Contains(card)) continue;

                totalWeight += card.GetTotalWeight();
            }

            if (totalWeight <= 0) return null;

            int randomValue = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var card in _deck)
            {
                if (category != EIngredientCategory.None && card.GetCardCategory() != category) continue;
                if (excludeCards != null && excludeCards.Contains(card)) continue;

                cumulative += card.GetTotalWeight();
                if (randomValue < cumulative) return card;
            }

            return null;
        }

        public static List<IngredientScriptable> DrawCards(int cardCount, IReadOnlyList<IngredientScriptable> excludeCards = null)
        {
            var drawnCards = new List<IngredientScriptable>(cardCount);
            if (cardCount <= 0) return drawnCards;

            for (int i = 0; i < cardCount; i++)
            {
                var cardsToExclude = new List<IngredientScriptable>(drawnCards);
                if (excludeCards != null)
                {
                    for (int j = 0; j < excludeCards.Count; j++)
                    {
                        if (excludeCards[j] == null || cardsToExclude.Contains(excludeCards[j])) continue;
                        cardsToExclude.Add(excludeCards[j]);
                    }
                }

                var randomCard = GetRandomCard(excludeCards: cardsToExclude);
                if (randomCard == null) break;

                drawnCards.Add(randomCard);
            }

            return drawnCards;
        }

        public static List<RecipeScriptable> EvaluateCardsWithRecipe(List<IngredientScriptable> handCards)
        {
            var playerIngredients = new List<EIngredientType>();
            foreach (var card in handCards)
            {
                playerIngredients.Add(card.GetIngredientType());
            }

            var matchingRecipes = new List<RecipeScriptable>();

            foreach (var recipe in RecipeController.GetAllRecipeScriptables())
            {
                if (recipe.CheckIngredientsMet(playerIngredients))
                {
                    matchingRecipes.Add(recipe);
                    UnityEngine.Debug.Log($"Completed recipe: {recipe.GetName()}");
                }
            }

            if (matchingRecipes.Count == 0)
            {
                UnityEngine.Debug.Log("No matching recipe found with the current hand.");
            }

            return matchingRecipes;
        }

        #region Test Methods

        public static List<IngredientScriptable> CreateTestHand()
        {
            return DrawCards(6);
        }
        #endregion
    }
}