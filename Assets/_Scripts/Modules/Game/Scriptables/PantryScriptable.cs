using Modules.Game.Constants;
using Modules.Game.Enums;
using UnityEngine;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(PantryScriptable), fileName = nameof(PantryScriptable))]

    public class PantryScriptable : CardScriptable
    {
        [SerializeField] private EPantryType _pantryType;
        public EPantryType GetPantryType() => _pantryType;
        public override string GetResourceKey() => $"{GetCardType().ToString().ToLower()}-{_pantryType.ToString().Replace("_", "-").ToLower()}";
    }
}
