using Cysharp.Threading.Tasks;
using Modules.Event.Managers;
using Modules.Game.Events;
using Modules.Game.Managers;
using Modules.Game.Scriptables.Card;
using UnityEngine;

namespace Modules.Game.Components
{
    public class CardItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;

        private CardScriptable _cardScriptable;
        public CardScriptable CardScriptable => _cardScriptable;

        private bool _isSelected => DeckManager.GetSelectedCards().Contains(_cardScriptable);
        public async UniTask Init(CardScriptable cardScriptable)
        {
            _cardScriptable = cardScriptable;
            _iconSpriteRenderer.sprite = await cardScriptable.GetIconSprite();
        }

        public void OnCardSelected(CardSelectedEvent e)
        {
            if (e.Card.GetIngredientType() != _cardScriptable.GetIngredientType()) return;
            transform.localPosition = new Vector3(transform.localPosition.x, 2f, transform.localPosition.z);
        }

        public void OnCardDeselected(CardDeselectedEvent e)
        {
            if (e.Card.GetIngredientType() != _cardScriptable.GetIngredientType()) return;
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }

        private void OnMouseDown()
        {
            if (_isSelected)
            {
                EventManager.Delegate(new RemoveSelectedCardEvent(_cardScriptable));
            }
            else
            {
                EventManager.Delegate(new AddSelectedCardEvent(_cardScriptable));
            }
        }

        private void OnEnable()
        {
            EventManager.Subscribe<CardSelectedEvent>(OnCardSelected);
            EventManager.Subscribe<CardDeselectedEvent>(OnCardDeselected);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<CardSelectedEvent>(OnCardSelected);
            EventManager.Unsubscribe<CardDeselectedEvent>(OnCardDeselected);
        }
    }
}