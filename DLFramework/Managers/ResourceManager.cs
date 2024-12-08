using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.dl.framework
{
	public class ResourceManager : Singleton<ResourceManager>
	{
		private Dictionary<string, Object> resourceCache = new Dictionary<string, Object>();

		protected override void OnInit()
		{
			base.OnInit();
		}

		public T Load<T>(string path) where T : Object
		{
			string fullPath = GetFullPath<T>(path);

			if (resourceCache.TryGetValue(fullPath, out Object cachedResource))
			{
				return cachedResource as T;
			}

			T resource = Resources.Load<T>(path);
			if (resource != null)
			{
				resourceCache[fullPath] = resource;
			}
			else
			{
				DLLogger.LogError($"Failed to load resource: {path}");
			}

			return resource;
		}

		public async Task<T> LoadAsync<T>(string path) where T : Object
		{
			string fullPath = GetFullPath<T>(path);

			if (resourceCache.TryGetValue(fullPath, out Object cachedResource))
			{
				return cachedResource as T;
			}

			ResourceRequest request = Resources.LoadAsync<T>(path);
			while (!request.isDone)
			{
				await Task.Yield();
			}

			T resource = request.asset as T;
			if (resource != null)
			{
				resourceCache[fullPath] = resource;
			}
			else
			{
				DLLogger.LogError($"Failed to load resource async: {path}");
			}

			return resource;
		}

		public void Unload(string path)
		{
			if (resourceCache.ContainsKey(path))
			{
				resourceCache.Remove(path);
				Resources.UnloadUnusedAssets();
			}
		}

		public void ClearCache()
		{
			resourceCache.Clear();
			Resources.UnloadUnusedAssets();
		}

		private string GetFullPath<T>(string path)
		{
			return $"{typeof(T).Name}_{path}";
		}
	}
}
