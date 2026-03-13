using Modules.Economy.Enums;
using UnityEngine;

namespace Modules.Economy.Events
{
    public struct CurrencyRequestCompletedEvent
    {
        public ECurrencyType CurrencyType { get; }
        public CurrencyRequestCompletedEvent(ECurrencyType currencyType)
        {
            CurrencyType = currencyType;
        }
    }

    public struct CurrencyChangedEvent
    {
        public ECurrencyType CurrencyType { get; }
        public int Amount { get; }
        public CurrencyChangedEvent(ECurrencyType currencyType, int amount)
        {
            CurrencyType = currencyType;
            Amount = amount;
        }
    }

    public struct SpawnCurrencyRequestEvent
    {
        public ECurrencyType CurrencyType { get; }
        public int Amount { get; }
        public Transform SpawnPoint { get; }
        public SpawnCurrencyRequestEvent(Transform spawnPoint, ECurrencyType currencyType, int amount)
        {
            SpawnPoint = spawnPoint;
            CurrencyType = currencyType;
            Amount = amount;
        }
    }
}