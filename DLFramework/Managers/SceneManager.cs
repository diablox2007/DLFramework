using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace com.dl.framework
{
	public class SceneManager : Singleton<SceneManager>
	{
		private Action onSceneLoadComplete;
		private string currentSceneName;
		private bool isLoading;

		protected override void OnInit()
		{
			currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		}

		public void LoadScene(string sceneName, Action onComplete = null)
		{
			if (isLoading)
			{
				DLLogger.LogWarning("Scene is already loading!");
				return;
			}

			if (currentSceneName == sceneName)
			{
				DLLogger.LogWarning($"Scene {sceneName} is already loaded!");
				onComplete?.Invoke();
				return;
			}

			onSceneLoadComplete = onComplete;
			CoroutineManager.Instance.StartCoroutine(LoadSceneAsync(sceneName));
		}

		private IEnumerator LoadSceneAsync(string sceneName)
		{
			isLoading = true;
			WindowManager.Instance.ShowLoading();

			// 触发场景切换前事件
			EventSystem.Instance.Trigger("OnBeforeSceneLoad", currentSceneName);

			AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
			asyncLoad.allowSceneActivation = false;

			while (asyncLoad.progress < 0.9f)
			{
				float progress = asyncLoad.progress / 0.9f;
				WindowManager.Instance.UpdateLoadingProgress(progress);
				yield return null;
			}

			asyncLoad.allowSceneActivation = true;
			while (!asyncLoad.isDone)
			{
				yield return null;
			}

			currentSceneName = sceneName;
			isLoading = false;

			// 触发场景切换完成事件
			EventSystem.Instance.Trigger("OnAfterSceneLoad", sceneName);

			WindowManager.Instance.HideLoading();
			onSceneLoadComplete?.Invoke();
		}

		public void LoadInitialScene()
		{
			LoadScene("Login");
		}

		public string GetCurrentSceneName()
		{
			return currentSceneName;
		}
	}
}
