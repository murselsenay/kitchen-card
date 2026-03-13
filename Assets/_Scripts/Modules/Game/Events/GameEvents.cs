using Modules.Game.Scriptables.Card;

namespace Modules.Game.Events
{
    public struct AddSelectedCardEvent
    {
        public CardScriptable Card;
        public AddSelectedCardEvent(CardScriptable card)
        {
            Card = card;
        }
    }

    public struct RemoveSelectedCardEvent
    {
        public CardScriptable Card;
        public RemoveSelectedCardEvent(CardScriptable card)
        {
            Card = card;
        }
    }

    public struct CardSelectedEvent
    {
        public CardScriptable Card;
        public CardSelectedEvent(CardScriptable card)
        {
            Card = card;
        }
    }

    public struct CardDeselectedEvent
    {
        public CardScriptable Card;
        public CardDeselectedEvent(CardScriptable card)
        {
            Card = card;
        }
    }
}