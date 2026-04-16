using Game.Controllers;
using Game.Core.Enums;
using Game.Models.Cards;
using Game.Models.Recipes;
using Modules.Singleton;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Models.Scoring
{
    [CreateAssetMenu(menuName = "Scriptables/Singletons/" + nameof(ScoringConfig), fileName = nameof(ScoringConfig))]
    public class ScoringConfig : ScriptableSingleton<ScoringConfig>
    {
        [Serializable]
        private struct RarityPointEntry
        {
            public ERarity Rarity;
            public int Points;
        }

        [Serializable]
        private struct IngredientCategoryPointEntry
        {
            public EIngredientCategory Category;
            public int Points;
        }

        [Serializable]
        private struct RecipeCategoryPointEntry
        {
            public ERecipeCategory Category;
            public int Points;
        }

        [Header("Ingredient Points")]
        [SerializeField] private List<RarityPointEntry> _ingredientRarityPoints = new List<RarityPointEntry>
        {
            new RarityPointEntry { Rarity = ERarity.Local, Points = 2 },
            new RarityPointEntry { Rarity = ERarity.Gourmet, Points = 4 },
            new RarityPointEntry { Rarity = ERarity.Signature, Points = 7 },
        };

        [SerializeField] private List<IngredientCategoryPointEntry> _ingredientCategoryPoints = new List<IngredientCategoryPointEntry>
        {
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Vegetable, Points = 0 },
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Fruit, Points = 0 },
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Pantry, Points = 1 },
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Spice, Points = 1 },
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Sauce, Points = 1 },
            new IngredientCategoryPointEntry { Category = EIngredientCategory.Protein, Points = 2 },
        };

        [Header("Recipe Bonuses")]
        [SerializeField] private List<RarityPointEntry> _recipeRarityBonuses = new List<RarityPointEntry>
        {
            new RarityPointEntry { Rarity = ERarity.Local, Points = 2 },
            new RarityPointEntry { Rarity = ERarity.Gourmet, Points = 5 },
            new RarityPointEntry { Rarity = ERarity.Signature, Points = 9 },
        };

        [SerializeField] private List<RecipeCategoryPointEntry> _recipeCategoryBonuses = new List<RecipeCategoryPointEntry>
        {
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Two, Points = 0 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Three, Points = 2 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Four, Points = 5 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Five, Points = 8 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Six, Points = 12 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Unique, Points = 15 },
            new RecipeCategoryPointEntry { Category = ERecipeCategory.Desert, Points = 10 },
        };

        public int GetIngredientPoints(IngredientScriptable ingredient)
        {
            if (ingredient == null) return 0;

            return GetIngredientRarityPoints(ingredient.GetRarity())
                + GetIngredientCategoryPoints(ingredient.GetCardCategory());
        }

        public int GetRecipePoints(RecipeScriptable recipe)
        {
            if (recipe == null) return 0;

            int totalPoints = 0;
            var ingredientTypes = recipe.GetIngredients();
            for (int i = 0; i < ingredientTypes.Count; i++)
            {
                var ingredient = CardController.GetCard(ingredientTypes[i]);
                totalPoints += GetIngredientPoints(ingredient);
            }

            totalPoints += GetRecipeRarityBonus(recipe.GetRarity());
            totalPoints += GetRecipeCategoryBonus(recipe.GetRecipeCategory());

            return Mathf.Max(0, totalPoints);
        }

        public int GetIngredientRarityPoints(ERarity rarity)
        {
            for (int i = 0; i < _ingredientRarityPoints.Count; i++)
            {
                if (_ingredientRarityPoints[i].Rarity == rarity)
                {
                    return _ingredientRarityPoints[i].Points;
                }
            }

            return 0;
        }

        public int GetIngredientCategoryPoints(EIngredientCategory category)
        {
            for (int i = 0; i < _ingredientCategoryPoints.Count; i++)
            {
                if (_ingredientCategoryPoints[i].Category == category)
                {
                    return _ingredientCategoryPoints[i].Points;
                }
            }

            return 0;
        }

        public int GetRecipeRarityBonus(ERarity rarity)
        {
            for (int i = 0; i < _recipeRarityBonuses.Count; i++)
            {
                if (_recipeRarityBonuses[i].Rarity == rarity)
                {
                    return _recipeRarityBonuses[i].Points;
                }
            }

            return 0;
        }

        public int GetRecipeCategoryBonus(ERecipeCategory category)
        {
            for (int i = 0; i < _recipeCategoryBonuses.Count; i++)
            {
                if (_recipeCategoryBonuses[i].Category == category)
                {
                    return _recipeCategoryBonuses[i].Points;
                }
            }

            return 0;
        }
    }
}