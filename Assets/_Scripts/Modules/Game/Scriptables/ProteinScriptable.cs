using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(ProteinScriptable), fileName = nameof(ProteinScriptable))]

    public class ProteinScriptable : CardScriptable
    {
        [SerializeField] private EProteinType _proteinType;
        public EProteinType GetProteinType() => _proteinType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_proteinType.ToString().Replace("_", "-").ToLower()}";
    }
}
