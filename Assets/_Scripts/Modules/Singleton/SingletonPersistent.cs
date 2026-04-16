using UnityEngine;

namespace Modules.Singleton
{
    public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            if (IsCurrentInstance)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}