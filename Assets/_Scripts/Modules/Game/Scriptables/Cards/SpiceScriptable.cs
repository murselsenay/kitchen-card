using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables.Card
{
    [CreateAssetMenu(menuName = GameConstants.CARD_SCRIPTABLE_PATH + nameof(SpiceScriptable), fileName = nameof(SpiceScriptable))]

    public class SpiceScriptable : CardScriptable
    {
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{GetIngredientType().ToString().Replace("_", "-").ToLower()}";
    }
}
