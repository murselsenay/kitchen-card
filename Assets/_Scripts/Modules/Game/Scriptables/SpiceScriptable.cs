using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(SpiceScriptable), fileName = nameof(SpiceScriptable))]

    public class SpiceScriptable : CardScriptable
    {
        [SerializeField] private ESpiceType _spiceType;
        public ESpiceType GetSpiceType() => _spiceType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_spiceType.ToString().Replace("_", "-").ToLower()}";
    }
}
