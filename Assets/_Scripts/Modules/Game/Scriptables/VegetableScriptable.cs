using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(VegetableScriptable), fileName = nameof(VegetableScriptable))]

    public class VegetableScriptable : CardScriptable
    {
        [SerializeField] private EVegetableType _vegetableType;
        public EVegetableType GetVegetableType() => _vegetableType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_vegetableType.ToString().Replace("_", "-").ToLower()}";
    }
}
