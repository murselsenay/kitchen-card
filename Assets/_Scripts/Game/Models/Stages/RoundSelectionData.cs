using Game.Extensions;

namespace Game.Models.Stages
{
    public sealed class RoundSelectionData
    {
        public RoundSelectionData(RoundScriptable round)
        {
            Round = round;
            RoundId = round.GetId();
            DifficultyName = round.GetRoundDifficultyName();
            DifficultyStarCount = round.GetRoundDifficulty().GetRoundDifficultyStarCount();
            TargetScore = round.GetCustormerSatisfactionTargetPoint();
        }

        public RoundScriptable Round { get; }
        public string RoundId { get; }
        public string DifficultyName { get; }
        public int DifficultyStarCount { get; }
        public int TargetScore { get; }
    }
}