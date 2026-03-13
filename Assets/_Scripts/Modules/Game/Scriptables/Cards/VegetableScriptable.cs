using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables.Card
{
    [CreateAssetMenu(menuName = GameConstants.CARD_SCRIPTABLE_PATH + nameof(VegetableScriptable), fileName = nameof(VegetableScriptable))]

    public class VegetableScriptable : CardScriptable
    {
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{GetIngredientType().ToString().Replace("_", "-").ToLower()}";
    }
}
