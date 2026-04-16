using Game.Core.Constants;
using Game.Core.Enums;
using Game.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Models.Stages
{
    [CreateAssetMenu(menuName = GameConstants.ROUND_SCRIPTABLE_PATH + nameof(RoundScriptable), fileName = nameof(RoundScriptable))]
    public class RoundScriptable : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private ERoundDifficulty _roundDifficulty;
        [SerializeField] private List<ERecipeType> _availableRecipes;
        [SerializeField] private int _customerSatisfactionTargetPoint;
        [SerializeField] private int _handCardCount;

        public string GetId() => _id;
        public ERoundDifficulty GetRoundDifficulty() => _roundDifficulty;
        public string GetRoundDifficultyName() => _roundDifficulty.GetRoundDifficultyName();
        public List<ERecipeType> GetAvailableRecipes() => _availableRecipes;
        public int GetCustormerSatisfactionTargetPoint() => _customerSatisfactionTargetPoint;
        public int GetHandCardCount() => _handCardCount;
    }
}
