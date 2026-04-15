using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Game.Core.Constants;
using Game.Models.Progression;
using Game.Models.Stages;
using Modules.Logger;
using System.Collections.Generic;
using System.Linq;

namespace Game.Controllers
{
    public static class StageController
    {
        //Stage
        private static List<StageScriptable> _stages;
        private static List<StageProgressionData> _stageProgressions;
        private static StageScriptable _currentStage;

        //Round
        private static RoundScriptable _currentRound;
        public static async UniTask Init()
        {
            await LoadStages();
            StartStage();
        }

        #region Stages
        public static async UniTask LoadStages()
        {
            var stageScriptables = await AddressableManager.LoadAllAsync<StageScriptable>(GameConstants.STAGE_SCRIPTABLE_ADDRESSABLE_KEY);

            _stages = new List<StageScriptable>();
            _stageProgressions = new List<StageProgressionData>();

            foreach (var stage in stageScriptables)
            {
                _stages.Add(stage);
                _stageProgressions.Add(new StageProgressionData(stage.GetId()));
            }
        }
        public static void StartStage()
        {
            _currentStage = GetNextStage();
            _currentRound = GetNextRound();
            if (_currentStage == null)
            {
                DebugLogger.Log("No more stages to play.");
                return;
            }

            if (_currentRound == null)
            {
                DebugLogger.Log("No more round to play.");
                return;
            }
        }
        public static void CompleteStage()
        {
            _stageProgressions.Find(x => x.Id == _currentStage.GetId()).CompleteStage();
        }
        public static StageScriptable GetCurrentStage() => _currentStage;
        public static StageScriptable GetNextStage()
        {
            if (_stages == null || _stageProgressions == null) return null;

            foreach (var progression in _stageProgressions)
            {
                if (!progression.IsCompleted)
                {
                    var stage = _stages.FirstOrDefault(s => s.GetId() == progression.Id);
                    return stage;
                }
            }
            return null;
        }
        #endregion

        #region Rounds
        public static RoundScriptable GetCurrentRound() => _currentRound;
        public static void CompleteRound(string roundId)
        {
            _stageProgressions.Find(x => x.Id == _currentStage.GetId()).CompleteRound(roundId);
        }
        public static RoundScriptable GetNextRound()
        {
            if (_currentStage == null) return null;

            var stageProgression = _stageProgressions.FirstOrDefault(x => x.Id == _currentStage.GetId());
            foreach (var round in _currentStage.GetRounds())
            {
                bool exists = stageProgression.RoundProgressions.Any(x => x.Id == round.GetId());
                if (!exists)
                    return round;
            }
            return null;
        }
        #endregion
    }
}