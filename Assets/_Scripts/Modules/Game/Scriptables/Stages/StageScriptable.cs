using Modules.Game.Constants;
using System.Collections.Generic;
using UnityEngine;
namespace Modules.Game.Scriptables.Stage
{
    [CreateAssetMenu(menuName = GameConstants.STAGE_SCRIPTABLE_PATH + nameof(StageScriptable), fileName = nameof(StageScriptable))]
    public class StageScriptable : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private List<RoundScriptable> _rounds;

        public string GetId() => _id;
        public List<RoundScriptable> GetRounds() => _rounds;
    }
}
