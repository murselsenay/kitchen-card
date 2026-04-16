using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Recipes;
using Game.Models.Scoring;
using Game.Models.Stages;
using Modules.Event.Managers;
using Modules.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Game.Controllers
{
    public static class ScoreController
    {
        private sealed class RecipeCandidate
        {
            public RecipeScriptable Recipe;
            public int BonusScore;
            public bool IsHinted;
            public List<int> Masks = new List<int>();
        }

        private sealed class ResolvedRecipeScore
        {
            public RecipeScriptable Recipe;
            public int UsedMask;
            public int BonusScore;
        }

        private sealed class PlayScoreResult
        {
            public int IngredientScore;
            public int RecipeBonusScore;
            public int TotalScore;
            public List<IngredientScriptable> Cards = new List<IngredientScriptable>();
            public List<ResolvedRecipeScore> Recipes = new List<ResolvedRecipeScore>();
        }

        private static int _currentRoundScore;
        private static int _currentRoundTargetScore;
        private static bool _isCurrentRoundCompleted;
        private static bool _isSubscribed;

        public static void Init()
        {
            EnsureSubscribed();

            if (StageController.GetCurrentRound() != null)
            {
                SetCurrentRound(StageController.GetCurrentRound());
            }
        }

        public static int GetCurrentRoundScore() => _currentRoundScore;
        public static int GetCurrentRoundTargetScore() => _currentRoundTargetScore;
        public static bool HasReachedCurrentRoundTarget() => _currentRoundTargetScore > 0 && _currentRoundScore >= _currentRoundTargetScore;

        private static void EnsureSubscribed()
        {
            if (_isSubscribed) return;

            EventManager.Subscribe<StageStartedEvent>(OnStageStarted);
            EventManager.Subscribe<StageRoundSelectedEvent>(OnStageRoundSelected);
            EventManager.Subscribe<SelectedCardsPlayedEvent>(OnSelectedCardsPlayed);

            _isSubscribed = true;
        }

        private static void OnStageStarted(StageStartedEvent e)
        {
            ResetCurrentRound();
            DispatchRoundScoreUpdated(0, null);
        }

        private static void OnStageRoundSelected(StageRoundSelectedEvent e)
        {
            SetCurrentRound(e.Round);
        }

        private static void OnSelectedCardsPlayed(SelectedCardsPlayedEvent e)
        {
            if (_isCurrentRoundCompleted) return;

            var currentRound = StageController.GetCurrentRound();
            if (currentRound == null) return;

            var scoreResult = ResolvePlayedScore(e.PlayedCards, e.PlayedRecipe);
            if (scoreResult.TotalScore <= 0) return;

            _currentRoundScore += scoreResult.TotalScore;
            DebugLogger.Log(BuildScoreLog(scoreResult));
            DispatchRoundScoreUpdated(scoreResult.TotalScore, currentRound);

            if (!HasReachedCurrentRoundTarget()) return;

            _isCurrentRoundCompleted = true;
            StageController.CompleteRound(currentRound.GetId());
            EventManager.Delegate(new RoundTargetReachedEvent(currentRound, _currentRoundScore, _currentRoundTargetScore));
        }

        private static PlayScoreResult ResolvePlayedScore(IReadOnlyList<IngredientScriptable> playedCards, RecipeScriptable playedRecipe)
        {
            var result = new PlayScoreResult();
            if (playedCards == null || playedCards.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < playedCards.Count; i++)
            {
                if (playedCards[i] == null) continue;
                result.Cards.Add(playedCards[i]);
            }

            result.IngredientScore = CalculateIngredientOnlyScore(result.Cards);

            var recipeCandidates = BuildRecipeCandidates(result.Cards, playedRecipe);
            result.Recipes = ResolveBestRecipeCombination(recipeCandidates);

            for (int i = 0; i < result.Recipes.Count; i++)
            {
                result.RecipeBonusScore += result.Recipes[i].BonusScore;
            }

            result.TotalScore = Mathf.Max(0, result.IngredientScore + result.RecipeBonusScore);
            return result;
        }

        private static string BuildScoreLog(PlayScoreResult scoreResult)
        {
            string ingredientBreakdown = BuildIngredientBreakdown(scoreResult.Cards);

            if (scoreResult.Recipes.Count == 0)
            {
                return $"Ingredient play | Cards: {ingredientBreakdown} | Hand score: {scoreResult.TotalScore} | Total score: {_currentRoundScore}/{_currentRoundTargetScore}";
            }

            return $"Recipe combo play | Recipes: {BuildRecipeBreakdown(scoreResult.Recipes)} | Cards: {ingredientBreakdown} | Ingredient score: {scoreResult.IngredientScore} | Recipe bonus: {scoreResult.RecipeBonusScore} | Hand score: {scoreResult.TotalScore} | Total score: {_currentRoundScore}/{_currentRoundTargetScore}";
        }

        private static List<RecipeCandidate> BuildRecipeCandidates(IReadOnlyList<IngredientScriptable> playedCards, RecipeScriptable playedRecipeHint)
        {
            var candidateRecipes = new List<RecipeCandidate>();
            if (playedCards == null || playedCards.Count == 0)
            {
                return candidateRecipes;
            }

            var uniqueRecipes = new List<RecipeScriptable>(DeckController.EvaluateCardsWithRecipe(new List<IngredientScriptable>(playedCards)));
            if (playedRecipeHint != null && !uniqueRecipes.Contains(playedRecipeHint))
            {
                var selectedIngredientTypes = new List<Game.Core.Enums.EIngredientType>(playedCards.Count);
                for (int i = 0; i < playedCards.Count; i++)
                {
                    if (playedCards[i] == null) continue;
                    selectedIngredientTypes.Add(playedCards[i].GetIngredientType());
                }

                if (playedRecipeHint.CheckIngredientsMet(selectedIngredientTypes))
                {
                    uniqueRecipes.Add(playedRecipeHint);
                }
            }

            var scoringConfig = ScoringConfig.Instance;
            for (int i = 0; i < uniqueRecipes.Count; i++)
            {
                var recipe = uniqueRecipes[i];
                if (recipe == null) continue;

                var candidate = new RecipeCandidate
                {
                    Recipe = recipe,
                    BonusScore = scoringConfig.GetRecipeRarityBonus(recipe.GetRarity()) + scoringConfig.GetRecipeCategoryBonus(recipe.GetRecipeCategory()),
                    IsHinted = recipe == playedRecipeHint
                };

                FillRecipeMasks(candidate.Masks, recipe, playedCards, 0, 0, new HashSet<int>());
                if (candidate.Masks.Count == 0) continue;

                candidateRecipes.Add(candidate);
            }

            candidateRecipes.Sort((left, right) =>
            {
                int hintedCompare = right.IsHinted.CompareTo(left.IsHinted);
                if (hintedCompare != 0) return hintedCompare;

                int bonusCompare = right.BonusScore.CompareTo(left.BonusScore);
                if (bonusCompare != 0) return bonusCompare;

                return string.Compare(left.Recipe.GetName(), right.Recipe.GetName(), StringComparison.Ordinal);
            });

            return candidateRecipes;
        }

        private static int CalculateIngredientOnlyScore(IReadOnlyList<IngredientScriptable> playedCards)
        {
            int totalScore = 0;
            if (playedCards == null) return totalScore;

            for (int i = 0; i < playedCards.Count; i++)
            {
                if (playedCards[i] == null) continue;
                totalScore += playedCards[i].GetPoint();
            }

            return Mathf.Max(0, totalScore);
        }

        private static List<ResolvedRecipeScore> ResolveBestRecipeCombination(IReadOnlyList<RecipeCandidate> candidates)
        {
            var memo = new Dictionary<long, int>();
            var selectedMasks = new Dictionary<long, int>();

            SolveRecipeBonus(candidates, 0, 0, memo, selectedMasks);

            var resolvedRecipes = new List<ResolvedRecipeScore>();
            int candidateIndex = 0;
            int usedMask = 0;

            while (candidateIndex < candidates.Count)
            {
                long stateKey = ComposeStateKey(candidateIndex, usedMask);
                if (!selectedMasks.TryGetValue(stateKey, out int chosenMask) || chosenMask == 0)
                {
                    candidateIndex++;
                    continue;
                }

                var candidate = candidates[candidateIndex];
                resolvedRecipes.Add(new ResolvedRecipeScore
                {
                    Recipe = candidate.Recipe,
                    UsedMask = chosenMask,
                    BonusScore = candidate.BonusScore
                });

                usedMask |= chosenMask;
                candidateIndex++;
            }

            return resolvedRecipes;
        }

        private static int SolveRecipeBonus(IReadOnlyList<RecipeCandidate> candidates, int candidateIndex, int usedMask, Dictionary<long, int> memo, Dictionary<long, int> selectedMasks)
        {
            if (candidateIndex >= candidates.Count)
            {
                return 0;
            }

            long stateKey = ComposeStateKey(candidateIndex, usedMask);
            if (memo.TryGetValue(stateKey, out int cachedScore))
            {
                return cachedScore;
            }

            int bestScore = SolveRecipeBonus(candidates, candidateIndex + 1, usedMask, memo, selectedMasks);
            int bestMask = 0;

            var candidate = candidates[candidateIndex];
            for (int i = 0; i < candidate.Masks.Count; i++)
            {
                int recipeMask = candidate.Masks[i];
                if ((recipeMask & usedMask) != 0) continue;

                int recipeScore = candidate.BonusScore + SolveRecipeBonus(candidates, candidateIndex + 1, usedMask | recipeMask, memo, selectedMasks);
                if (recipeScore <= bestScore) continue;

                bestScore = recipeScore;
                bestMask = recipeMask;
            }

            memo[stateKey] = bestScore;
            selectedMasks[stateKey] = bestMask;
            return bestScore;
        }

        private static long ComposeStateKey(int candidateIndex, int usedMask)
        {
            return ((long)candidateIndex << 32) | (uint)usedMask;
        }

        private static void FillRecipeMasks(List<int> masks, RecipeScriptable recipe, IReadOnlyList<IngredientScriptable> playedCards, int ingredientIndex, int currentMask, HashSet<int> visitedMasks)
        {
            if (recipe == null || playedCards == null)
            {
                return;
            }

            var ingredients = recipe.GetIngredients();
            if (ingredientIndex >= ingredients.Count)
            {
                if (visitedMasks.Add(currentMask))
                {
                    masks.Add(currentMask);
                }

                return;
            }

            var targetIngredientType = ingredients[ingredientIndex];
            for (int i = 0; i < playedCards.Count; i++)
            {
                if ((currentMask & (1 << i)) != 0) continue;

                var card = playedCards[i];
                if (card == null || card.GetIngredientType() != targetIngredientType) continue;

                FillRecipeMasks(masks, recipe, playedCards, ingredientIndex + 1, currentMask | (1 << i), visitedMasks);
            }
        }

        private static string BuildRecipeBreakdown(IReadOnlyList<ResolvedRecipeScore> recipes)
        {
            if (recipes == null || recipes.Count == 0)
            {
                return "None";
            }

            var scoringConfig = ScoringConfig.Instance;
            var builder = new StringBuilder();

            for (int i = 0; i < recipes.Count; i++)
            {
                var resolvedRecipe = recipes[i];
                if (resolvedRecipe?.Recipe == null) continue;

                int rarityBonus = scoringConfig.GetRecipeRarityBonus(resolvedRecipe.Recipe.GetRarity());
                int categoryBonus = scoringConfig.GetRecipeCategoryBonus(resolvedRecipe.Recipe.GetRecipeCategory());

                if (builder.Length > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(resolvedRecipe.Recipe.GetName());
                builder.Append(" [rarity:");
                builder.Append(resolvedRecipe.Recipe.GetRarity());
                builder.Append("=");
                builder.Append(rarityBonus);
                builder.Append(", category:");
                builder.Append(resolvedRecipe.Recipe.GetRecipeCategory());
                builder.Append("=");
                builder.Append(categoryBonus);
                builder.Append(", bonus:");
                builder.Append(resolvedRecipe.BonusScore);
                builder.Append("]");
            }

            return builder.Length > 0 ? builder.ToString() : "None";
        }

        private static string BuildIngredientBreakdown(IReadOnlyList<IngredientScriptable> playedCards)
        {
            if (playedCards == null || playedCards.Count == 0)
            {
                return "None";
            }

            var scoringConfig = ScoringConfig.Instance;
            var builder = new StringBuilder();

            for (int i = 0; i < playedCards.Count; i++)
            {
                var card = playedCards[i];
                if (card == null) continue;

                int rarityPoints = scoringConfig.GetIngredientRarityPoints(card.GetRarity());
                int categoryPoints = scoringConfig.GetIngredientCategoryPoints(card.GetCardCategory());

                if (builder.Length > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(card.GetName());
                builder.Append(" [");
                builder.Append(card.GetRarity());
                builder.Append(":");
                builder.Append(rarityPoints);
                builder.Append(", ");
                builder.Append(card.GetCardCategory());
                builder.Append(":");
                builder.Append(categoryPoints);
                builder.Append(", total:");
                builder.Append(card.GetPoint());
                builder.Append("]");
            }

            return builder.Length > 0 ? builder.ToString() : "None";
        }

        private static void SetCurrentRound(RoundScriptable round)
        {
            _currentRoundScore = 0;
            _currentRoundTargetScore = round != null ? round.GetCustormerSatisfactionTargetPoint() : 0;
            _isCurrentRoundCompleted = false;
            DispatchRoundScoreUpdated(0, round);
        }

        private static void ResetCurrentRound()
        {
            _currentRoundScore = 0;
            _currentRoundTargetScore = 0;
            _isCurrentRoundCompleted = false;
        }

        private static void DispatchRoundScoreUpdated(int gainedScore, RoundScriptable round)
        {
            EventManager.Delegate(new RoundScoreUpdatedEvent(
                round,
                _currentRoundScore,
                _currentRoundTargetScore,
                gainedScore,
                HasReachedCurrentRoundTarget()));
        }
    }
}