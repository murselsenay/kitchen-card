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
        [SerializeField]private CardViewer _cardViewer;
        [SerializeField]private CardAnimator _cardAnimator;
        private IngredientScriptable _cardScriptable;
        public IngredientScriptable CardScriptable => _cardScriptable;
        private bool _isPreviewSelected;
        private bool _isInitialized;

        private bool _isSelected => ContainsCard(HandController.GetSelectedCards(), _cardScriptable);

        private void Awake()
        {
            EnsurePresentationComponents();
        }

        public async UniTask Init(IngredientScriptable cardScriptable)
        {
            _isInitialized = false;
            _cardScriptable = cardScriptable;
            _cardAnimator.SetBaseLocalPosition(transform.localPosition);
            _cardViewer.SetIcon(await cardScriptable.GetIconSprite());
            _isPreviewSelected = false;
            RefreshSelectionVisual();
            _isInitialized = true;
        }

        public void SetSlotLocalPosition(Vector3 localPosition)
        {
            transform.localPosition = localPosition;
            _cardAnimator.SetBaseLocalPosition(localPosition);
        }

        public void SnapToBaseState()
        {
            _cardAnimator.SnapToBaseState();
        }

        public UniTask PlayRecipeMergeAsync(Vector3 targetWorldPosition)
        {
            return _cardAnimator.PlayRecipeMergeAsync(targetWorldPosition);
        }

        public UniTask PlayLooseConsumeAsync()
        {
            return _cardAnimator.PlayLooseConsumeAsync();
        }

        public UniTask PlaySpawnAsync()
        {
            return _cardAnimator.PlaySpawnAsync();
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

        private void OnViewerClicked()
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

        private void OnDestroy()
        {
            if (_cardViewer != null)
            {
                _cardViewer.Clicked -= OnViewerClicked;
            }
        }

        private void RefreshSelectionVisual()
        {
            if (_cardScriptable == null) return;

            _cardAnimator.ApplySelectionState(_isSelected, _isPreviewSelected);
        }

        private void EnsurePresentationComponents()
        {
            _cardViewer = GetComponent<CardViewer>();
            if (_cardViewer == null)
            {
                _cardViewer = gameObject.AddComponent<CardViewer>();
            }

            _cardAnimator = GetComponent<CardAnimator>();
            if (_cardAnimator == null)
            {
                _cardAnimator = gameObject.AddComponent<CardAnimator>();
            }

            _cardViewer.Clicked -= OnViewerClicked;
            _cardViewer.Clicked += OnViewerClicked;
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