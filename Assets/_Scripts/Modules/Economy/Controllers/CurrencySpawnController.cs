using UnityEngine;
using Cysharp.Threading.Tasks;
using Modules.ObjectPoolSystem;
using Modules.Economy.Enums;
using Modules.Economy.Managers;
using NaughtyAttributes;
using Modules.PopupSystem.Managers;
using Modules.Event.Managers;
using Modules.Economy.Events;
using Modules.Economy.Constants;

namespace Modules.Economy.Controllers
{
    public class CurrencySpawnController : MonoBehaviour
    {
        [Header("Spawn Settings")]
        private ECurrencyType _currencyType = ECurrencyType.Gold;
        private Transform _spawnPoint;
        [SerializeField] private int _spawnDelayMs = 40;

        [Button]
        public void ShowPopup()
        {
            PopupManager.ShowPopup(Modules.PopupSystem.Enums.EPopup.Jobs);
        }
        private void OnEnable()
        {
            EventManager.Subscribe<SpawnCurrencyRequestEvent>(OnSpawnRequest);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<SpawnCurrencyRequestEvent>(OnSpawnRequest);
        }

        private void OnSpawnRequest(SpawnCurrencyRequestEvent e)
        {
            if (e.Amount <= 0) return;

            _currencyType = e.CurrencyType;
            _spawnPoint = e.SpawnPoint;

            CurrencyManager.Add(_currencyType, e.Amount);

            SpawnFromPoolAsync(e.Amount).Forget();
        }

        private async UniTaskVoid SpawnFromPoolAsync(int amount)
        {
            if (_spawnPoint == null) return;

            string key = CurrencyKeys.GetKeyForCurrency(_currencyType);

            for (int i = 0; i < amount; i++)
            {
                CurrencyObjectController obj = null;
                try
                {
                    obj = await ObjectPool.GetObjectAsync<CurrencyObjectController>(null, key);
                    obj.Initialize(amount);
                }
                catch { }

                if (obj != null)
                {
                    obj.transform.position = _spawnPoint.position + Vector3.up * 0.5f;
                    try { obj.Activate(); } catch { }
                }

                await UniTask.Delay(_spawnDelayMs);
            }
        }
    }
}

