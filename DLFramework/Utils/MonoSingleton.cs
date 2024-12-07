using UnityEngine;
namespace com.dl.framework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static volatile T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    DLLogger.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            // 查找场景中是否已存在实例
                            instance = FindObjectOfType<T>();

                            if (instance == null)
                            {
                                GameObject go = new GameObject($"[{typeof(T).Name}]");
                                instance = go.AddComponent<T>();
                                DontDestroyOnLoad(go);

                                //DLLogger.Log($"[Singleton] Created instance of {typeof(T)}");
                            }
                        }
                    }
                }
                return instance;
            }
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                OnInit();
                isInitialized = true;
            }
        }

        private bool isInitialized = false;
        protected virtual void OnInit() { DLLogger.Log($"[{GetType().Name}] initialized."); }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
                //DLLogger.Log($"[Singleton] Instance '{typeof(T)}' set in Awake");
            }
            else if (instance != this)
            {
                DLLogger.LogWarning($"[Singleton] Second instance of '{typeof(T)}' detected and destroyed");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            string[] paths = UnityEditor.AssetDatabase.GetAssetPath(this).Split('/');
            if (paths.Length > 1)
            {
                DLLogger.LogError($"[Singleton] {typeof(T)} is an asset. Singleton components should not be saved as assets.");
            }
        }
#endif
    }
}