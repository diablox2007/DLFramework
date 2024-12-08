using com.dl.framework;
using System;
using UnityEngine;

namespace com.dl.framework
{
	public abstract class BaseDataModule : DataManager.IData
	{
		public abstract int Priority { get; }
		public abstract Type[] Dependencies { get; }

		protected virtual string FileName => GetType().Name;

		public virtual void Initialize() { }

		public virtual void Save()
		{
			try
			{
				SaveSystem.SaveData(FileName, this);
				//DLLogger.Log($"[{GetType().Name}] Saved successfully");
			}
			catch (Exception e)
			{
				DLLogger.LogError($"[{GetType().Name}] Save failed: {e.Message}");
			}
		}

		public virtual void Load()
		{
			try
			{
				if (SaveSystem.HasData(FileName))
				{
					// 使用泛型方法动态调用加载
					var methodInfo = typeof(SaveSystem).GetMethod("LoadData");
					var genericMethod = methodInfo.MakeGenericMethod(this.GetType());
					var loadedData = genericMethod.Invoke(null, new object[] { FileName });

					if (loadedData != null)
					{
						// 将加载的数据复制到当前实例
						var json = JsonUtility.ToJson(loadedData);
						JsonUtility.FromJsonOverwrite(json, this);
						//DLLogger.Log($"[{GetType().Name}] Loaded successfully");
					}
				}
				else
				{
					DLLogger.Log($"[{GetType().Name}] No save file found, using defaults");
				}
			}
			catch (Exception e)
			{
				DLLogger.LogError($"[{GetType().Name}] Load failed: {e.Message}");
			}
		}
	}
}
