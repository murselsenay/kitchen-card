using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables.Card
{
    [CreateAssetMenu(menuName = GameConstants.CARD_SCRIPTABLE_PATH + nameof(PantryScriptable), fileName = nameof(PantryScriptable))]

    public class PantryScriptable : CardScriptable
    {
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{GetIngredientType().ToString().Replace("_", "-").ToLower()}";
    }
}
