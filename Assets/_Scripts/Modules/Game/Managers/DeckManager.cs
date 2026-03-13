using Cysharp.Threading.Tasks;
using Modules.Event.Managers;
using Modules.Game.Constants;
using Modules.Game.Enums;
using Modules.Game.Events;
using Modules.Game.Scriptables.Card;
using Modules.Game.Scriptables.Recipe;
using Modules.Logger;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Game.Managers
{
    public static class DeckManager
    {
        private static List<CardScriptable> _deck;
        private static List<CardScriptable> _selectedCards;
        public static async UniTask Init()
        {
            await CreateDeck();

            EventSubscribe();
        }

        public static void EventSubscribe()
        {
            EventManager.Subscribe<AddSelectedCardEvent>(OnAddSelectedCard);
            EventManager.Subscribe<RemoveSelectedCardEvent>(OnRemoveSelectedCard);
        }

        private static async UniTask CreateDeck()
        {
            _deck = CardManager.GetStageCardScriptables();
            DebugLogger.Log(_deck.Count.ToString());
        }

        public static CardScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None)
        {
            return GetRandomCard(category, null);
        }
        public static CardScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None, List<CardScriptable> excludeCards = null)
        {
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

        public static List<RecipeScriptable> EvaluateCardsWithRecipe(List<CardScriptable> handCards)
        {
            var playerIngredients = new List<Modules.Game.Enums.EIngredientType>();
            foreach (var card in handCards)
            {
                playerIngredients.Add(card.GetIngredientType());
            }

            var matchingRecipes = new List<RecipeScriptable>();

            foreach (var recipe in RecipeManager.GetAllRecipeScriptables())
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

        public static List<CardScriptable> GetSelectedCards()
        {
            return _selectedCards ?? new List<CardScriptable>();
        }

        public static void OnAddSelectedCard(AddSelectedCardEvent e)
        {
            if (_selectedCards == null)
                _selectedCards = new List<CardScriptable>(GameConstants.DEFAULT_SELECTED_CARDS_LIMIT);
            if (_selectedCards.Count < 6 && !_selectedCards.Contains(e.Card))
            {
                _selectedCards.Add(e.Card);
                EventManager.Delegate(new CardSelectedEvent(e.Card));
            }
        }

        public static void OnRemoveSelectedCard(RemoveSelectedCardEvent e)
        {
            if (_selectedCards != null && _selectedCards.Contains(e.Card))
            {
                _selectedCards.Remove(e.Card);
                EventManager.Delegate(new CardDeselectedEvent(e.Card));
            }
        }

        #region Test Methods

        public static List<CardScriptable> CreateTestHand()
        {
            var cards = new List<CardScriptable>();
            //cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Spice, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Sauce, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            //cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            return cards;
        }
        #endregion
    }
}