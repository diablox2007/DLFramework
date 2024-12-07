using UnityEngine;
using System;
using System.Collections.Generic;

namespace com.dl.framework
{
    /// <summary>
    /// 可池化对象接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 重置对象状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 回收时调用
        /// </summary>
        void OnRecycle();

        /// <summary>
        /// 获取时调用
        /// </summary>
        void OnSpawn();
    }


    /// <summary>
    /// 通用对象池
    /// </summary>
    public class ObjectPool<T> where T : class, IPoolable
    {
        private readonly Queue<T> m_cache;
        private readonly Func<T> m_createFunc;
        private readonly Action<T> m_destroyAction;
        private readonly int m_maxSize;

        public int CacheCount => m_cache.Count;

        public ObjectPool(Func<T> createFunc, Action<T> destroyAction = null, int initSize = 0, int maxSize = 100)
        {
            m_cache = new Queue<T>();
            m_createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            m_destroyAction = destroyAction;
            m_maxSize = maxSize;

            // 预热对象池
            for (int i = 0; i < initSize; i++)
            {
                Recycle(Create());
            }
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public T Spawn()
        {
            T element = m_cache.Count > 0 ? m_cache.Dequeue() : Create();
            element.OnSpawn();
            return element;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public void Recycle(T element)
        {
            if (element == null)
            {
                return;
            }

            if (m_cache.Count >= m_maxSize)
            {
                Destroy(element);
                return;
            }

            element.OnRecycle();
            element.Reset();
            m_cache.Enqueue(element);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            if (m_destroyAction != null)
            {
                while (m_cache.Count > 0)
                {
                    Destroy(m_cache.Dequeue());
                }
            }
            m_cache.Clear();
        }

        private T Create()
        {
            try
            {
                return m_createFunc();
            }
            catch (Exception e)
            {
                DLLogger.LogError($"Failed to create pooled object: {e}");
                return null;
            }
        }

        private void Destroy(T element)
        {
            m_destroyAction?.Invoke(element);
        }
    }
}
