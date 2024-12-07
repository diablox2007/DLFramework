using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class WindowManager : MonoSingleton<WindowManager>
    {
        private Dictionary<string, UIBase> windowCache = new Dictionary<string, UIBase>();
        private Dictionary<UILayer, Transform> layerParents = new Dictionary<UILayer, Transform>();
        private Stack<string> windowStack = new Stack<string>();
        private Canvas mainCanvas;

        protected override void OnInit()
        {
            InitializeCanvas();
            InitializeLayers();

            base.OnInit();
        }

        private void InitializeCanvas()
        {
            GameObject canvasObj = new GameObject("[UICanvas]");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            DontDestroyOnLoad(canvasObj);
        }

        private void InitializeLayers()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                GameObject layerObj = new GameObject($"Layer_{layer}");
                layerObj.transform.SetParent(mainCanvas.transform, false);

                RectTransform rectTrans = layerObj.AddComponent<RectTransform>();
                rectTrans.anchorMin = Vector2.zero;
                rectTrans.anchorMax = Vector2.one;
                rectTrans.offsetMin = rectTrans.offsetMax = Vector2.zero;

                layerParents[layer] = layerObj.transform;
            }
        }

        public T ShowWindow<T>(Action<T> onInit = null) where T : UIBase
        {
            string windowName = typeof(T).Name;
            UIBase window;

            if (!windowCache.TryGetValue(windowName, out window))
            {
                // 加载预制体
                string prefabPath = $"UI/{windowName}";
                GameObject prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab == null)
                {
                    DLLogger.LogError($"Failed to load window prefab: {prefabPath}");
                    return null;
                }

                // 实例化窗口
                GameObject windowObj = Instantiate(prefab);
                window = windowObj.GetComponent<T>();
                if (window == null)
                {
                    DLLogger.LogError($"Window component not found on prefab: {windowName}");
                    Destroy(windowObj);
                    return null;
                }

                // 设置父物体和层级
                UILayer layer = GetWindowLayer<T>();
                window.transform.SetParent(layerParents[layer], false);
                window.Init(layer);

                windowCache[windowName] = window;
                onInit?.Invoke(window as T);
            }

            window.Show();
            windowStack.Push(windowName);
            return window as T;
        }

        public void CloseWindow(string windowName)
        {
            if (windowCache.TryGetValue(windowName, out UIBase window))
            {
                window.Hide(() =>
                {
                    if (windowStack.Count > 0 && windowStack.Peek() == windowName)
                    {
                        windowStack.Pop();
                    }
                });
            }
        }

        public void CloseAllWindows()
        {
            foreach (var window in windowCache.Values)
            {
                window.Hide();
            }
            windowStack.Clear();
        }

        public T GetWindow<T>() where T : UIBase
        {
            string windowName = typeof(T).Name;
            if (windowCache.TryGetValue(windowName, out UIBase window))
            {
                return window as T;
            }
            return null;
        }

        private UILayer GetWindowLayer<T>() where T : UIBase
        {
            // 可以通过特性或配置来设定窗口层级
            return UILayer.Normal;
        }

        public void ShowLoading(string tip = "Loading...")
        {
            ShowWindow<LoadingWindow>((window) => window.SetTip(tip));
        }

        public void HideLoading()
        {
            CloseWindow(typeof(LoadingWindow).Name);
        }

        public void UpdateLoadingProgress(float progress)
        {
            var loadingWindow = GetWindow<LoadingWindow>();
            if (loadingWindow != null)
            {
                loadingWindow.UpdateProgress(progress);
            }
        }

        public bool IsWindowOpen(string windowName)
        {
            return windowCache.TryGetValue(windowName, out UIBase window) && window.IsVisible;
        }

        public void DestroyWindow(string windowName)
        {
            if (windowCache.TryGetValue(windowName, out UIBase window))
            {
                Destroy(window.gameObject);
                windowCache.Remove(windowName);
            }
        }

        public void ClearCache()
        {
            foreach (var window in windowCache.Values)
            {
                if (window != null)
                {
                    Destroy(window.gameObject);
                }
            }
            windowCache.Clear();
            windowStack.Clear();
        }
    }
}
