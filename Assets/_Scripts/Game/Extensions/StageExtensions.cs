using Game.Core.Enums;

namespace Game.Extensions
{
    public static class StageExtensions
    {
        public static string GetRoundDifficultyName(this ERoundDifficulty difficulty)
        {
            switch (difficulty)
            {
                case ERoundDifficulty.MiseEnPlace:
                    return "Mise En Place";
                case ERoundDifficulty.Service:
                    return "Service";
                case ERoundDifficulty.Rush:
                    return "Rush";
                default:
                    return "Unknown";
            }
        }

        public static int GetRoundDifficultyStarCount(this ERoundDifficulty difficulty)
        {
            switch (difficulty)
            {
                case ERoundDifficulty.MiseEnPlace:
                    return 1;
                case ERoundDifficulty.Service:
                    return 2;
                case ERoundDifficulty.Rush:
                    return 3;
                default:
                    return 0;
            }
        }
    }
}