using Game.Core.Enums;
using System.Collections.Generic;

namespace Game.Core.Constants
{
    public static class CardConstants
    {
        public static readonly IReadOnlyList<EIngredientType> EXCLUDED_INGREDIENTS = new List<EIngredientType>
        {
            EIngredientType.None,
            EIngredientType.Baking_Powder,
            EIngredientType.Dry_Oats,
            EIngredientType.Dry_Yeast,
            EIngredientType.Wheat,
            EIngredientType.Vinegar,
            EIngredientType.Powder_Pepper,
            EIngredientType.Turmeric,
            EIngredientType.Balzamic_Glaze,
            EIngredientType.Basic_Sauce,
            EIngredientType.Hollandaise,
            EIngredientType.Ketchup,
        };

        #region Card Category Weighting

        public static readonly Dictionary<EIngredientCategory, int> CARD_CATEGORY_WEIGHTS = new Dictionary<EIngredientCategory, int>
        {
            { EIngredientCategory.Vegetable, 20 },
            { EIngredientCategory.Fruit, 5 },
            { EIngredientCategory.Pantry, 20  },
            { EIngredientCategory.Protein, 25 },
            { EIngredientCategory.Sauce, 15 },
            { EIngredientCategory.Spice, 15 },
        };

        #endregion
    }
}