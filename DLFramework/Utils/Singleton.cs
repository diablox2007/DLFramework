using com.dl.framework;

public abstract class Singleton<T> where T : class, new()
{
	private static T instance;
	private static readonly object lockObject = new object();

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				lock (lockObject)
				{
					if (instance == null)
					{
						instance = new T();
					}
				}
			}
			return instance;
		}
	}

	public void Initialize()
	{
		OnInit();
	}

	protected virtual void OnInit() { DLLogger.Log($"[{GetType().Name}] initialized."); }
}
