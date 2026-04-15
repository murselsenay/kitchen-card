using Game.Core.Constants;
using UnityEngine;

namespace Game.Models.Cards
{
    [CreateAssetMenu(menuName = GameConstants.CARD_SCRIPTABLE_PATH + nameof(FruitScriptable), fileName = nameof(FruitScriptable))]

    public class FruitScriptable : CardScriptable
    {
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{GetIngredientType().ToString().Replace("_", "-").ToLower()}";
    }
}
