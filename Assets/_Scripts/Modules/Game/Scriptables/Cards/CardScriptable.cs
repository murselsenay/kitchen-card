using UnityEngine;
using Modules.Game.Enums;
using Modules.AdressableSystem;
using Cysharp.Threading.Tasks;
using Modules.Game.Constants;
namespace Modules.Game.Scriptables.Card
{
    public class CardScriptable : ScriptableObject
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
        public ERarity GetCardRarity() => _rarity;
        public EIngredientType GetIngredientType() => _ingredientType;
        public virtual string GetResourceKey() { return string.Empty; }
        public async UniTask<Sprite> GetIconSprite() => await AddressableManager.LoadAsync<Sprite>(GetResourceKey());
        public int GetTotalWeight() => CardConstants.RARITY_WEIGHTS[_rarity];
    }
}
