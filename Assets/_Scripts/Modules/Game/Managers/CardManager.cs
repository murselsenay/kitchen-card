using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Modules.Game.Constants;
using Modules.Game.Enums;
using Modules.Game.Scriptables.Card;
using Modules.Logger;

namespace Modules.Game.Managers
{
    public static class CardManager
    {
        private static Dictionary<EIngredientCategory, List<CardScriptable>> _cardScriptables;
        public static async UniTask Init()
        {
            await LoadCardData();
        }

        private static async UniTask LoadCardData()
        {
            var cardScriptables = await AddressableManager.LoadAllAsync<CardScriptable>(GameConstants.INGREDIENT_SCRIPTABLE_ADDRESSABLE_KEY);

            _cardScriptables = new Dictionary<EIngredientCategory, List<CardScriptable>>();

            foreach (var card in cardScriptables)
            {
                if (CardConstants.EXCLUDED_INGREDIENTS.Contains(card.GetIngredientType())) continue;
                if (!_cardScriptables.ContainsKey(card.GetCardCategory()))
                {
                    _cardScriptables[card.GetCardCategory()] = new List<CardScriptable>();
                }
                _cardScriptables[card.GetCardCategory()].Add(card);
            }
        }

        #region Getters
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
        public static List<CardScriptable> GetAllCardScriptables()
        {
            var allCards = new List<CardScriptable>();
            foreach (var category in _cardScriptables.Values)
            {
                allCards.AddRange(category);
            }
            return allCards;
        }

        #endregion
    }
}