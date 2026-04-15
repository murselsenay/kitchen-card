using Game.Core.Constants;
using Game.Core.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Models.Stages
{
    [CreateAssetMenu(menuName = GameConstants.ROUND_SCRIPTABLE_PATH + nameof(RoundScriptable), fileName = nameof(RoundScriptable))]
    public class RoundScriptable : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private List<ERecipeType> _availableRecipes;
        [SerializeField] private int _customerSatisfactionTargetPoint;

        public string GetId() => _id;
        public List<ERecipeType> GetAvailableRecipes() => _availableRecipes;
        public int GetCustormerSatisfactionTargetPoint() => _customerSatisfactionTargetPoint;
    }
}
