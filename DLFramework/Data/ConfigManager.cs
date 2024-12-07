using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<Type, object> configCache = new Dictionary<Type, object>();

        protected override void OnInit()
        {
            LoadAllConfigs();
            DLLogger.Log($"[{GetType().Name}] initialized.");
        }

        private void LoadAllConfigs()
        {
            // 预加载常用配置
            LoadConfig<GameConfig>();
            //LoadConfig<ItemConfig>();
            //LoadConfig<CharacterConfig>();
            // 添加其他配置...
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            Type type = typeof(T);

            if (configCache.TryGetValue(type, out object config))
            {
                return config as T;
            }

            return LoadConfig<T>();
        }

        private T LoadConfig<T>() where T : ScriptableObject
        {
            string configPath = $"Configs/{typeof(T).Name}";
            T config = Resources.Load<T>(configPath);

            if (config == null)
            {
                DLLogger.LogError($"Failed to load config: {configPath}");
                return null;
            }

            configCache[typeof(T)] = config;
            DLLogger.Log($"Loaded config: {typeof(T).Name}");
            return config;
        }

        public void ReloadConfig<T>() where T : ScriptableObject
        {
            Type type = typeof(T);
            if (configCache.ContainsKey(type))
            {
                configCache.Remove(type);
            }
            LoadConfig<T>();
        }

        public void ClearCache()
        {
            configCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }

    // 配置基类示例
    public abstract class ConfigBase : ScriptableObject
    {
        public string version;
        public string description;
    }

    // 游戏配置示例
    [CreateAssetMenu(fileName = "GameConfig", menuName = "DLFramework/Configs/GameConfig")]
    public class GameConfig : ConfigBase
    {
        public float musicVolume = 1.0f;
        public float soundVolume = 1.0f;
        public bool vibrationEnabled = true;
        public string defaultLanguage = "en";
        public int targetFrameRate = 60;
        // 其他游戏配置...
    }
}
