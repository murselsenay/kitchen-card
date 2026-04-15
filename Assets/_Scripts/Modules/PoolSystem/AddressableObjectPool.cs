using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Modules.AdressableSystem;
using Modules.Logger;
using Modules.AdressableSystem.Constants;

namespace Modules.PoolSystem
{
    public static class AddressableObjectPool
    {
        #region Private Fields
        private static readonly Dictionary<string, Stack<GameObject>> _available = new();
        private static readonly Dictionary<GameObject, string> _instanceKeys = new();
        #endregion

        #region Public Methods
        public static async UniTask<GameObject> GetAsync(string key, Transform parent = null)
        {
            if (_available.TryGetValue(key, out var stack) && stack.Count > 0)
            {
                var pooled = stack.Pop();
                pooled.transform.SetParent(parent, worldPositionStays: false);
                pooled.SetActive(true);
                return pooled;
            }

            var go = await AddressableManager.InstantiateAsync(key, parent);
            if (go != null) _instanceKeys[go] = key;
            else DebugLogger.LogErrorFormat(AddressableLogMessages.POOL_FAILED, key);
            return go;
        }

        public static void Return(GameObject go)
        {
            if (go == null || !go.activeSelf) return;

            if (!_instanceKeys.TryGetValue(go, out var key))
            {
                Object.Destroy(go);
                return;
            }

            go.SetActive(false);
            go.transform.SetParent(null, worldPositionStays: false);

            if (!_available.ContainsKey(key))
                _available[key] = new Stack<GameObject>();

            _available[key].Push(go);
        }

        public static void Clear()
        {
            foreach (var stack in _available.Values)
                while (stack.Count > 0)
                    if (stack.Pop() is { } go && go != null)
                        Object.Destroy(go);

            _available.Clear();
            _instanceKeys.Clear();
        }
        #endregion
    }
}
