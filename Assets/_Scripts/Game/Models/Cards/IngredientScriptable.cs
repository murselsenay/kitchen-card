using UnityEngine;
using Modules.AdressableSystem;
using Cysharp.Threading.Tasks;
using Game.Core.Constants;
using Game.Core.Enums;
using Game.Models.Scoring;

namespace Game.Models.Cards
{
    public class IngredientScriptable : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ECardType _type;
        [SerializeField] private EIngredientCategory _category;
        [SerializeField] private ERarity _rarity;
        [SerializeField] private EIngredientType _ingredientType;

        public string GetName() => _name;
        public string GetDescription() => _description;
        public ECardType GetCardType() => _type;
        public EIngredientCategory GetCardCategory() => _category;
        public ERarity GetRarity() => _rarity;
        public EIngredientType GetIngredientType() => _ingredientType;
        public virtual string GetResourceKey() { return string.Empty; }
        public async UniTask<Sprite> GetIconSprite() => await AddressableManager.LoadAsync<Sprite>(GetResourceKey());
        public int GetTotalWeight() => GameConstants.RARITY_WEIGHTS[_rarity];
        public int GetPoint() => ScoringConfig.Instance.GetIngredientPoints(this);
    }
}
