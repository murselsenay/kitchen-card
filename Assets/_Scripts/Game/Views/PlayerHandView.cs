using Cysharp.Threading.Tasks;
using Game.Controllers;
using Game.Core.Constants;
using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Recipes;
using Modules.AdressableSystem;
using Modules.Event.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Views
{
    public class PlayerHandView : MonoBehaviour
    {
        [SerializeField] private Transform _cardRoot;
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TMP_Text _possibleRecipesText;
        [SerializeField] private float _cardSpacing = 10f;
        [SerializeField] private float _replaceRenderDelaySeconds = 1f;

        private readonly List<CardItem> _spawnedItems = new List<CardItem>();
        private int _renderVersion;
        private List<IngredientScriptable> _pendingRemovedCards;
        private List<IngredientScriptable> _pendingDrawnCards;

        private void OnEnable()
        {
            HandController.Init();
            EventManager.Subscribe<HandUpdatedEvent>(OnHandUpdated);
            EventManager.Subscribe<SelectedCardsPlayedEvent>(OnSelectedCardsPlayed);
            EventManager.Subscribe<SelectedCardsDiscardedEvent>(OnSelectedCardsDiscarded);
            EventManager.Subscribe<StageStartedEvent>(OnStageStarted);
            RefreshFromController().Forget();
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<HandUpdatedEvent>(OnHandUpdated);
            EventManager.Unsubscribe<SelectedCardsPlayedEvent>(OnSelectedCardsPlayed);
            EventManager.Unsubscribe<SelectedCardsDiscardedEvent>(OnSelectedCardsDiscarded);
            EventManager.Unsubscribe<StageStartedEvent>(OnStageStarted);
            Clear();
        }

        private void OnHandUpdated(HandUpdatedEvent e)
        {
            if (HasPendingReplaceTransition())
            {
                ReplaceCardsInPlace(e.HandCards, e.MatchingRecipes).Forget();
                return;
            }

            Render(e.HandCards, e.MatchingRecipes).Forget();
        }

        private void OnSelectedCardsPlayed(SelectedCardsPlayedEvent e)
        {
            CachePendingReplaceTransition(e.PlayedCards, e.DrawnCards);
        }

        private void OnSelectedCardsDiscarded(SelectedCardsDiscardedEvent e)
        {
            CachePendingReplaceTransition(e.DiscardedCards, e.DrawnCards);
        }

        private void OnStageStarted(StageStartedEvent e)
        {
            ClearPendingReplaceTransition();
            Clear();
            Hide();
        }

        private async UniTask RefreshFromController()
        {
            var handCards = HandController.GetCurrentHand();
            if (handCards == null || handCards.Count == 0)
            {
                Hide();
                return;
            }

            var handSnapshot = new List<IngredientScriptable>(handCards);
            var matchingRecipes = new List<RecipeScriptable>(DeckController.EvaluateCardsWithRecipe(handSnapshot));
            await Render(handSnapshot, matchingRecipes);
        }

        public async UniTask Render(IReadOnlyList<IngredientScriptable> handCards, IReadOnlyList<RecipeScriptable> matchingRecipes)
        {
            Clear();

            if (_cardRoot == null || handCards == null || handCards.Count == 0)
            {
                Hide();
                UpdatePossibleRecipesText(matchingRecipes);
                EventManager.Delegate(new HandRenderedEvent(0));
                return;
            }

            int renderVersion = _renderVersion;
            float totalWidth = (handCards.Count - 1) * _cardSpacing;
            var spawnTasks = new UniTask<CardItem>[handCards.Count];

            for (int i = 0; i < handCards.Count; i++)
            {
                float xPosition = i * _cardSpacing - totalWidth / 2f;
                spawnTasks[i] = SpawnCardItem(handCards[i], xPosition, renderVersion);
            }

            var cardItems = await UniTask.WhenAll(spawnTasks);
            if (renderVersion != _renderVersion)
            {
                ReleaseCardItems(cardItems);
                return;
            }

            for (int i = 0; i < cardItems.Length; i++)
            {
                var cardItem = cardItems[i];
                if (cardItem == null) continue;

                cardItem.gameObject.SetActive(true);
                _spawnedItems.Add(cardItem);
            }

            UpdatePossibleRecipesText(matchingRecipes);
            Show();
            EventManager.Delegate(new HandRenderedEvent(_spawnedItems.Count));
        }

        private async UniTask ReplaceCardsInPlace(IReadOnlyList<IngredientScriptable> handCards, IReadOnlyList<RecipeScriptable> matchingRecipes)
        {
            if (_cardRoot == null || handCards == null)
            {
                ClearPendingReplaceTransition();
                await Render(handCards, matchingRecipes);
                return;
            }

            if (_spawnedItems.Count == 0 || _pendingRemovedCards == null || _pendingDrawnCards == null)
            {
                ClearPendingReplaceTransition();
                await Render(handCards, matchingRecipes);
                return;
            }

            int renderVersion = _renderVersion;
            float totalWidth = (handCards.Count - 1) * _cardSpacing;
            var removedIndexes = new List<int>(_pendingRemovedCards.Count);

            for (int i = 0; i < _pendingRemovedCards.Count; i++)
            {
                int removedIndex = FindItemIndex(_pendingRemovedCards[i], removedIndexes);
                if (removedIndex < 0) continue;

                var removedItem = _spawnedItems[removedIndex];
                if (removedItem != null)
                {
                    AddressableManager.ReleaseInstance(removedItem.gameObject);
                    _spawnedItems[removedIndex] = null;
                }

                removedIndexes.Add(removedIndex);
            }

            UpdatePossibleRecipesText(matchingRecipes);
            Show();

            int delayMilliseconds = Mathf.Max(0, Mathf.RoundToInt(_replaceRenderDelaySeconds * 1000f));
            if (delayMilliseconds > 0)
            {
                await UniTask.Delay(delayMilliseconds);
            }

            if (renderVersion != _renderVersion)
            {
                ClearPendingReplaceTransition();
                return;
            }

            removedIndexes.Sort();
            int replacementCount = Mathf.Min(_pendingDrawnCards.Count, removedIndexes.Count);
            var spawnTasks = new UniTask<CardItem>[replacementCount];

            for (int i = 0; i < replacementCount; i++)
            {
                int insertIndex = removedIndexes[i];
                float xPosition = insertIndex * _cardSpacing - totalWidth / 2f;
                spawnTasks[i] = SpawnCardItem(_pendingDrawnCards[i], xPosition, renderVersion);
            }

            var cardItems = await UniTask.WhenAll(spawnTasks);
            if (renderVersion != _renderVersion)
            {
                ReleaseCardItems(cardItems);
                ClearPendingReplaceTransition();
                return;
            }

            for (int i = 0; i < cardItems.Length; i++)
            {
                var cardItem = cardItems[i];
                if (cardItem == null) continue;

                int insertIndex = removedIndexes[i];
                cardItem.gameObject.SetActive(true);

                while (_spawnedItems.Count <= insertIndex)
                {
                    _spawnedItems.Add(null);
                }

                _spawnedItems[insertIndex] = cardItem;
            }

            ClearPendingReplaceTransition();
            EventManager.Delegate(new HandRenderedEvent(GetVisibleItemCount()));
        }

        private async UniTask<CardItem> SpawnCardItem(IngredientScriptable card, float xPosition, int renderVersion)
        {
            var cardItem = await AddressableManager.InstantiateAsync<CardItem>(GameConstants.CARD_ITEM_SCRIPTABLE_ADDRESSABLE_KEY, _cardRoot);
            if (cardItem == null) return null;

            cardItem.gameObject.SetActive(false);

            if (renderVersion != _renderVersion)
            {
                AddressableManager.ReleaseInstance(cardItem.gameObject);
                return null;
            }

            cardItem.transform.localPosition = new Vector3(xPosition, 0f, 0f);
            await cardItem.Init(card);

            if (renderVersion != _renderVersion)
            {
                AddressableManager.ReleaseInstance(cardItem.gameObject);
                return null;
            }

            return cardItem;
        }

        private static void ReleaseCardItems(IReadOnlyList<CardItem> cardItems)
        {
            if (cardItems == null) return;

            for (int i = 0; i < cardItems.Count; i++)
            {
                if (cardItems[i] == null) continue;
                AddressableManager.ReleaseInstance(cardItems[i].gameObject);
            }
        }

        public void Clear()
        {
            _renderVersion++;
            ClearPendingReplaceTransition();

            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                if (_spawnedItems[i] == null) continue;
                AddressableManager.ReleaseInstance(_spawnedItems[i].gameObject);
            }

            _spawnedItems.Clear();
            UpdatePossibleRecipesText(null);
        }

        public void Show()
        {
            GetContentRoot().SetActive(true);
        }

        public void Hide()
        {
            GetContentRoot().SetActive(false);
        }

        private void UpdatePossibleRecipesText(IReadOnlyList<RecipeScriptable> matchingRecipes)
        {
            if (_possibleRecipesText == null) return;

            if (matchingRecipes == null || matchingRecipes.Count == 0)
            {
                _possibleRecipesText.text = "No matches found";
                return;
            }

            var recipeText = string.Empty;
            for (int i = 0; i < matchingRecipes.Count; i++)
            {
                recipeText += $" {matchingRecipes[i].GetName()} - ({matchingRecipes[i].GetRecipeCategory()}) |";
            }

            _possibleRecipesText.text = recipeText.Remove(recipeText.Length - 1);
        }

        private void CachePendingReplaceTransition(IReadOnlyList<IngredientScriptable> removedCards, IReadOnlyList<IngredientScriptable> drawnCards)
        {
            _pendingRemovedCards = removedCards != null ? new List<IngredientScriptable>(removedCards) : null;
            _pendingDrawnCards = drawnCards != null ? new List<IngredientScriptable>(drawnCards) : null;
        }

        private bool HasPendingReplaceTransition()
        {
            return _pendingRemovedCards != null
                && _pendingDrawnCards != null
                && _pendingRemovedCards.Count > 0
                && _pendingDrawnCards.Count > 0;
        }

        private void ClearPendingReplaceTransition()
        {
            _pendingRemovedCards = null;
            _pendingDrawnCards = null;
        }

        private int FindItemIndex(IngredientScriptable card, IReadOnlyList<int> excludedIndexes)
        {
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                if (ContainsIndex(excludedIndexes, i)) continue;

                var item = _spawnedItems[i];
                if (item == null || item.CardScriptable != card) continue;

                return i;
            }

            return -1;
        }

        private int GetVisibleItemCount()
        {
            int count = 0;
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                if (_spawnedItems[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ContainsIndex(IReadOnlyList<int> indexes, int targetIndex)
        {
            if (indexes == null) return false;

            for (int i = 0; i < indexes.Count; i++)
            {
                if (indexes[i] == targetIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject GetContentRoot()
        {
            if (_contentRoot != null)
                return _contentRoot;

            if (_cardRoot != null)
                return _cardRoot.gameObject;

            return gameObject;
        }
    }
}