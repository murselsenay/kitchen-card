using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Game.Controllers;
using Game.Core.Constants;
using Game.Core.Enums;
using Game.Core.Extensions;
using Game.Models.Cards;
using Game.Models.Scoring;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Models.Recipes
{
    [CreateAssetMenu(menuName = GameConstants.RECIPE_SCRIPTABLE_PATH + nameof(RecipeScriptable), fileName = nameof(RecipeScriptable))]
    public class RecipeScriptable : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ERecipeType _recipeType;
        [SerializeField] private ERecipeCategory _recipeCategory;
        [SerializeField] private List<EIngredientType> _ingredients;
        private ERarity _rarity;
        public string GetName() => _name;
        public string GetDescription() => _description;
        public ERecipeType GetRecipeType() => _recipeType;
        public ERecipeCategory GetRecipeCategory() => _recipeCategory;
        public List<EIngredientType> GetIngredients() => _ingredients;
        public virtual string GetResourceKey() => $"{_recipeType.ToString().ToLower()}";
        public int GetTotalWeight() => GameConstants.RARITY_WEIGHTS[GetRarity()];
        public int GetPoint() => ScoringConfig.Instance.GetRecipePoints(this);
        public async UniTask<Sprite> GetSprite() => await AddressableManager.LoadAsync<Sprite>(GetResourceKey());
        public ERarity GetRarity()
        {
            if (_rarity != ERarity.None) return _rarity;

            if (_ingredients == null || _ingredients.Count == 0)
                return _rarity = ERarity.None;

            int local = 0, gourmet = 0, signature = 0;

            foreach (var ingredient in _ingredients)
            {
                var card = CardController.GetCard(ingredient);
                if (card == null) continue;

                ERarity r = card.GetRarity();
                if (r == ERarity.Local) local++;
                else if (r == ERarity.Gourmet) gourmet++;
                else if (r == ERarity.Signature) signature++;
            }

            if (signature >= 2) return _rarity = ERarity.Signature;

            if (local >= gourmet && local >= signature) _rarity = ERarity.Local;
            else if (gourmet >= local && gourmet >= signature) _rarity = ERarity.Gourmet;
            else _rarity = ERarity.Signature;

            return _rarity;
        }

        public bool CheckIngredientsMet(List<EIngredientType> playerIngredients)
        {
            foreach (var ingredient in _ingredients)
            {
                if (!playerIngredients.Contains(ingredient))
                    return false;
            }
            return true;
        }

        public List<EIngredientType> GetRandomIngredients(int randomCount = 2)
        {
            List<EIngredientType> result = new List<EIngredientType>();

            // Güvenlik kontrolü: Liste boşsa hata almamak için boş dön
            if (_ingredients == null || _ingredients.Count == 0) return result;

            for (int i = 0; i < randomCount; i++)
            {
                // Tamamen rastgele bir index seç
                int randomIndex = UnityEngine.Random.Range(0, _ingredients.Count);

                // Seçilen kartın tipini (EIngredientType) sonuca ekle
                result.Add(_ingredients[randomIndex]);
            }

            return result;
        }
        [Button]
        public void FillIngredients()
        {
#if UNITY_EDITOR
            RecipeScriptableExtensions.FillIngredientsFromCsv(this);
#endif
        }
    }
}
