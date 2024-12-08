using UnityEngine;
using System.Collections.Generic;

namespace com.dl.framework
{
	/// <summary>
	/// 游戏物体对象池
	/// </summary>
	public class GameObjectPool
	{
		private readonly GameObject m_prefab;
		private readonly Transform m_root;
		private readonly Queue<GameObject> m_cache;
		private readonly int m_maxSize;
		private readonly HashSet<GameObject> m_spawnedObjects;

		public int CacheCount => m_cache.Count;

		public GameObjectPool(GameObject prefab, Transform root, int initSize = 0, int maxSize = 100)
		{
			m_prefab = prefab;
			m_root = root;
			m_maxSize = maxSize;
			m_cache = new Queue<GameObject>();
			m_spawnedObjects = new HashSet<GameObject>();

			// 预热对象池
			for (int i = 0; i < initSize; i++)
			{
				Recycle(CreateNew());
			}
		}

		public GameObject Spawn()
		{
			GameObject go = m_cache.Count > 0 ? m_cache.Dequeue() : CreateNew();
			go.SetActive(true);
			go.transform.SetParent(null);

			var poolable = go.GetComponent<IPoolable>();
			poolable?.OnSpawn();

			m_spawnedObjects.Add(go);
			return go;
		}

		public void Recycle(GameObject go)
		{
			if (go == null || m_cache.Count >= m_maxSize)
			{
				GameObject.Destroy(go);
				return;
			}

			var poolable = go.GetComponent<IPoolable>();
			poolable?.OnRecycle();

			go.SetActive(false);
			go.transform.SetParent(m_root);
			m_cache.Enqueue(go);
			m_spawnedObjects.Remove(go);
		}

		public bool Contains(GameObject go)
		{
			return m_spawnedObjects.Contains(go);
		}

		public void Clear()
		{
			while (m_cache.Count > 0)
			{
				GameObject.Destroy(m_cache.Dequeue());
			}

			foreach (var go in m_spawnedObjects)
			{
				if (go != null)
				{
					GameObject.Destroy(go);
				}
			}

			m_spawnedObjects.Clear();
		}

		private GameObject CreateNew()
		{
			GameObject go = GameObject.Instantiate(m_prefab, m_root);
			go.name = m_prefab.name;
			return go;
		}
	}
}
