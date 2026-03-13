namespace Modules.Game.Constants
{
    public class GameConstants
    {
        #region Scriptable Paths
        public const string ROOT_SCRIPTABLE_PATH = "Scriptables/Game/";
        public const string CARD_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Card/";
        public const string RECIPE_SCRIPTABLE_PATH = ROOT_SCRIPTABLE_PATH + "Recipe/";
        #endregion

        #region Addressable Keys
        public const string INGREDIENT_SCRIPTABLE_ADDRESSABLE_KEY = "ingredient-scriptables";
        public const string RECIPE_SCRIPTABLE_ADDRESSABLE_KEY = "recipe-scriptables";
        public const string CARD_ITEM_SCRIPTABLE_ADDRESSABLE_KEY = "card-item";
        #endregion

        #region Game Constants

        public const int DEFAULT_HAND_SIZE = 12;
        public const int DEFAULT_SELECTED_CARDS_LIMIT = 6;

        #endregion
    }
}