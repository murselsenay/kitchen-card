using System.Collections.Generic;

namespace Game.Models.Progression
{
    public struct StageProgressionData
    {
        public string Id;
        public bool IsCompleted;
        public List<RoundProgressionData> RoundProgressions;
        public StageProgressionData(string id)
        {
            Id = id;
            IsCompleted = false;
            RoundProgressions = new List<RoundProgressionData>();
        }
        public void CompleteRound(string id)
        {
            if (RoundProgressions.Exists(x => x.Id == id)) return;
            RoundProgressions.Add(new RoundProgressionData(id));
        }
        public void CompleteStage()
        {
            IsCompleted = true;
        }
    }
}