using Cysharp.Threading.Tasks;
using Game.Core.Events;
using Game.Models.Stages;
using Game.Views;
using Modules.Event.Managers;
using System.Collections.Generic;

namespace Game.Controllers
{
    public static class RoundSelectionController
    {
        private static RoundSelectionPanelView _view;
        private static bool _isSubscribed;

        public static void Bind(RoundSelectionPanelView view)
        {
            _view = view;
            EnsureSubscribed();
            Refresh().Forget();
        }

        public static void Unbind(RoundSelectionPanelView view)
        {
            if (_view != view) return;

            _view.Clear();
            _view = null;
        }

        private static void EnsureSubscribed()
        {
            if (_isSubscribed) return;

            EventManager.Subscribe<RoundSelectedEvent>(OnRoundSelected);
            EventManager.Subscribe<StageStartedEvent>(OnStageStarted);
            EventManager.Subscribe<StageRoundSelectedEvent>(OnStageRoundSelected);
            _isSubscribed = true;
        }

        private static void OnStageStarted(StageStartedEvent e)
        {
            Refresh().Forget();
        }

        private static void OnStageRoundSelected(StageRoundSelectedEvent e)
        {
            _view?.Hide();
        }

        public static async UniTask Refresh()
        {
            var view = _view;
            if (view == null) return;

            var rounds = StageController.GetSelectableRounds();
            if (rounds.Count == 0)
            {
                view.Clear();
                view.Hide();
                return;
            }

            var items = new List<RoundSelectionData>(rounds.Count);
            foreach (var round in rounds)
            {
                items.Add(new RoundSelectionData(round));
            }

            await view.Render(items);

            if (_view != view) return;

            view.Show();
        }

        private static void OnRoundSelected(RoundSelectedEvent e)
        {
            if (string.IsNullOrEmpty(e.RoundId)) return;

            StageController.SelectRound(e.RoundId);
        }
    }
}