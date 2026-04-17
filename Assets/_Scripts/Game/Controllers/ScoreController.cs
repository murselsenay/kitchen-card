using Game.Core.Events;
using Game.Models.Cards;
using Game.Models.Play;
using Game.Models.Stages;
using Modules.Event.Managers;
using Modules.Logger;
using System.Collections.Generic;
using System.Text;

namespace Game.Controllers
{
    public static class ScoreController
    {
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

            var scoreResult = e.Resolution ?? PlayResolutionResolver.Resolve(e.PlayedCards, e.PlayedRecipe);
            if (scoreResult.TotalScore <= 0) return;

            _currentRoundScore += scoreResult.TotalScore;
            DebugLogger.Log(BuildScoreLog(scoreResult));
            DispatchRoundScoreUpdated(scoreResult.TotalScore, currentRound);

            if (!HasReachedCurrentRoundTarget()) return;

            _isCurrentRoundCompleted = true;
            StageController.CompleteRound(currentRound.GetId());
            EventManager.Delegate(new RoundTargetReachedEvent(currentRound, _currentRoundScore, _currentRoundTargetScore));
        }

        private static string BuildScoreLog(PlayResolutionResult scoreResult)
        {
            string ingredientBreakdown = BuildIngredientBreakdown(scoreResult.PlayedCards);

            if (scoreResult.Recipes.Count == 0)
            {
                return $"Ingredient play | Cards: {ingredientBreakdown} | Hand score: {scoreResult.TotalScore} | Total score: {_currentRoundScore}/{_currentRoundTargetScore}";
            }

            return $"Recipe combo play | Recipes: {BuildRecipeBreakdown(scoreResult.Recipes)} | Cards: {ingredientBreakdown} | Ingredient score: {scoreResult.IngredientScore} | Recipe bonus: {scoreResult.RecipeBonusScore} | Hand score: {scoreResult.TotalScore} | Total score: {_currentRoundScore}/{_currentRoundTargetScore}";
        }

        private static string BuildRecipeBreakdown(IReadOnlyList<ResolvedRecipePlay> recipes)
        {
            if (recipes == null || recipes.Count == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();

            for (int i = 0; i < recipes.Count; i++)
            {
                var resolvedRecipe = recipes[i];
                if (resolvedRecipe?.Recipe == null) continue;

                if (builder.Length > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(resolvedRecipe.Recipe.GetName());
                builder.Append(" [rarity:");
                builder.Append(resolvedRecipe.Recipe.GetRarity());
                builder.Append("=");
                builder.Append(resolvedRecipe.RarityBonus);
                builder.Append(", category:");
                builder.Append(resolvedRecipe.Recipe.GetRecipeCategory());
                builder.Append("=");
                builder.Append(resolvedRecipe.CategoryBonus);
                builder.Append(", bonus:");
                builder.Append(resolvedRecipe.BonusScore);
                builder.Append("]");
            }

            return builder.Length > 0 ? builder.ToString() : "None";
        }

        private static string BuildIngredientBreakdown(IReadOnlyList<IngredientScriptable> cards)
        {
            if (cards == null || cards.Count == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null) continue;

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(card.GetName());
                builder.Append(" [rarity:");
                builder.Append(card.GetRarity());
                builder.Append(", category:");
                builder.Append(card.GetCardCategory());
                builder.Append(", points:");
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

        private static void DispatchRoundScoreUpdated(int deltaScore, RoundScriptable round)
        {
            EventManager.Delegate(new RoundScoreUpdatedEvent(round, _currentRoundScore, _currentRoundTargetScore, deltaScore, HasReachedCurrentRoundTarget()));
        }
    }
}
