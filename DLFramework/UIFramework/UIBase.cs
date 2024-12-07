using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace com.dl.framework
{
    public abstract class UIBase : MonoBehaviour
    {
        protected RectTransform rectTransform;
        protected CanvasGroup canvasGroup;
        protected Dictionary<string, UIAnimation> animations;

        public bool IsVisible { get; private set; }
        public string WindowName { get; private set; }
        public UILayer Layer { get; protected set; }

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            animations = new Dictionary<string, UIAnimation>();
            WindowName = GetType().Name;
        }

        public virtual void Init(UILayer layer)
        {
            Layer = layer;
            OnInit();
        }

        protected virtual void OnInit() { }

        public virtual void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            IsVisible = true;

            if (animations.ContainsKey("Show"))
            {
                animations["Show"].Play(() =>
                {
                    OnShow();
                    onComplete?.Invoke();
                });
            }
            else
            {
                canvasGroup.alpha = 1;
                OnShow();
                onComplete?.Invoke();
            }
        }

        public virtual void Hide(Action onComplete = null)
        {
            IsVisible = false;

            if (animations.ContainsKey("Hide"))
            {
                animations["Hide"].Play(() =>
                {
                    OnHide();
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
            }
            else
            {
                canvasGroup.alpha = 0;
                OnHide();
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        public virtual void Close()
        {
            WindowManager.Instance.CloseWindow(WindowName);
        }

        protected virtual void OnDestroy()
        {
            foreach (var animation in animations.Values)
            {
                animation.Stop();
            }
            animations.Clear();
        }

        #region Animation Handlers
        protected void RegisterAnimation(string name, UIAnimation animation)
        {
            if (!animations.ContainsKey(name))
            {
                animations.Add(name, animation);
            }
        }

        protected void PlayAnimation(string name, Action onComplete = null)
        {
            if (animations.TryGetValue(name, out UIAnimation animation))
            {
                animation.Play(onComplete);
            }
        }

        protected void StopAnimation(string name)
        {
            if (animations.TryGetValue(name, out UIAnimation animation))
            {
                animation.Stop();
            }
        }
        #endregion
    }

    public abstract class UIAnimation
    {
        protected UIBase target;
        protected bool isPlaying;

        public UIAnimation(UIBase target)
        {
            this.target = target;
        }

        public abstract void Play(Action onComplete = null);
        public abstract void Stop();
    }

    public enum UILayer
    {
        Background = 0,
        Normal = 1000,
        Top = 2000,
        Modal = 3000,
        Loading = 4000,
    }
}
