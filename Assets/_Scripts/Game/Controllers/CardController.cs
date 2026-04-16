using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Game.Core.Constants;
using Game.Core.Enums;
using Game.Models.Cards;
using System.Collections.Generic;
using System.Linq;

namespace Game.Controllers
{
    public static class CardController
    {
        private static Dictionary<EIngredientCategory, List<IngredientScriptable>> _cardScriptables;
        private static List<IngredientScriptable> _allCardScriptables;
        public static async UniTask Init()
        {
            await LoadCardData();
        }

        private static async UniTask LoadCardData()
        {
            var cardScriptables = await AddressableManager.LoadAllAsync<IngredientScriptable>(GameConstants.INGREDIENT_SCRIPTABLE_ADDRESSABLE_KEY);

            _cardScriptables = new Dictionary<EIngredientCategory, List<IngredientScriptable>>();

            foreach (var card in cardScriptables)
            {
                if (CardConstants.EXCLUDED_INGREDIENTS.Contains(card.GetIngredientType())) continue;
                if (!_cardScriptables.ContainsKey(card.GetCardCategory()))
                {
                    _cardScriptables[card.GetCardCategory()] = new List<IngredientScriptable>();
                }
                _cardScriptables[card.GetCardCategory()].Add(card);
            }
        }

        #region Getters
        public static IngredientScriptable GetCard(EIngredientType ingredientType)
        {
            foreach (var category in _cardScriptables.Values)
            {
                var card = category.FirstOrDefault(c => c.GetIngredientType() == ingredientType);
                if (card != null)
                    return card;
            }
            return null;
        }

        public static List<IngredientScriptable> GetCards(List<EIngredientType> ingredientTypes)
        {
            var cards = new List<IngredientScriptable>();
            foreach (var ingredientType in ingredientTypes)
            {
                var card = GetCard(ingredientType);
                if (card != null)
                    cards.Add(card);
            }
            return cards;
        }
        public static List<VegetableScriptable> GetVegetableScriptables()
        {
            return _cardScriptables[EIngredientCategory.Vegetable].ConvertAll(card => (VegetableScriptable)card);
        }
        public static List<ProteinScriptable> GetProteinScriptables()
        {
            return _cardScriptables[EIngredientCategory.Protein].ConvertAll(card => (ProteinScriptable)card);
        }
        public static List<SpiceScriptable> GetSpiceScriptables()
        {
            return _cardScriptables[EIngredientCategory.Spice].ConvertAll(card => (SpiceScriptable)card);
        }
        public static List<SauceScriptable> GetSauceScriptables()
        {
            return _cardScriptables[EIngredientCategory.Sauce].ConvertAll(card => (SauceScriptable)card);
        }
        public static List<FruitScriptable> GetFruitScriptables()
        {
            return _cardScriptables[EIngredientCategory.Fruit].ConvertAll(card => (FruitScriptable)card);
        }
        public static List<PantryScriptable> GetPantryScriptables()
        {
            return _cardScriptables[EIngredientCategory.Pantry].ConvertAll(card => (PantryScriptable)card);
        }
        public static List<IngredientScriptable> GetAllCardScriptables()
        {
            if (_allCardScriptables != null)
                return _allCardScriptables;

            _allCardScriptables = new List<IngredientScriptable>();
            foreach (var category in _cardScriptables.Values)
            {
                _allCardScriptables.AddRange(category);
            }
            return _allCardScriptables;
        }

        public static List<IngredientScriptable> GetStageCardScriptables()
        {
            var cards = new List<IngredientScriptable>();
            var currentRound = StageController.GetCurrentRound();
            if (currentRound == null)
                return cards;

            foreach (var recipe in currentRound.GetAvailableRecipes())
            {
                var recipeScriptable = RecipeController.GetRecipeScriptable(recipe);
                if (recipeScriptable != null)
                {
                    cards.AddRange(GetCards(recipeScriptable.GetIngredients()));
                }
            }

            return cards;
        }

        #endregion
    }
}