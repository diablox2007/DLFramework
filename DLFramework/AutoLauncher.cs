using UnityEngine;

namespace com.dl.framework
{
    public class AutoLauncher : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            // 直接获取或创建实例并初始化
            var framework = DLFrameworkManager.Instance;
            framework?.Initialize();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                if (DLFrameworkManager.Instance != null)
                {
                    Destroy(DLFrameworkManager.Instance.gameObject);
                }
            }
        }
#endif
    }
}
