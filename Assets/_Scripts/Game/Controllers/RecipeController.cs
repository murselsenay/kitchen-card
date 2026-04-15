using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Game.Core.Constants;
using Game.Core.Enums;
using Game.Models.Recipes;
using Modules.Logger;
using System.Collections.Generic;

namespace Game.Controllers
{
    public static class RecipeController
    {
        private static Dictionary<ERecipeCategory, List<RecipeScriptable>> _recipeScriptables;
        private static List<RecipeScriptable> _allRecipeScriptables;
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
            if (_allRecipeScriptables != null)
                return _allRecipeScriptables;

            _allRecipeScriptables = new List<RecipeScriptable>();
            foreach (var category in _recipeScriptables.Values)
            {
                _allRecipeScriptables.AddRange(category);
            }
            return _allRecipeScriptables;
        }

        public static RecipeScriptable GetRecipeScriptable(ERecipeType recipeType)
        {
            foreach (var category in _recipeScriptables.Values)
            {
                var recipe = category.Find(r => r.GetRecipeType() == recipeType);
                if (recipe != null) return recipe;
            }
            DebugLogger.LogError($"RecipeScriptable with type {recipeType} not found.");
            return null;
        }
    }
}