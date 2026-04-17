using Game.Models.Cards;
using Game.Models.Recipes;
using System.Collections.Generic;

namespace Game.Models.Play
{
    public sealed class ResolvedRecipePlay
    {
        public RecipeScriptable Recipe { get; set; }
        public List<IngredientScriptable> ConsumedCards { get; } = new List<IngredientScriptable>();
        public int RarityBonus { get; set; }
        public int CategoryBonus { get; set; }
        public int BonusScore => RarityBonus + CategoryBonus;
    }

    public sealed class PlayResolutionResult
    {
        public List<IngredientScriptable> PlayedCards { get; } = new List<IngredientScriptable>();
        public List<IngredientScriptable> LooseCards { get; } = new List<IngredientScriptable>();
        public List<ResolvedRecipePlay> Recipes { get; } = new List<ResolvedRecipePlay>();
        public int IngredientScore { get; set; }
        public int RecipeBonusScore { get; set; }
        public int TotalScore { get; set; }
    }
}