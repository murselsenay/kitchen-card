using Cysharp.Threading.Tasks;
using Game.Controllers;
using UnityEngine;

namespace Game.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private static UniTask _initializationTask;
        private static bool _isInitialized;
        private static bool _isInitializing;

        private async void Start()
        {
            await InitializeAsync();
        }

        public static UniTask InitializeAsync()
        {
            if (_isInitialized)
            {
                return UniTask.CompletedTask;
            }

            if (_isInitializing)
            {
                return _initializationTask;
            }

            _initializationTask = InitializeInternal();
            return _initializationTask;
        }

        private static async UniTask InitializeInternal()
        {
            _isInitializing = true;

            await CardController.Init();
            await RecipeController.Init();
            ScoreController.Init();
            await StageController.Init();
            await DeckController.Init();
            HandController.Init();

            _isInitialized = true;
            _isInitializing = false;
        }
    }
}