using UnityEngine;
using Modules.Game.Constants;
using Modules.Game.Enums;
using Modules.AdressableSystem;
using Cysharp.Threading.Tasks;
namespace Modules.Game.Scriptables
{
    [CreateAssetMenu(menuName = GameConstants.ROOT_SCRIPTABLE_PATH + nameof(CardScriptable), fileName = nameof(CardScriptable))]

    public class CardScriptable : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ECardType _type;
        [SerializeField] private ECardCategory _category;
        [SerializeField] private ECardRarity _rarity;

        public string GetName() => _name;
        public string GetDescription() => _description;
        public ECardType GetCardType() => _type;
        public ECardCategory GetCardCategory() => _category;
        public ECardRarity GetCardRarity() => _rarity;
        public virtual string GetResourceKey() { return string.Empty; }
        public async UniTask<Sprite> GetSprite() => await AddressableManager.LoadAsync<Sprite>(GetResourceKey());
    }
}
