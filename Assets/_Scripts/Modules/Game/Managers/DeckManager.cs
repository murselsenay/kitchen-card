using Cysharp.Threading.Tasks;
using Modules.Game.Constants;
using Modules.Game.Enums;
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
        }

        private static async UniTask CreateDeck()
        {
            _deck = new List<CardScriptable>(CardManager.GetAllCardScriptables());
        }

        public static CardScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None)
        {
            return GetRandomCard(category, null);
        }
        public static CardScriptable GetRandomCard(EIngredientCategory category = EIngredientCategory.None, List<CardScriptable> excludeCards = null)
        {
            var filteredDeck = new List<CardScriptable>(_deck);
            if (category != EIngredientCategory.None)
            {
                filteredDeck = filteredDeck.FindAll(card => card.GetCardCategory() == category);
            }
            if (excludeCards != null && excludeCards.Count > 0)
            {
                filteredDeck = filteredDeck.FindAll(card => !excludeCards.Contains(card));
            }

            int totalWeight = 0;
            foreach (var card in filteredDeck)
            {
                totalWeight += card.GetTotalWeight();
            }
            if (totalWeight == 0 || filteredDeck.Count == 0)
                return null;
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            CardScriptable drawnCard = null;
            foreach (var card in filteredDeck)
            {
                cumulative += card.GetTotalWeight();
                if (randomValue < cumulative)
                {
                    drawnCard = card;
                    break;
                }
            }
            return drawnCard;
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

        public static bool AddSelectedCard(CardScriptable card)
        {
            if (_selectedCards == null)
                _selectedCards = new List<CardScriptable>(GameConstants.DEFAULT_SELECTED_CARDS_LIMIT);
            if (_selectedCards.Count < 6 && !_selectedCards.Contains(card))
            {
                _selectedCards.Add(card);
                return true;
            }
            return false;
        }

        public static bool RemoveSelectedCard(CardScriptable card)
        {
            if (_selectedCards != null && _selectedCards.Contains(card))
                _selectedCards.Remove(card);

            return true;
        }

        #region Test Methods

        public static List<CardScriptable> CreateTestHand()
        {
            var cards = new List<CardScriptable>();
            cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Spice, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Sauce, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Vegetable, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Pantry, cards));
            cards.Add(GetRandomCard(EIngredientCategory.Protein, cards));
            cards.Add(GetRandomCard(excludeCards: cards));
            return cards;
        }
        #endregion
    }
}