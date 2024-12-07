using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace com.dl.framework
{
    /// <summary>
    /// 配置路径特性，用于标记配置类并指定配置文件名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigPathAttribute : Attribute
    {
        /// <summary>
        /// 配置文件名称
        /// </summary>
        public string ConfigName { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configName">配置文件名称，不需要包含扩展名</param>
        public ConfigPathAttribute(string configName)
        {
            ConfigName = configName;
        }
    }

    /// <summary>
    /// 配置基类，所有配置类都应继承此类
    /// </summary>
    public abstract class ConfigBase : ScriptableObject
    {
        /// <summary>
        /// 配置版本号
        /// </summary>
        public string configVersion = "1.0";

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string lastModifiedTime;

        /// <summary>
        /// 配置加载完成后的回调
        /// </summary>
        public virtual void OnConfigLoaded() { }

        /// <summary>
        /// Unity序列化验证回调
        /// </summary>
        protected virtual void OnValidate()
        {
            lastModifiedTime = DateTime.Now.ToString();
        }
    }

    /// <summary>
    /// 配置管理器，负责所有配置文件的加载和管理
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        /// <summary>
        /// 配置缓存字典
        /// </summary>
        private Dictionary<Type, ConfigBase> configCache = new Dictionary<Type, ConfigBase>();

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        protected override void OnInit()
        {
            LoadAllConfigs();
        }

        /// <summary>
        /// 加载所有配置
        /// </summary>
        private void LoadAllConfigs()
        {
            try
            {
                // 从Resources/Configs目录加载所有配置文件
                var configs = Resources.LoadAll<ConfigBase>("Configs");
                foreach (var config in configs)
                {
                    var type = config.GetType();
                    configCache[type] = config;
                    config.OnConfigLoaded();
                    DLLogger.Log($"[ConfigManager] Loaded config: {type.Name}");
                }

                DLLogger.Log($"[ConfigManager] Successfully loaded {configCache.Count} configs");
            }
            catch (Exception e)
            {
                DLLogger.LogError($"[ConfigManager] Failed to load configs: {e}");
            }
        }

        /// <summary>
        /// 获取指定类型的配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <returns>配置实例</returns>
        public T GetConfig<T>() where T : ConfigBase
        {
            if (configCache.TryGetValue(typeof(T), out var config))
            {
                return config as T;
            }

            DLLogger.LogWarning($"[ConfigManager] Config not found: {typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// 重新加载指定类型的配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        public void ReloadConfig<T>() where T : ConfigBase
        {
            Type type = typeof(T);
            var attr = type.GetCustomAttributes(typeof(ConfigPathAttribute), false);
            if (attr.Length == 0)
            {
                DLLogger.LogError($"[ConfigManager] Config {type.Name} missing ConfigPath attribute");
                return;
            }

            var configPath = $"Configs/{(attr[0] as ConfigPathAttribute).ConfigName}";
            var config = Resources.Load<T>(configPath);

            if (config != null)
            {
                configCache[type] = config;
                config.OnConfigLoaded();
                DLLogger.Log($"[ConfigManager] Reloaded config: {type.Name}");
            }
            else
            {
                DLLogger.LogError($"[ConfigManager] Failed to reload config: {type.Name}");
            }
        }

        /// <summary>
        /// 重新加载所有配置
        /// </summary>
        public void ReloadAllConfigs()
        {
            configCache.Clear();
            Resources.UnloadUnusedAssets();
            LoadAllConfigs();
        }

        /// <summary>
        /// 清理配置缓存
        /// </summary>
        public void ClearConfigCache()
        {
            configCache.Clear();
            Resources.UnloadUnusedAssets();
            DLLogger.Log("[ConfigManager] Config cache cleared");
        }
    }
}
