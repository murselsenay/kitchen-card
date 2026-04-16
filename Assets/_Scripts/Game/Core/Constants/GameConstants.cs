using Game.Core.Enums;
using System.Collections.Generic;

namespace Game.Core.Constants
{
    public class GameConstants
    {
        #region Scriptable Paths
        public const string ROOT_SCRIPTABLE_PATH = "Scriptables/Game/";
        public const string CARD_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Card/";
        public const string RECIPE_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Recipe/";
        public const string STAGE_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Stage/";
        public const string ROUND_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Round/";
        #endregion

        #region Addressable Keys
        public const string INGREDIENT_SCRIPTABLE_ADDRESSABLE_KEY = "ingredient-scriptables";
        public const string RECIPE_SCRIPTABLE_ADDRESSABLE_KEY = "recipe-scriptables";
        public const string STAGE_SCRIPTABLE_ADDRESSABLE_KEY = "stage-scriptables";
        public const string CARD_ITEM_SCRIPTABLE_ADDRESSABLE_KEY = "card-item";
        public const string ROUND_SELECTION_ITEM_ADDRESSABLE_KEY = "round-selection-item";
        #endregion

        #region Game Constants

        public const int DEFAULT_HAND_SIZE = 12;
        public const int DEFAULT_SELECTED_CARDS_LIMIT = 6;

        #endregion

        #region Rarity Weights

        public static readonly Dictionary<ERarity, int> RARITY_WEIGHTS = new Dictionary<ERarity, int>
        {
            { ERarity.Local, 60 },
            { ERarity.Gourmet, 35 },
            { ERarity.Signature, 5 },
        };

        #endregion
    }
}