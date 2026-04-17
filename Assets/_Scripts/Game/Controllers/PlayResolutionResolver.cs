using Game.Models.Cards;
using Game.Models.Play;
using Game.Models.Recipes;
using Game.Models.Scoring;
using System;
using System.Collections.Generic;

namespace Game.Controllers
{
    public static class PlayResolutionResolver
    {
        private sealed class RecipeCandidate
        {
            public RecipeScriptable Recipe;
            public int BonusScore;
            public int RarityBonus;
            public int CategoryBonus;
            public bool IsHinted;
            public List<int> Masks = new List<int>();
        }

        private sealed class RecipeOption
        {
            public RecipeCandidate Candidate;
            public int UsedMask;
        }

        private sealed class ResolvedRecipeMask
        {
            public RecipeCandidate Candidate;
            public int UsedMask;
        }

        public static PlayResolutionResult Resolve(IReadOnlyList<IngredientScriptable> playedCards, RecipeScriptable playedRecipeHint)
        {
            var result = new PlayResolutionResult();
            if (playedCards == null || playedCards.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < playedCards.Count; i++)
            {
                if (playedCards[i] == null) continue;
                result.PlayedCards.Add(playedCards[i]);
            }

            result.IngredientScore = CalculateIngredientOnlyScore(result.PlayedCards);

            var recipeCandidates = BuildRecipeCandidates(result.PlayedCards, playedRecipeHint);
            var recipeOptions = BuildRecipeOptions(recipeCandidates);
            var resolvedRecipes = ResolveBestRecipeCombination(recipeOptions);
            var consumedCardSet = new HashSet<IngredientScriptable>();

            for (int i = 0; i < resolvedRecipes.Count; i++)
            {
                var resolvedRecipe = resolvedRecipes[i];
                if (resolvedRecipe?.Candidate?.Recipe == null) continue;

                var recipePlay = new ResolvedRecipePlay
                {
                    Recipe = resolvedRecipe.Candidate.Recipe,
                    RarityBonus = resolvedRecipe.Candidate.RarityBonus,
                    CategoryBonus = resolvedRecipe.Candidate.CategoryBonus
                };

                for (int cardIndex = 0; cardIndex < result.PlayedCards.Count; cardIndex++)
                {
                    if ((resolvedRecipe.UsedMask & (1 << cardIndex)) == 0) continue;

                    var card = result.PlayedCards[cardIndex];
                    if (card == null) continue;

                    recipePlay.ConsumedCards.Add(card);
                    consumedCardSet.Add(card);
                }

                result.RecipeBonusScore += recipePlay.BonusScore;
                result.Recipes.Add(recipePlay);
            }

            for (int i = 0; i < result.PlayedCards.Count; i++)
            {
                var playedCard = result.PlayedCards[i];
                if (playedCard == null) continue;
                if (consumedCardSet.Contains(playedCard)) continue;

                result.LooseCards.Add(playedCard);
            }

            result.TotalScore = Math.Max(0, result.IngredientScore + result.RecipeBonusScore);
            return result;
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
                    RarityBonus = scoringConfig.GetRecipeRarityBonus(recipe.GetRarity()),
                    CategoryBonus = scoringConfig.GetRecipeCategoryBonus(recipe.GetRecipeCategory()),
                    IsHinted = recipe == playedRecipeHint
                };

                candidate.BonusScore = candidate.RarityBonus + candidate.CategoryBonus;
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

        private static List<RecipeOption> BuildRecipeOptions(IReadOnlyList<RecipeCandidate> candidates)
        {
            var recipeOptions = new List<RecipeOption>();
            if (candidates == null)
            {
                return recipeOptions;
            }

            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                var candidate = candidates[candidateIndex];
                if (candidate == null || candidate.Masks == null || candidate.Masks.Count == 0) continue;

                for (int maskIndex = 0; maskIndex < candidate.Masks.Count; maskIndex++)
                {
                    recipeOptions.Add(new RecipeOption
                    {
                        Candidate = candidate,
                        UsedMask = candidate.Masks[maskIndex]
                    });
                }
            }

            recipeOptions.Sort((left, right) =>
            {
                int hintedCompare = right.Candidate.IsHinted.CompareTo(left.Candidate.IsHinted);
                if (hintedCompare != 0) return hintedCompare;

                int bonusCompare = right.Candidate.BonusScore.CompareTo(left.Candidate.BonusScore);
                if (bonusCompare != 0) return bonusCompare;

                return string.Compare(left.Candidate.Recipe.GetName(), right.Candidate.Recipe.GetName(), StringComparison.Ordinal);
            });

            return recipeOptions;
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

            return Math.Max(0, totalScore);
        }

        private static List<ResolvedRecipeMask> ResolveBestRecipeCombination(IReadOnlyList<RecipeOption> recipeOptions)
        {
            var memo = new Dictionary<long, int>();
            var selectedMasks = new Dictionary<long, int>();

            SolveRecipeBonus(recipeOptions, 0, 0, memo, selectedMasks);

            var resolvedRecipes = new List<ResolvedRecipeMask>();
            int optionIndex = 0;
            int usedMask = 0;

            while (optionIndex < recipeOptions.Count)
            {
                long stateKey = ComposeStateKey(optionIndex, usedMask);
                if (!selectedMasks.TryGetValue(stateKey, out int chosenOptionMask) || chosenOptionMask == 0)
                {
                    optionIndex++;
                    continue;
                }

                var recipeOption = recipeOptions[optionIndex];
                resolvedRecipes.Add(new ResolvedRecipeMask
                {
                    Candidate = recipeOption.Candidate,
                    UsedMask = chosenOptionMask
                });

                usedMask |= chosenOptionMask;
                optionIndex++;
            }

            return resolvedRecipes;
        }

        private static int SolveRecipeBonus(IReadOnlyList<RecipeOption> recipeOptions, int optionIndex, int usedMask, Dictionary<long, int> memo, Dictionary<long, int> selectedMasks)
        {
            if (optionIndex >= recipeOptions.Count)
            {
                return 0;
            }

            long stateKey = ComposeStateKey(optionIndex, usedMask);
            if (memo.TryGetValue(stateKey, out int cachedScore))
            {
                return cachedScore;
            }

            int bestScore = SolveRecipeBonus(recipeOptions, optionIndex + 1, usedMask, memo, selectedMasks);
            int bestMask = 0;

            var recipeOption = recipeOptions[optionIndex];
            if ((recipeOption.UsedMask & usedMask) == 0)
            {
                int recipeScore = recipeOption.Candidate.BonusScore + SolveRecipeBonus(recipeOptions, optionIndex + 1, usedMask | recipeOption.UsedMask, memo, selectedMasks);
                if (recipeScore > bestScore)
                {
                    bestScore = recipeScore;
                    bestMask = recipeOption.UsedMask;
                }
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
    }
}