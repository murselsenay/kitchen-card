using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Modules.Logger;

namespace Modules.Event.Managers
{
    public static class EventManager
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<Type, HashSet<Delegate>> _subscribers = new();
        private static readonly ConcurrentQueue<List<Delegate>> _delegateListPool = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<T>(Action<T> listener)
        {
            if (listener == null) return;

            lock (_lock)
            {
                var type = typeof(T);
                if (!_subscribers.TryGetValue(type, out var hashSet))
                {
                    hashSet = new HashSet<Delegate>(4);
                    _subscribers[type] = hashSet;
                }
                hashSet.Add(listener);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubscribeOnce<T>(Action<T> listener)
        {
            if (listener == null) return;

            lock (_lock)
            {
                var type = typeof(T);
                if (!_subscribers.TryGetValue(type, out var hashSet))
                {
                    hashSet = new HashSet<Delegate>(4);
                    _subscribers[type] = hashSet;
                }
                else
                {
                    var target = listener.Target;
                    var declaringType = listener.Method.DeclaringType;
                    Delegate existing = null;

                    foreach (var d in hashSet)
                    {
                        if (target != null)
                        {
                            if (d.Target == target) { existing = d; break; }
                        }
                        else
                        {
                            if (d.Target == null && d.Method.DeclaringType == declaringType) { existing = d; break; }
                        }
                    }

                    if (existing != null)
                        hashSet.Remove(existing);
                }

                hashSet.Add(listener);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unsubscribe<T>(Action<T> listener)
        {
            if (listener == null) return;

            lock (_lock)
            {
                var type = typeof(T);
                if (_subscribers.TryGetValue(type, out var hashSet))
                {
                    hashSet.Remove(listener);
                    if (hashSet.Count == 0)
                        _subscribers.Remove(type);
                }
            }
        }

        public static void Delegate<T>(T evt)
        {
            List<Delegate> listenersToInvoke = null;

            lock (_lock)
            {
                var type = typeof(T);
                if (_subscribers.TryGetValue(type, out var hashSet) && hashSet.Count > 0)
                {
                    if (!_delegateListPool.TryDequeue(out listenersToInvoke))
                        listenersToInvoke = new List<Delegate>(hashSet.Count);
                    else
                    {
                        listenersToInvoke.Clear();
                        if (listenersToInvoke.Capacity < hashSet.Count)
                            listenersToInvoke.Capacity = hashSet.Count;
                    }

                    foreach (var listener in hashSet)
                        listenersToInvoke.Add(listener);
                }
            }

            if (listenersToInvoke != null && listenersToInvoke.Count > 0)
            {
                for (int i = 0; i < listenersToInvoke.Count; i++)
                {
                    try
                    {
                        ((Action<T>)listenersToInvoke[i]).Invoke(evt);
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.LogException(ex);
                    }
                }

                listenersToInvoke.Clear();
                if (listenersToInvoke.Capacity < 100)
                    _delegateListPool.Enqueue(listenersToInvoke);
            }
        }

        public static void DelegateNextFrame<T>(T evt)
        {
            DelegateNextFrameAsync(evt).Forget();
        }

        private static async UniTaskVoid DelegateNextFrameAsync<T>(T evt)
        {
            await UniTask.NextFrame();
            Delegate(evt);
        }

        public static async UniTask DelegateIf<T>(T evt, Func<T, UniTask<bool>> predicate)
        {
            if (await predicate(evt))
            {
                Delegate(evt);
            }
        }

        public static async UniTask WaitForEvent<T>(CancellationToken token = default)
        {
            var tcs = new UniTaskCompletionSource();

            void handler(T e)
            {
                Unsubscribe((Action<T>)handler);
                tcs.TrySetResult();
            }

            Subscribe<T>(handler);

            try
            {
                await tcs.Task.AttachExternalCancellation(token);
            }
            finally
            {
                Unsubscribe((Action<T>)handler);
            }
        }

        public static async UniTask WaitForEvent<T>(Func<T, bool> predicate, CancellationToken token = default)
        {
            var tcs = new UniTaskCompletionSource();

            void handler(T e)
            {
                if (predicate(e))
                {
                    Unsubscribe((Action<T>)handler);
                    tcs.TrySetResult();
                }
            }

            Subscribe<T>(handler);

            try
            {
                await tcs.Task.AttachExternalCancellation(token);
            }
            finally
            {
                Unsubscribe((Action<T>)handler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Delegate<T>() where T : struct
        {
            Delegate(default(T));
        }

        public static void ClearAll()
        {
            lock (_lock)
            {
                _subscribers.Clear();

                while (_delegateListPool.TryDequeue(out _)) { }
            }
        }
    }
}
