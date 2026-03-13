using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Modules.Game.Constants;
using Modules.Game.Enums;
using Modules.Game.Extensions;
using Modules.Game.Scriptables.Card;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
namespace Modules.Game.Scriptables.Recipe
{
    [CreateAssetMenu(menuName = GameConstants.RECIPE_SCRIPTABLE_PATH + nameof(RecipeScriptable), fileName = nameof(RecipeScriptable))]
    public class RecipeScriptable : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ERecipeType _recipeType;
        [SerializeField] private ERecipeCategory _recipeCategory;
        [SerializeField] private List<EIngredientType> _ingredients;

        public string GetName() => _name;
        public string GetDescription() => _description;
        public ERecipeType GetRecipeType() => _recipeType;
        public ERecipeCategory GetRecipeCategory() => _recipeCategory;
        public List<EIngredientType> GetIngredients() => _ingredients;
        public virtual string GetResourceKey() => $"{_recipeType.ToString().ToLower()}";
        public async UniTask<Sprite> GetSprite() => await AddressableManager.LoadAsync<Sprite>(GetResourceKey());
        public bool CheckIngredientsMet(List<EIngredientType> playerIngredients)
        {
            foreach (var ingredient in _ingredients)
            {
                if (!playerIngredients.Contains(ingredient))
                    return false;
            }
            return true;
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
