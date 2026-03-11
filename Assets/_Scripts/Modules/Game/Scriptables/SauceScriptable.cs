using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(SauceScriptable), fileName = nameof(SauceScriptable))]

    public class SauceScriptable : CardScriptable
    {
        [SerializeField] private ESauceType _sauceType;
        public ESauceType GetSauceType() => _sauceType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_sauceType.ToString().Replace("_", "-").ToLower()}";
    }
}
