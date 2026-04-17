using Game.Models.Cards;
using Game.Models.Play;
using Game.Models.Recipes;
using System.Collections.Generic;

namespace Game.Core.Events
{
    public struct AddSelectedCardEvent
    {
        public IngredientScriptable Card;

        public AddSelectedCardEvent(IngredientScriptable card)
        {
            Card = card;
        }
    }

    public struct RemoveSelectedCardEvent
    {
        public IngredientScriptable Card;

        public RemoveSelectedCardEvent(IngredientScriptable card)
        {
            Card = card;
        }
    }

    public struct CardSelectedEvent
    {
        public IngredientScriptable Card;

        public CardSelectedEvent(IngredientScriptable card)
        {
            Card = card;
        }
    }

    public struct CardDeselectedEvent
    {
        public IngredientScriptable Card;

        public CardDeselectedEvent(IngredientScriptable card)
        {
            Card = card;
        }
    }

    public struct DiscardSelectedCardsRequestEvent
    {
    }

    public struct PlaySelectedCardsRequestEvent
    {
    }

    public struct HandUpdatedEvent
    {
        public IReadOnlyList<IngredientScriptable> HandCards;
        public IReadOnlyList<RecipeScriptable> MatchingRecipes;

        public HandUpdatedEvent(IReadOnlyList<IngredientScriptable> handCards, IReadOnlyList<RecipeScriptable> matchingRecipes)
        {
            HandCards = handCards;
            MatchingRecipes = matchingRecipes;
        }
    }

    public struct HandRenderedEvent
    {
        public int HandCardCount;

        public HandRenderedEvent(int handCardCount)
        {
            HandCardCount = handCardCount;
        }
    }

    public struct PreviewRecipeRequestEvent
    {
        public RecipeScriptable Recipe;

        public PreviewRecipeRequestEvent(RecipeScriptable recipe)
        {
            Recipe = recipe;
        }
    }

    public struct RecipePreviewChangedEvent
    {
        public RecipeScriptable Recipe;
        public IReadOnlyList<IngredientScriptable> PreviewCards;

        public RecipePreviewChangedEvent(RecipeScriptable recipe, IReadOnlyList<IngredientScriptable> previewCards)
        {
            Recipe = recipe;
            PreviewCards = previewCards;
        }
    }

    public struct SelectedCardsDiscardedEvent
    {
        public IReadOnlyList<IngredientScriptable> DiscardedCards;
        public IReadOnlyList<IngredientScriptable> DrawnCards;
        public IReadOnlyList<int> RemovedCardIndexes;

        public SelectedCardsDiscardedEvent(IReadOnlyList<IngredientScriptable> discardedCards, IReadOnlyList<IngredientScriptable> drawnCards, IReadOnlyList<int> removedCardIndexes)
        {
            DiscardedCards = discardedCards;
            DrawnCards = drawnCards;
            RemovedCardIndexes = removedCardIndexes;
        }
    }

    public struct SelectedCardsPlayedEvent
    {
        public IReadOnlyList<IngredientScriptable> PlayedCards;
        public IReadOnlyList<IngredientScriptable> DrawnCards;
        public IReadOnlyList<int> RemovedCardIndexes;
        public RecipeScriptable PlayedRecipe;
        public PlayResolutionResult Resolution;

        public SelectedCardsPlayedEvent(IReadOnlyList<IngredientScriptable> playedCards, IReadOnlyList<IngredientScriptable> drawnCards, IReadOnlyList<int> removedCardIndexes, RecipeScriptable playedRecipe, PlayResolutionResult resolution)
        {
            PlayedCards = playedCards;
            DrawnCards = drawnCards;
            RemovedCardIndexes = removedCardIndexes;
            PlayedRecipe = playedRecipe;
            Resolution = resolution;
        }
    }
}