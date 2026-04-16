using Cysharp.Threading.Tasks;
using Modules.AdressableSystem.Constants;
using Modules.Logger;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Modules.AdressableSystem
{
    public static class AddressableManager
    {
        #region Private Fields
        private static readonly Dictionary<string, AsyncOperationHandle> _handles = new();
        #endregion

        #region Getters
        public static async UniTask<T> LoadAsync<T>(string address) where T : Object
        {
            if (_handles.TryGetValue(address, out var cached))
                return cached.Convert<T>().Result;

            var handle = Addressables.LoadAssetAsync<T>(address);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _handles[address] = handle;
                return handle.Result;
            }

            DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_LOAD_FAILED, address);
            return null;
        }

        public static async UniTask<List<T>> LoadAllAsync<T>(string label) where T : Object
        {
            if (_handles.TryGetValue(label, out var cached))
                return cached.Convert<IList<T>>().Result.ToList();

            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _handles[label] = handle;
                return handle.Result.ToList();
            }

            DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_LOAD_ALL_FAILED, label);
            return new List<T>();
        }
        #endregion
        
        #region Instantiate
        public static async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(address, parent);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;

            DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_INSTANTIATE_FAILED, address);
            return null;
        }

        public static async UniTask<TComponent> InstantiateAsync<TComponent>(string address, Transform parent = null) where TComponent : Component
        {
            var handle = Addressables.InstantiateAsync(address, parent);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_INSTANTIATE_FAILED, address);
                return null;
            }

            var go = handle.Result;
            if (go == null)
            {
                DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_INSTANTIATE_RETURN_NULL, address);
                return null;
            }

            var comp = go.GetComponent<TComponent>();
            if (comp != null) return comp;

            Addressables.ReleaseInstance(go);
            DebugLogger.LogErrorFormat(AddressableLogMessages.ADDRESSABLE_COMPONENT_NOT_FOUND, address, typeof(TComponent).FullName);
            return null;
        }
        #endregion

        #region Release
        public static void Release(string address)
        {
            if (!_handles.TryGetValue(address, out var handle)) return;
            Addressables.Release(handle);
            _handles.Remove(address);
        }

        public static void ReleaseAll()
        {
            foreach (var handle in _handles.Values)
                Addressables.Release(handle);
            _handles.Clear();
        }

        public static void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;
            Addressables.ReleaseInstance(instance);
        }
        #endregion
    }
}

