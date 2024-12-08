#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace com.dl.framework.editor
{
	/// <summary>
	/// 配置管理器窗口
	/// </summary>
	public class ConfigManagerWindow : EditorWindow
	{
		private Vector2 scrollPosition;
		private string searchString = "";
		private const string CONFIG_FOLDER = "Assets/00_Game/Resources/Configs";

		[MenuItem("Tools/DLFramework/Config Manager")]
		public static void ShowWindow()
		{
			GetWindow<ConfigManagerWindow>("配置管理器");
		}

		private void OnGUI()
		{
			DrawToolbar();
			DrawSearchBar();
			DrawConfigList();
		}

		/// <summary>
		/// 绘制工具栏
		/// </summary>
		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			if (GUILayout.Button("创建所有配置", EditorStyles.toolbarButton))
			{
				CreateAllConfigs();
			}
			
			if (GUILayout.Button("打开配置文件夹", EditorStyles.toolbarButton))
			{
				OpenConfigFolder();
			}

			if (GUILayout.Button("刷新", EditorStyles.toolbarButton))
			{
				AssetDatabase.Refresh();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// 绘制搜索栏
		/// </summary>
		private void DrawSearchBar()
		{
			EditorGUILayout.Space();
			searchString = EditorGUILayout.TextField("搜索", searchString);
			EditorGUILayout.Space();
		}

		/// <summary>
		/// 绘制配置列表
		/// </summary>
		private void DrawConfigList()
		{
			EditorGUILayout.LabelField("配置文件列表", EditorStyles.boldLabel);

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			var configs = Resources.LoadAll<ConfigBase>("Configs")
				.Where(c => string.IsNullOrEmpty(searchString) || 
						   c.name.ToLower().Contains(searchString.ToLower()));

			foreach (var config in configs)
			{
				DrawConfigItem(config);
			}

			EditorGUILayout.EndScrollView();
		}

		/// <summary>
		/// 绘制单个配置项
		/// </summary>
		private void DrawConfigItem(ConfigBase config)
		{
			EditorGUILayout.BeginHorizontal("box");
			
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField(config, typeof(ConfigBase), false);
			EditorGUI.EndDisabledGroup();
			
			if (GUILayout.Button("选择", GUILayout.Width(60)))
			{
				Selection.activeObject = config;
			}
			
			if (GUILayout.Button("定位", GUILayout.Width(60)))
			{
				EditorGUIUtility.PingObject(config);
			}
			
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// 创建所有配置文件
		/// </summary>
		private static void CreateAllConfigs()
		{
			// 确保配置文件夹存在
			if (!AssetDatabase.IsValidFolder(CONFIG_FOLDER))
			{
				System.IO.Directory.CreateDirectory(CONFIG_FOLDER);
				AssetDatabase.Refresh();
			}

			// 查找所有带有ConfigPath特性的配置类
			var configTypes = TypeCache.GetTypesWithAttribute<ConfigPathAttribute>()
				.Where(t => !t.IsAbstract && typeof(ConfigBase).IsAssignableFrom(t));

			foreach (var type in configTypes)
			{
				var attr = type.GetCustomAttributes(typeof(ConfigPathAttribute), false)
					.FirstOrDefault() as ConfigPathAttribute;
				
				if (attr == null) continue;

				string assetPath = $"{CONFIG_FOLDER}/{attr.ConfigName}.asset";
				
				// 检查配置文件是否已存在
				if (AssetDatabase.LoadAssetAtPath<ConfigBase>(assetPath) != null)
				{
					Debug.Log($"Config already exists: {assetPath}");
					continue;
				}

				// 创建新的配置文件
				var config = CreateInstance(type);
				AssetDatabase.CreateAsset(config, assetPath);
				Debug.Log($"Created config: {assetPath}");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 打开配置文件夹
		/// </summary>
		private static void OpenConfigFolder()
		{
			if (!AssetDatabase.IsValidFolder(CONFIG_FOLDER))
			{
				System.IO.Directory.CreateDirectory(CONFIG_FOLDER);
				AssetDatabase.Refresh();
			}
			
			var folderObject = AssetDatabase.LoadAssetAtPath<Object>(CONFIG_FOLDER);
			Selection.activeObject = folderObject;
			EditorGUIUtility.PingObject(folderObject);
		}
	}
}
#endif
