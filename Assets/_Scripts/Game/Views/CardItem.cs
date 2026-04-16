using Cysharp.Threading.Tasks;
using Modules.Event.Managers;
using Game.Controllers;
using Game.Core.Events;
using Game.Models.Cards;
using UnityEngine;

namespace Game.Views
{
    public class CardItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer _iconSpriteRenderer;

        private IngredientScriptable _cardScriptable;
        public IngredientScriptable CardScriptable => _cardScriptable;
        private bool _isPreviewSelected;
        private bool _isInitialized;

        private bool _isSelected => ContainsCard(HandController.GetSelectedCards(), _cardScriptable);

        public async UniTask Init(IngredientScriptable cardScriptable)
        {
            _isInitialized = false;
            _cardScriptable = cardScriptable;
            _iconSpriteRenderer.sprite = await cardScriptable.GetIconSprite();
            _isPreviewSelected = false;
            RefreshSelectionVisual();
            _isInitialized = true;
        }

        public void OnCardSelected(CardSelectedEvent e)
        {
            if (e.Card != _cardScriptable) return;
            RefreshSelectionVisual();
        }

        public void OnCardDeselected(CardDeselectedEvent e)
        {
            if (e.Card != _cardScriptable) return;
            RefreshSelectionVisual();
        }

        public void OnRecipePreviewChanged(RecipePreviewChangedEvent e)
        {
            _isPreviewSelected = ContainsCard(e.PreviewCards, _cardScriptable);
            RefreshSelectionVisual();
        }

        private void OnMouseDown()
        {
            if (!_isInitialized) return;

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
            EventManager.Subscribe<RecipePreviewChangedEvent>(OnRecipePreviewChanged);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<CardSelectedEvent>(OnCardSelected);
            EventManager.Unsubscribe<CardDeselectedEvent>(OnCardDeselected);
            EventManager.Unsubscribe<RecipePreviewChangedEvent>(OnRecipePreviewChanged);
            _isInitialized = false;
        }

        private void RefreshSelectionVisual()
        {
            if (_cardScriptable == null) return;

            float yPosition = _isSelected || _isPreviewSelected ? 2f : 0f;
            transform.localPosition = new Vector3(transform.localPosition.x, yPosition, transform.localPosition.z);
        }

        private static bool ContainsCard(System.Collections.Generic.IReadOnlyList<IngredientScriptable> cards, IngredientScriptable targetCard)
        {
            if (cards == null || targetCard == null) return false;

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == targetCard)
                {
                    return true;
                }
            }

            return false;
        }
    }
}