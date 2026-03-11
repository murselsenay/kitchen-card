using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(FruitScriptable), fileName = nameof(FruitScriptable))]

    public class FruitScriptable : CardScriptable
    {
        [SerializeField] private EFruitType _fruitType;
        public EFruitType GetFruitType() => _fruitType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_fruitType.ToString().Replace("_", "-").ToLower()}";
    }
}
