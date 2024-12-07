using UnityEngine;

namespace com.dl.framework
{
    public class FrameworkManager : MonoSingleton<FrameworkManager>
    {
        protected override void OnInit()
        {
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            try
            {
                // 事件系统
                EventSystem.Instance.Initialize();

                // 时间管理
                TimeManager.Instance.Initialize();

                // 协程管理
                CoroutineManager.Instance.Initialize();

                // 资源管理
                ResourceManager.Instance.Initialize();

                // 对象池
                PoolManager.Instance.Initialize();

                // 数据管理
                DataManager.Instance.Initialize();

                // 配置管理
                ConfigManager.Instance.Initialize();

                // UI管理
                WindowManager.Instance.Initialize();

                // 音频管理
                AudioManager.Instance.Initialize();

                DLLogger.Log("DLFramework initialization completed.");
            }
            catch (System.Exception e)
            {
                DLLogger.LogError($"DLFramework initialization failed: {e.Message} { e.StackTrace}");
            }
        }

        protected override void OnDestroy()
        {
            if (this == Instance)
            {
                // 清理资源
                PoolManager.Instance.ClearAll();
                WindowManager.Instance.ClearCache();
                ResourceManager.Instance.ClearCache();
                DataManager.Instance.SaveAll();
                EventSystem.Instance.RemoveAllListeners();

                DLLogger.Log("DLFramework destroyed.");
            }

            base.OnDestroy();
        }
    }
}
