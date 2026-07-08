using UnityEngine;

namespace GGG.Tool.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private static readonly object LockObject = new object();

        public static T MainInstance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (LockObject)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;

                if (transform.parent == null)
                    DontDestroyOnLoad(gameObject);

                return;
            }

            if (_instance != this)
                Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}
