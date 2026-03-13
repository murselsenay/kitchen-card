using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables.Card
{
    [CreateAssetMenu(menuName = GameConstants.CARD_SCRIPTABLE_PATH + nameof(SauceScriptable), fileName = nameof(SauceScriptable))]

    public class SauceScriptable : CardScriptable
    {
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{GetIngredientType().ToString().Replace("_", "-").ToLower()}";
    }
}
