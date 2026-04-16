using UnityEngine;

namespace Modules.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
        }

        protected bool IsCurrentInstance => ReferenceEquals(_instance, this as T);

        protected virtual void Awake()
        {
            if (_instance != null && !ReferenceEquals(_instance, this as T))
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_instance, this as T))
            {
                _instance = null;
            }
        }
    }
}