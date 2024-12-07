using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class PoolManager : Singleton<PoolManager>
    {
        private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();
        private Transform poolRoot;

        protected override void OnInit()
        {
            GameObject root = new GameObject("[ObjectPools]");
            GameObject.DontDestroyOnLoad(root);
            poolRoot = root.transform;
            DLLogger.Log($"[{GetType().Name}] initialized.");
        }

        public GameObject Spawn(string prefabPath, Vector3 position = default, Quaternion rotation = default)
        {
            if (!pools.ContainsKey(prefabPath))
            {
                CreatePool(prefabPath);
            }

            return pools[prefabPath].Spawn(position, rotation);
        }

        public void Despawn(GameObject obj)
        {
            foreach (var pool in pools.Values)
            {
                if (pool.Contains(obj))
                {
                    pool.Despawn(obj);
                    return;
                }
            }

            GameObject.Destroy(obj);
        }

        private void CreatePool(string prefabPath)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                DLLogger.LogError($"Failed to load prefab: {prefabPath}");
                return;
            }

            GameObject poolObject = new GameObject($"Pool-{prefab.name}");
            poolObject.transform.SetParent(poolRoot);
            ObjectPool pool = new ObjectPool(prefab, poolObject.transform);
            pools.Add(prefabPath, pool);
        }

        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
            pools.Clear();
        }

        private class ObjectPool
        {
            private GameObject prefab;
            private Transform parent;
            private Stack<GameObject> inactiveObjects = new Stack<GameObject>();
            private List<GameObject> activeObjects = new List<GameObject>();

            public ObjectPool(GameObject prefab, Transform parent)
            {
                this.prefab = prefab;
                this.parent = parent;
            }

            public GameObject Spawn(Vector3 position, Quaternion rotation)
            {
                GameObject obj;
                if (inactiveObjects.Count > 0)
                {
                    obj = inactiveObjects.Pop();
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                }
                else
                {
                    obj = GameObject.Instantiate(prefab, position, rotation, parent);
                }

                obj.SetActive(true);
                activeObjects.Add(obj);
                return obj;
            }

            public void Despawn(GameObject obj)
            {
                if (activeObjects.Remove(obj))
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(parent);
                    inactiveObjects.Push(obj);
                }
            }

            public bool Contains(GameObject obj)
            {
                return activeObjects.Contains(obj);
            }

            public void Clear()
            {
                foreach (var obj in activeObjects)
                {
                    GameObject.Destroy(obj);
                }
                activeObjects.Clear();

                while (inactiveObjects.Count > 0)
                {
                    GameObject obj = inactiveObjects.Pop();
                    GameObject.Destroy(obj);
                }
            }
        }
    }
}
