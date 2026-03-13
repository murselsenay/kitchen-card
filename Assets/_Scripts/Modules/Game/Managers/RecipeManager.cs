using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Modules.Game.Constants;
using Modules.Game.Enums;
using Modules.Game.Scriptables.Recipe;
using Modules.Logger;
using System.Collections.Generic;

namespace Modules.Game.Managers
{
    public static class RecipeManager
    {
        private static Dictionary<ERecipeCategory, List<RecipeScriptable>> _recipeScriptables;

        public static async UniTask Init()
        {
            await LoadRecipeData();
        }

        private static async UniTask LoadRecipeData()
        {
            var recipeScriptables = await AddressableManager.LoadAllAsync<RecipeScriptable>(GameConstants.RECIPE_SCRIPTABLE_ADDRESSABLE_KEY);

            _recipeScriptables = new Dictionary<ERecipeCategory, List<RecipeScriptable>>();

            foreach (var recipe in recipeScriptables)
            {
                if (!_recipeScriptables.ContainsKey(recipe.GetRecipeCategory()))
                {
                    _recipeScriptables[recipe.GetRecipeCategory()] = new List<RecipeScriptable>();
                }
                _recipeScriptables[recipe.GetRecipeCategory()].Add(recipe);
            }
        }

        public static List<RecipeScriptable> GetAllRecipeScriptables()
        {
            var allRecipes = new List<RecipeScriptable>();
            foreach (var category in _recipeScriptables.Values)
            {
                allRecipes.AddRange(category);
            }
            return allRecipes;
        }
    }
}