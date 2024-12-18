#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace com.dl.framework.editor
{
	public abstract class DataModuleEditorBaseWindow<T> : EditorWindow where T : class, DataManager.IData, new()
	{
		protected T targetModule;
		protected Vector2 scrollPosition;
		protected bool isDirty = false;

		protected virtual void OnEnable()
		{
			// 现在编译器知道 T 是一个具体的类类型
			// 直接从存档加载数据，不依赖 DataManager
			targetModule = LoadModuleData();
			if (targetModule == null)
			{
				targetModule = new T();
				targetModule.Initialize();
			}
		}

		// 编辑器专用的数据加载方法
		private T LoadModuleData()
		{
			try
			{
				if (SaveSystem.HasData(typeof(T).Name))
				{
					return SaveSystem.LoadData<T>(typeof(T).Name);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to load editor data: {e.Message}");
			}
			return null;
		}

		protected virtual void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			DrawModuleGUI();

			EditorGUILayout.EndScrollView();

			bool changed = EditorGUI.EndChangeCheck();
			if (changed)
			{
				isDirty = true;
			}

			EditorGUILayout.Space();
			DrawToolbar();
		}

		protected virtual void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal();

			GUI.enabled = isDirty;
			if (GUILayout.Button("Save"))
			{
				SaveModule();
				isDirty = false;
			}
			GUI.enabled = true;

			if (GUILayout.Button("Load"))
			{
				LoadModule();
				isDirty = false;
			}

			if (GUILayout.Button("Reset"))
			{
				if (EditorUtility.DisplayDialog("Reset Data",
					"Are you sure you want to reset this module to default values?",
					"Yes", "No"))
				{
					ResetModule();
					isDirty = false;
				}
			}

			EditorGUILayout.EndHorizontal();

			// 显示未保存提示
			if (isDirty)
			{
				EditorGUILayout.HelpBox("There are unsaved changes!", MessageType.Warning);
			}
		}

		protected virtual void SaveModule()
		{
			try
			{
				targetModule.Save();
				EditorUtility.DisplayDialog("Success", "Data saved successfully!", "OK");
				AssetDatabase.Refresh();
			}
			catch (System.Exception e)
			{
				EditorUtility.DisplayDialog("Error", $"Failed to save data: {e.Message}", "OK");
				Debug.LogError($"Save failed: {e.Message}");
			}
		}

		protected virtual void LoadModule()
		{
			try
			{
				targetModule.Load();
				Repaint();
			}
			catch (System.Exception e)
			{
				EditorUtility.DisplayDialog("Error", $"Failed to load data: {e.Message}", "OK");
				Debug.LogError($"Load failed: {e.Message}");
			}
		}

		protected virtual void ResetModule()
		{
			targetModule = new T();
			targetModule.Initialize();
			Repaint();
		}

		protected virtual void OnDisable()
		{
			if (isDirty)
			{
				if (EditorUtility.DisplayDialog("Unsaved Changes",
					"You have unsaved changes. Would you like to save them?",
					"Yes", "No"))
				{
					SaveModule();
				}
			}
		}

		// 子类必须实现的绘制方法
		protected abstract void DrawModuleGUI();
	}
}
#endif