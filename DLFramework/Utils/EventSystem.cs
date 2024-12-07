using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class EventSystem : Singleton<EventSystem>
    {
        // 普通事件字典
        private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();

        // 临时事件字典（只触发一次）
        private Dictionary<string, Delegate> onceEventDict = new Dictionary<string, Delegate>();

        // 延迟删除列表
        private List<KeyValuePair<string, Delegate>> delayRemoveList = new List<KeyValuePair<string, Delegate>>();

        // 是否正在分发事件
        private bool isDispatchingEvent = false;

        protected override void OnInit()
        {
            base.OnInit();
        }

        #region 添加监听
        public void AddListener(string eventName, Action handler)
        {
            AddEventListener(eventName, handler, eventDict);
        }

        public void AddListener<T>(string eventName, Action<T> handler)
        {
            AddEventListener(eventName, handler, eventDict);
        }

        public void AddListener<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            AddEventListener(eventName, handler, eventDict);
        }

        public void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            AddEventListener(eventName, handler, eventDict);
        }

        // 添加一次性监听
        public void AddListenerOnce(string eventName, Action handler)
        {
            AddEventListener(eventName, handler, onceEventDict);
        }

        public void AddListenerOnce<T>(string eventName, Action<T> handler)
        {
            AddEventListener(eventName, handler, onceEventDict);
        }
        #endregion

        #region 移除监听
        public void RemoveListener(string eventName, Action handler)
        {
            RemoveEventListener(eventName, handler, eventDict);
        }

        public void RemoveListener<T>(string eventName, Action<T> handler)
        {
            RemoveEventListener(eventName, handler, eventDict);
        }

        public void RemoveListener<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            RemoveEventListener(eventName, handler, eventDict);
        }

        public void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler)
        {
            RemoveEventListener(eventName, handler, eventDict);
        }
        #endregion

        #region 触发事件
        public void Trigger(string eventName)
        {
            TriggerEvent(eventName, null, typeof(Action));
        }

        public void Trigger<T>(string eventName, T arg)
        {
            TriggerEvent(eventName, new object[] { arg }, typeof(Action<T>));
        }

        public void Trigger<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            TriggerEvent(eventName, new object[] { arg1, arg2 }, typeof(Action<T1, T2>));
        }

        public void Trigger<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
        {
            TriggerEvent(eventName, new object[] { arg1, arg2, arg3 }, typeof(Action<T1, T2, T3>));
        }
        #endregion

        #region 私有方法
        private void AddEventListener(string eventName, Delegate handler, Dictionary<string, Delegate> dict)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                DLLogger.LogError("Event name cannot be null or empty");
                return;
            }

            if (handler == null)
            {
                DLLogger.LogError("Handler cannot be null");
                return;
            }

            if (!dict.ContainsKey(eventName))
            {
                dict[eventName] = handler;
            }
            else
            {
                dict[eventName] = Delegate.Combine(dict[eventName], handler);
            }
        }

        private void RemoveEventListener(string eventName, Delegate handler, Dictionary<string, Delegate> dict)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return;
            }

            if (isDispatchingEvent)
            {
                delayRemoveList.Add(new KeyValuePair<string, Delegate>(eventName, handler));
                return;
            }

            if (dict.ContainsKey(eventName))
            {
                dict[eventName] = Delegate.Remove(dict[eventName], handler);
                if (dict[eventName] == null)
                {
                    dict.Remove(eventName);
                }
            }
        }

        private void TriggerEvent(string eventName, object[] args, Type delegateType)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            isDispatchingEvent = true;

            // 触发普通事件
            if (eventDict.ContainsKey(eventName))
            {
                Delegate d = eventDict[eventName];
                if (d != null)
                {
                    try
                    {
                        Delegate[] delegates = d.GetInvocationList();
                        foreach (Delegate handler in delegates)
                        {
                            if (handler.GetType() == delegateType)
                            {
                                handler.DynamicInvoke(args);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        DLLogger.LogError($"Error triggering event {eventName}: {e.Message} { e.StackTrace}");
                    }
                }
            }

            // 触发一次性事件
            if (onceEventDict.ContainsKey(eventName))
            {
                Delegate d = onceEventDict[eventName];
                if (d != null)
                {
                    try
                    {
                        Delegate[] delegates = d.GetInvocationList();
                        foreach (Delegate handler in delegates)
                        {
                            if (handler.GetType() == delegateType)
                            {
                                handler.DynamicInvoke(args);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        DLLogger.LogError($"Error triggering once event {eventName}: {e.Message} { e.StackTrace}");
                    }
                }
                onceEventDict.Remove(eventName);
            }

            isDispatchingEvent = false;
            ProcessDelayRemoveList();
        }

        private void ProcessDelayRemoveList()
        {
            foreach (var pair in delayRemoveList)
            {
                RemoveEventListener(pair.Key, pair.Value, eventDict);
            }
            delayRemoveList.Clear();
        }
        #endregion

        #region 清理方法
        public void RemoveAllListeners()
        {
            if (isDispatchingEvent)
            {
                DLLogger.LogWarning("Cannot remove all listeners while dispatching event");
                return;
            }

            eventDict.Clear();
            onceEventDict.Clear();
            delayRemoveList.Clear();
        }

        public void RemoveAllListeners(string eventName)
        {
            if (isDispatchingEvent)
            {
                DLLogger.LogWarning("Cannot remove all listeners while dispatching event");
                return;
            }

            if (eventDict.ContainsKey(eventName))
            {
                eventDict.Remove(eventName);
            }
            if (onceEventDict.ContainsKey(eventName))
            {
                onceEventDict.Remove(eventName);
            }
        }
        #endregion

        #region 调试方法
        public bool HasListener(string eventName)
        {
            return eventDict.ContainsKey(eventName) || onceEventDict.ContainsKey(eventName);
        }

        public int GetListenerCount(string eventName)
        {
            int count = 0;
            if (eventDict.ContainsKey(eventName))
            {
                count += eventDict[eventName].GetInvocationList().Length;
            }
            if (onceEventDict.ContainsKey(eventName))
            {
                count += onceEventDict[eventName].GetInvocationList().Length;
            }
            return count;
        }

        public void LogEventStatus()
        {
            DLLogger.Log("=== Event System Status ===");
            DLLogger.Log("Regular Events:");
            foreach (var kvp in eventDict)
            {
                DLLogger.Log($"Event: {kvp.Key}, Listeners: {kvp.Value.GetInvocationList().Length}");
            }
            DLLogger.Log("Once Events:");
            foreach (var kvp in onceEventDict)
            {
                DLLogger.Log($"Event: {kvp.Key}, Listeners: {kvp.Value.GetInvocationList().Length}");
            }
        }
        #endregion
    }
}
