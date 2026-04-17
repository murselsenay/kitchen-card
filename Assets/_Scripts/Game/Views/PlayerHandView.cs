using Cysharp.Threading.Tasks;
using Game.Controllers;
using Game.Core.Constants;
using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Play;
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
        private sealed class PendingReplaceTransition
        {
            public List<IngredientScriptable> DrawnCards;
            public List<int> RemovedCardIndexes;
            public PlayResolutionResult Resolution;
        }

        private sealed class PreparedRecipeAnimation
        {
            public ResolvedRecipePlay Recipe;
            public List<CardItem> RecipeItems;
            public RecipeItemView RecipeItemView;
        }

        [SerializeField] private Transform _cardRoot;
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private TMP_Text _possibleRecipesText;
        [SerializeField] private List<RecipeItemView> _recipeItemPool = new List<RecipeItemView>(4);
        [SerializeField] private float _cardSpacing = 10f;
        [SerializeField] private float _recipeItemHorizontalSpacing = 3.5f;
        [SerializeField] private float _recipeItemVerticalSpacing = 3f;
        [SerializeField] private float _replaceRenderDelaySeconds = 0.12f;
        [SerializeField] private float _recipeItemVisibleSeconds = 0.28f;

        private readonly List<CardItem> _spawnedItems = new List<CardItem>();
        private readonly HashSet<RecipeItemView> _reservedRecipeItems = new HashSet<RecipeItemView>();
        private int _renderVersion;
        private PendingReplaceTransition _pendingReplaceTransition;
        private Vector3 _recipeItemLayoutCenterWorldPosition;
        private bool _hasRecipeItemLayoutCenterWorldPosition;

        private void Awake()
        {
            CacheRecipeItemLayoutCenter();
            HideAllRecipeItems();
        }

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
            CachePendingReplaceTransition(e.RemovedCardIndexes, e.DrawnCards, e.Resolution);
        }

        private void OnSelectedCardsDiscarded(SelectedCardsDiscardedEvent e)
        {
            CachePendingReplaceTransition(e.RemovedCardIndexes, e.DrawnCards, null);
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
            var pendingTransition = _pendingReplaceTransition;
            if (_cardRoot == null || handCards == null)
            {
                ClearPendingReplaceTransition();
                await Render(handCards, matchingRecipes);
                return;
            }

            if (_spawnedItems.Count == 0 || pendingTransition == null || pendingTransition.RemovedCardIndexes == null)
            {
                ClearPendingReplaceTransition();
                await Render(handCards, matchingRecipes);
                return;
            }

            int renderVersion = _renderVersion;
            float totalWidth = (handCards.Count - 1) * _cardSpacing;
            var removedIndexes = GetValidRemovedIndexes(pendingTransition.RemovedCardIndexes);
            await AnimatePendingTransitionAsync(pendingTransition, removedIndexes, renderVersion);

            if (renderVersion != _renderVersion)
            {
                ClearPendingReplaceTransition();
                return;
            }

            ReleaseItemsAtIndexes(removedIndexes);

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
            int replacementCount = Mathf.Min(pendingTransition.DrawnCards != null ? pendingTransition.DrawnCards.Count : 0, removedIndexes.Count);
            var spawnTasks = new UniTask<CardItem>[replacementCount];

            for (int i = 0; i < replacementCount; i++)
            {
                int insertIndex = removedIndexes[i];
                float xPosition = insertIndex * _cardSpacing - totalWidth / 2f;
                spawnTasks[i] = SpawnCardItem(pendingTransition.DrawnCards[i], xPosition, renderVersion);
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

            var spawnAnimations = new List<UniTask>(cardItems.Length);
            for (int i = 0; i < cardItems.Length; i++)
            {
                if (cardItems[i] == null) continue;
                spawnAnimations.Add(cardItems[i].PlaySpawnAsync());
            }

            if (spawnAnimations.Count > 0)
            {
                await UniTask.WhenAll(spawnAnimations);
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

            cardItem.SetSlotLocalPosition(new Vector3(xPosition, 0f, 0f));
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
            HideAllRecipeItems();

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

        private void CachePendingReplaceTransition(IReadOnlyList<int> removedCardIndexes, IReadOnlyList<IngredientScriptable> drawnCards, PlayResolutionResult resolution)
        {
            _pendingReplaceTransition = new PendingReplaceTransition
            {
                DrawnCards = drawnCards != null ? new List<IngredientScriptable>(drawnCards) : new List<IngredientScriptable>(),
                RemovedCardIndexes = removedCardIndexes != null ? new List<int>(removedCardIndexes) : new List<int>(),
                Resolution = resolution
            };
        }

        private bool HasPendingReplaceTransition()
        {
            return _pendingReplaceTransition != null
                && _pendingReplaceTransition.RemovedCardIndexes != null
                && _pendingReplaceTransition.RemovedCardIndexes.Count > 0;
        }

        private void ClearPendingReplaceTransition()
        {
            _pendingReplaceTransition = null;
        }

        private async UniTask AnimatePendingTransitionAsync(PendingReplaceTransition pendingTransition, IReadOnlyList<int> removedIndexes, int renderVersion)
        {
            if (removedIndexes == null || removedIndexes.Count == 0) return;

            if (pendingTransition?.Resolution != null)
            {
                await AnimatePlayedCardsAsync(pendingTransition.Resolution, removedIndexes, renderVersion);
            }
            else
            {
                await AnimateDiscardedCardsAsync(removedIndexes);
            }
        }

        private async UniTask AnimatePlayedCardsAsync(PlayResolutionResult resolution, IReadOnlyList<int> removedIndexes, int renderVersion)
        {
            var claimedIndexes = new HashSet<int>();
            var preparedRecipeAnimations = new List<PreparedRecipeAnimation>();
            var preloadTasks = new List<UniTask>();

            if (resolution != null)
            {
                for (int i = 0; i < resolution.Recipes.Count; i++)
                {
                    var recipe = resolution.Recipes[i];
                    var recipeItems = ClaimItems(recipe.ConsumedCards, removedIndexes, claimedIndexes);
                    if (recipeItems.Count == 0) continue;

                    var recipeItemView = ReserveRecipeItem();
                    if (recipeItemView == null)
                    {
                        Debug.LogWarning("No available RecipeItemView found in PlayerHandView recipe item pool.");
                    }
                    else
                    {
                        preloadTasks.Add(recipeItemView.PrepareAsync(recipe.Recipe));
                    }

                    preparedRecipeAnimations.Add(new PreparedRecipeAnimation
                    {
                        Recipe = recipe,
                        RecipeItems = recipeItems,
                        RecipeItemView = recipeItemView
                    });
                }
            }

            if (preloadTasks.Count > 0)
            {
                await UniTask.WhenAll(preloadTasks);
            }

            if (renderVersion != _renderVersion)
            {
                ReleasePreparedRecipeItems(preparedRecipeAnimations);
                return;
            }

            LayoutPreparedRecipeItems(preparedRecipeAnimations);

            var animationTasks = new List<UniTask>();

            for (int i = 0; i < preparedRecipeAnimations.Count; i++)
            {
                animationTasks.Add(AnimateRecipeGroupAsync(preparedRecipeAnimations[i], renderVersion));
            }

            if (resolution != null)
            {
                var looseItems = ClaimItems(resolution.LooseCards, removedIndexes, claimedIndexes);
                for (int i = 0; i < looseItems.Count; i++)
                {
                    animationTasks.Add(looseItems[i].PlayLooseConsumeAsync());
                }
            }

            for (int i = 0; i < removedIndexes.Count; i++)
            {
                int removedIndex = removedIndexes[i];
                if (claimedIndexes.Contains(removedIndex)) continue;

                var item = GetItemAtIndex(removedIndex);
                if (item == null) continue;

                claimedIndexes.Add(removedIndex);
                animationTasks.Add(item.PlayLooseConsumeAsync());
            }

            if (animationTasks.Count > 0)
            {
                await UniTask.WhenAll(animationTasks);
            }
        }

        private async UniTask AnimateDiscardedCardsAsync(IReadOnlyList<int> removedIndexes)
        {
            var animationTasks = new List<UniTask>();

            for (int i = 0; i < removedIndexes.Count; i++)
            {
                var item = GetItemAtIndex(removedIndexes[i]);
                if (item == null) continue;

                animationTasks.Add(item.PlayLooseConsumeAsync());
            }

            if (animationTasks.Count > 0)
            {
                await UniTask.WhenAll(animationTasks);
            }
        }

        private async UniTask AnimateRecipeGroupAsync(PreparedRecipeAnimation preparedRecipeAnimation, int renderVersion)
        {
            if (preparedRecipeAnimation?.Recipe == null || preparedRecipeAnimation.RecipeItems == null || preparedRecipeAnimation.RecipeItems.Count == 0) return;

            var recipeItemView = preparedRecipeAnimation.RecipeItemView;
            Vector3 mergePosition = recipeItemView != null
                ? recipeItemView.GetWorldPosition()
                : GetAverageWorldPosition(preparedRecipeAnimation.RecipeItems);

            var mergeTasks = new List<UniTask>(preparedRecipeAnimation.RecipeItems.Count);
            for (int i = 0; i < preparedRecipeAnimation.RecipeItems.Count; i++)
            {
                mergeTasks.Add(preparedRecipeAnimation.RecipeItems[i].PlayRecipeMergeAsync(mergePosition));
            }

            await UniTask.WhenAll(mergeTasks);

            if (renderVersion != _renderVersion) return;
            if (recipeItemView == null) return;

            if (renderVersion != _renderVersion)
            {
                ReleaseRecipeItem(recipeItemView);
                return;
            }

            recipeItemView.ShowPrepared();
            await recipeItemView.PlaySpawnAsync();

            int visibleMilliseconds = Mathf.Max(0, Mathf.RoundToInt(_recipeItemVisibleSeconds * 1000f));
            if (visibleMilliseconds > 0)
            {
                await UniTask.Delay(visibleMilliseconds);
            }

            if (renderVersion != _renderVersion)
            {
                ReleaseRecipeItem(recipeItemView);
                return;
            }

            await recipeItemView.PlayConsumeAsync();
            ReleaseRecipeItem(recipeItemView);
        }

        private void ReleaseRecipeItem(RecipeItemView recipeItemView)
        {
            if (recipeItemView == null) return;
            _reservedRecipeItems.Remove(recipeItemView);
            recipeItemView.HideImmediate();
        }

        private void CacheRecipeItemLayoutCenter()
        {
            if (_recipeItemPool == null || _recipeItemPool.Count == 0)
            {
                _hasRecipeItemLayoutCenterWorldPosition = false;
                _recipeItemLayoutCenterWorldPosition = Vector3.zero;
                return;
            }

            Vector3 positionTotal = Vector3.zero;
            int validCount = 0;

            for (int i = 0; i < _recipeItemPool.Count; i++)
            {
                if (_recipeItemPool[i] == null) continue;

                positionTotal += _recipeItemPool[i].transform.position;
                validCount++;
            }

            if (validCount == 0)
            {
                _hasRecipeItemLayoutCenterWorldPosition = false;
                _recipeItemLayoutCenterWorldPosition = Vector3.zero;
                return;
            }

            _recipeItemLayoutCenterWorldPosition = positionTotal / validCount;
            _hasRecipeItemLayoutCenterWorldPosition = true;
        }

        private void LayoutPreparedRecipeItems(IReadOnlyList<PreparedRecipeAnimation> preparedRecipeAnimations)
        {
            if (preparedRecipeAnimations == null || preparedRecipeAnimations.Count == 0)
            {
                return;
            }

            if (!_hasRecipeItemLayoutCenterWorldPosition)
            {
                CacheRecipeItemLayoutCenter();
            }

            var recipeItemViews = new List<RecipeItemView>(preparedRecipeAnimations.Count);
            for (int i = 0; i < preparedRecipeAnimations.Count; i++)
            {
                var recipeItemView = preparedRecipeAnimations[i]?.RecipeItemView;
                if (recipeItemView == null) continue;

                recipeItemViews.Add(recipeItemView);
            }

            for (int i = 0; i < recipeItemViews.Count; i++)
            {
                recipeItemViews[i].transform.position = _recipeItemLayoutCenterWorldPosition + GetRecipeItemLayoutOffset(recipeItemViews.Count, i);
            }
        }

        private Vector3 GetRecipeItemLayoutOffset(int recipeCount, int layoutIndex)
        {
            if (recipeCount <= 1)
            {
                return Vector3.zero;
            }

            if (recipeCount == 2)
            {
                return new Vector3((layoutIndex - 0.5f) * _recipeItemHorizontalSpacing, 0f, 0f);
            }

            if (recipeCount == 3)
            {
                return new Vector3((layoutIndex - 1f) * _recipeItemHorizontalSpacing, 0f, 0f);
            }

            if (recipeCount == 4)
            {
                int row = layoutIndex / 2;
                int column = layoutIndex % 2;
                float xOffset = (column == 0 ? -0.5f : 0.5f) * _recipeItemHorizontalSpacing;
                float yOffset = (row == 0 ? 0.5f : -0.5f) * _recipeItemVerticalSpacing;
                return new Vector3(xOffset, yOffset, 0f);
            }

            int genericColumnCount = Mathf.CeilToInt(Mathf.Sqrt(recipeCount));
            int genericRow = layoutIndex / genericColumnCount;
            int genericColumn = layoutIndex % genericColumnCount;
            float totalWidth = (genericColumnCount - 1) * _recipeItemHorizontalSpacing;
            float genericXOffset = genericColumn * _recipeItemHorizontalSpacing - totalWidth * 0.5f;
            float genericYOffset = -genericRow * _recipeItemVerticalSpacing;
            return new Vector3(genericXOffset, genericYOffset, 0f);
        }

        private void ReleasePreparedRecipeItems(IReadOnlyList<PreparedRecipeAnimation> preparedRecipeAnimations)
        {
            if (preparedRecipeAnimations == null) return;

            for (int i = 0; i < preparedRecipeAnimations.Count; i++)
            {
                ReleaseRecipeItem(preparedRecipeAnimations[i]?.RecipeItemView);
            }
        }

        private RecipeItemView ReserveRecipeItem()
        {
            if (_recipeItemPool == null || _recipeItemPool.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < _recipeItemPool.Count; i++)
            {
                var recipeItemView = _recipeItemPool[i];
                if (recipeItemView == null || _reservedRecipeItems.Contains(recipeItemView)) continue;

                _reservedRecipeItems.Add(recipeItemView);
                return recipeItemView;
            }

            return null;
        }

        private void HideAllRecipeItems()
        {
            if (_recipeItemPool == null) return;

            for (int i = 0; i < _recipeItemPool.Count; i++)
            {
                if (_recipeItemPool[i] == null) continue;
                _recipeItemPool[i].HideImmediate();
            }

            _reservedRecipeItems.Clear();
        }

        private List<CardItem> ClaimItems(IReadOnlyList<IngredientScriptable> cards, IReadOnlyList<int> removedIndexes, HashSet<int> claimedIndexes)
        {
            var claimedItems = new List<CardItem>();
            if (cards == null || removedIndexes == null || claimedIndexes == null)
            {
                return claimedItems;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                int claimedIndex = FindItemIndex(cards[i], removedIndexes, claimedIndexes);
                if (claimedIndex < 0) continue;

                claimedIndexes.Add(claimedIndex);
                var item = GetItemAtIndex(claimedIndex);
                if (item != null)
                {
                    claimedItems.Add(item);
                }
            }

            return claimedItems;
        }

        private int FindItemIndex(IngredientScriptable card, IReadOnlyList<int> candidateIndexes, HashSet<int> claimedIndexes)
        {
            if (card == null || candidateIndexes == null) return -1;

            for (int i = 0; i < candidateIndexes.Count; i++)
            {
                int candidateIndex = candidateIndexes[i];
                if (claimedIndexes != null && claimedIndexes.Contains(candidateIndex)) continue;

                var item = GetItemAtIndex(candidateIndex);
                if (item == null || item.CardScriptable != card) continue;

                return candidateIndex;
            }

            return -1;
        }

        private List<int> GetValidRemovedIndexes(IReadOnlyList<int> removedIndexes)
        {
            var validIndexes = new List<int>();
            if (removedIndexes == null) return validIndexes;

            for (int i = 0; i < removedIndexes.Count; i++)
            {
                int removedIndex = removedIndexes[i];
                if (removedIndex < 0 || removedIndex >= _spawnedItems.Count) continue;
                if (ContainsIndex(validIndexes, removedIndex)) continue;

                validIndexes.Add(removedIndex);
            }

            validIndexes.Sort();
            return validIndexes;
        }

        private void ReleaseItemsAtIndexes(IReadOnlyList<int> removedIndexes)
        {
            if (removedIndexes == null) return;

            for (int i = 0; i < removedIndexes.Count; i++)
            {
                int removedIndex = removedIndexes[i];
                if (removedIndex < 0 || removedIndex >= _spawnedItems.Count) continue;

                var removedItem = _spawnedItems[removedIndex];
                if (removedItem == null) continue;

                AddressableManager.ReleaseInstance(removedItem.gameObject);
                _spawnedItems[removedIndex] = null;
            }
        }

        private CardItem GetItemAtIndex(int index)
        {
            if (index < 0 || index >= _spawnedItems.Count) return null;
            return _spawnedItems[index];
        }

        private static Vector3 GetAverageWorldPosition(IReadOnlyList<CardItem> items)
        {
            if (items == null || items.Count == 0) return Vector3.zero;

            Vector3 totalPosition = Vector3.zero;
            int validItemCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;

                totalPosition += items[i].transform.position;
                validItemCount++;
            }

            return validItemCount > 0 ? totalPosition / validItemCount : Vector3.zero;
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