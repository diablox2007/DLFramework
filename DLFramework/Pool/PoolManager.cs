using UnityEngine;
using System;
using System.Collections.Generic;

namespace com.dl.framework
{
    /// <summary>
    /// 对象池管理器
    /// </summary>
    public sealed class PoolManager : Singleton<PoolManager>
    {
        private readonly Dictionary<Type, object> m_typePools = new Dictionary<Type, object>();
        private readonly Dictionary<string, GameObjectPool> m_prefabPools = new Dictionary<string, GameObjectPool>();
        private Transform m_poolRoot;

        protected override void OnInit()
        {
            GameObject go = new GameObject("[PoolManager]");
            GameObject.DontDestroyOnLoad(go);
            m_poolRoot = go.transform;

            base.OnInit();
        }

        #region GameObject Pool

        /// <summary>
        /// 创建游戏物体对象池
        /// </summary>
        public GameObjectPool CreateGameObjectPool(string prefabPath, int initSize = 0, int maxSize = 100)
        {
            if (m_prefabPools.TryGetValue(prefabPath, out GameObjectPool pool))
            {
                return pool;
            }

            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                DLLogger.LogError($"Failed to load prefab: {prefabPath}");
                return null;
            }

            Transform poolRoot = new GameObject($"Pool_{prefab.name}").transform;
            poolRoot.SetParent(m_poolRoot);

            pool = new GameObjectPool(prefab, poolRoot, initSize, maxSize);
            m_prefabPools.Add(prefabPath, pool);
            return pool;
        }

        /// <summary>
        /// 获取游戏物体
        /// </summary>
        public GameObject SpawnGameObject(string prefabPath)
        {
            if (!m_prefabPools.TryGetValue(prefabPath, out GameObjectPool pool))
            {
                pool = CreateGameObjectPool(prefabPath);
                if (pool == null) return null;
            }
            return pool.Spawn();
        }

        /// <summary>
        /// 回收游戏物体
        /// </summary>
        public void RecycleGameObject(GameObject go)
        {
            if (go == null) return;

            var poolable = go.GetComponent<IPoolable>();
            if (poolable == null)
            {
                GameObject.Destroy(go);
                return;
            }

            foreach (var pool in m_prefabPools.Values)
            {
                if (pool.Contains(go))
                {
                    pool.Recycle(go);
                    return;
                }
            }

            GameObject.Destroy(go);
        }

        #endregion

        #region Generic Pool

        /// <summary>
        /// 创建泛型对象池
        /// </summary>
        public ObjectPool<T> CreatePool<T>(Func<T> createFunc, Action<T> destroyAction = null,
            int initSize = 0, int maxSize = 100) where T : class, IPoolable
        {
            Type type = typeof(T);
            if (m_typePools.TryGetValue(type, out object existingPool))
            {
                return existingPool as ObjectPool<T>;
            }

            var pool = new ObjectPool<T>(createFunc, destroyAction, initSize, maxSize);
            m_typePools.Add(type, pool);
            return pool;
        }

        /// <summary>
        /// 获取泛型对象池
        /// </summary>
        public ObjectPool<T> GetPool<T>() where T : class, IPoolable
        {
            if (m_typePools.TryGetValue(typeof(T), out object pool))
            {
                return pool as ObjectPool<T>;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in m_typePools.Values)
            {
                var methodInfo = pool.GetType().GetMethod("Clear");
                methodInfo?.Invoke(pool, null);
            }
            m_typePools.Clear();

            foreach (var pool in m_prefabPools.Values)
            {
                pool.Clear();
            }
            m_prefabPools.Clear();
        }
    }
}
