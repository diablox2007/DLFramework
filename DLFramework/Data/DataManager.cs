using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.dl.framework
{
    public class DataManager : Singleton<DataManager>
    {
        private Dictionary<Type, object> dataModules = new Dictionary<Type, object>();

        protected override void OnInit()
        {
            InitializeAllModules();
            LoadAllData();

            DLLogger.Log($"[{GetType().Name}] initialized.");
        }

        private void InitializeAllModules()
        {
            // 注册所有数据模块
            //RegisterModule<PlayerData>();
            //RegisterModule<GameSettingsData>();
            //RegisterModule<InventoryData>();
            // 添加其他数据模块...
        }

        public void RegisterModule<T>() where T : class, new()
        {
            Type type = typeof(T);
            if (!dataModules.ContainsKey(type))
            {
                dataModules.Add(type, new T());
                DLLogger.Log($"Registered data module: {type.Name}");
            }
        }

        public T GetModule<T>() where T : class, new()
        {
            Type type = typeof(T);
            if (dataModules.TryGetValue(type, out object module))
            {
                return module as T;
            }

            RegisterModule<T>();
            return GetModule<T>();
        }

        public void LoadAllData()
        {
            foreach (var module in dataModules.Values)
            {
                if (module is IDataModule dataModule)
                {
                    dataModule.LoadData();
                }
            }
        }

        public void SaveAll()
        {
            foreach (var module in dataModules.Values)
            {
                if (module is IDataModule dataModule)
                {
                    dataModule.SaveData();
                }
            }
            DLLogger.Log("All data saved successfully.");
        }

        public void ClearAll()
        {
            foreach (var module in dataModules.Values)
            {
                if (module is IDataModule dataModule)
                {
                    dataModule.ClearData();
                }
            }
            DLLogger.Log("All data cleared.");
        }
    }

    public interface IDataModule
    {
        void LoadData();
        void SaveData();
        void ClearData();
    }
}
