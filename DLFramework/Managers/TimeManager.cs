using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class TimeManager : Singleton<TimeManager>
    {
        private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
        private List<string> timersToRemove = new List<string>();

        protected override void OnInit()
        {
            base.OnInit();
        }

        public void Update()
        {
            timersToRemove.Clear();

            foreach (var kvp in timers)
            {
                Timer timer = kvp.Value;
                if (timer.Update())
                {
                    timersToRemove.Add(kvp.Key);
                }
            }

            foreach (string timerKey in timersToRemove)
            {
                timers.Remove(timerKey);
            }
        }

        public string SetTimeout(Action callback, float delay)
        {
            string id = Guid.NewGuid().ToString();
            timers.Add(id, new Timer(callback, delay, false));
            return id;
        }

        public string SetInterval(Action callback, float interval)
        {
            string id = Guid.NewGuid().ToString();
            timers.Add(id, new Timer(callback, interval, true));
            return id;
        }

        public void ClearTimer(string id)
        {
            if (timers.ContainsKey(id))
            {
                timers.Remove(id);
            }
        }

        public void ClearAll()
        {
            timers.Clear();
        }

        private class Timer
        {
            private Action callback;
            private float interval;
            private float currentTime;
            private bool repeat;

            public Timer(Action callback, float interval, bool repeat)
            {
                this.callback = callback;
                this.interval = interval;
                this.repeat = repeat;
                this.currentTime = 0;
            }

            public bool Update()
            {
                currentTime += Time.deltaTime;
                if (currentTime >= interval)
                {
                    callback?.Invoke();
                    if (repeat)
                    {
                        currentTime -= interval;
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
