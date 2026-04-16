using Game.Models.Stages;

namespace Game.Core.Events
{
    public struct RoundSelectedEvent
    {
        public string RoundId;

        public RoundSelectedEvent(string roundId)
        {
            RoundId = roundId;
        }
    }

    public struct StageStartedEvent
    {
        public StageScriptable Stage;

        public StageStartedEvent(StageScriptable stage)
        {
            Stage = stage;
        }
    }

    public struct StageRoundSelectedEvent
    {
        public StageScriptable Stage;
        public RoundScriptable Round;

        public StageRoundSelectedEvent(StageScriptable stage, RoundScriptable round)
        {
            Stage = stage;
            Round = round;
        }
    }

    public struct RoundDeckCreatedEvent
    {
        public StageScriptable Stage;
        public RoundScriptable Round;
        public int DeckCardCount;

        public RoundDeckCreatedEvent(StageScriptable stage, RoundScriptable round, int deckCardCount)
        {
            Stage = stage;
            Round = round;
            DeckCardCount = deckCardCount;
        }
    }

    public struct RoundScoreUpdatedEvent
    {
        public RoundScriptable Round;
        public int CurrentScore;
        public int TargetScore;
        public int GainedScore;
        public bool HasReachedTarget;

        public RoundScoreUpdatedEvent(RoundScriptable round, int currentScore, int targetScore, int gainedScore, bool hasReachedTarget)
        {
            Round = round;
            CurrentScore = currentScore;
            TargetScore = targetScore;
            GainedScore = gainedScore;
            HasReachedTarget = hasReachedTarget;
        }
    }

    public struct RoundTargetReachedEvent
    {
        public RoundScriptable Round;
        public int CurrentScore;
        public int TargetScore;

        public RoundTargetReachedEvent(RoundScriptable round, int currentScore, int targetScore)
        {
            Round = round;
            CurrentScore = currentScore;
            TargetScore = targetScore;
        }
    }
}