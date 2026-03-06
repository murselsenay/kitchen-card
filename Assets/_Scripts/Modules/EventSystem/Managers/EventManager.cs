using UnityEngine;
using Modules.Economy.Enums;
using Modules.PopupSystem.Components;
using System;

namespace Modules.EventSystem.Managers
{
    public static class EventManager
    {
        #region Events

        //Economy
        public static event Action<Transform, ECurrencyType, int> OnSpawnCurrencyRequest;
        public static event Action<ECurrencyType, int> OnCurrencyChanged;
        public static event Action<ECurrencyType> OnCurrencyRequestCompleted;

        //Popups
        public static event Action<BasePopup> OnPopupShowed;
        public static event Action<BasePopup> OnPopupClosed;

        // Timer
        public static event Action<long> OnTimerTick;

        #endregion

        #region Timer
        public static void DelegateTimerTick(long unixTime)
        {
            try
            {
                OnTimerTick?.Invoke(unixTime);
            }
            catch { }
        }

        #endregion

        #region Popup
        public static void DelegatePopupShowed(BasePopup popup)
        {
            try
            {
                OnPopupShowed?.Invoke(popup);
            }
            catch { }
        }

        public static void DelegatePopupClosed(BasePopup popup)
        {
            try
            {
                OnPopupClosed?.Invoke(popup);
            }
            catch { }
        }
        #endregion

        #region Currency
        public static void DelegateSpawnCurrencyRequest(Transform spawnPoint, ECurrencyType type, int amount)
        {
            try
            {
                OnSpawnCurrencyRequest?.Invoke(spawnPoint, type, amount);
            }
            catch { }
        }

        public static void DelegateCurrencyChanged(ECurrencyType type, int amount)
        {
            try
            {
                OnCurrencyChanged?.Invoke(type, amount);
            }
            catch { }
        }

        public static void DelegateCurrencyRequestCompleted(ECurrencyType type)
        {
            try
            {
                OnCurrencyRequestCompleted?.Invoke(type);
            }
            catch { }
        }
        #endregion

    }
}
