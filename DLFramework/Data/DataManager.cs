using com.dl.framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class DataManager : Singleton<DataManager>
{
    // 数据模块接口
    public interface IData
    {
        void Initialize();
        void Save();
        void Load();
        int Priority { get; } // 初始化优先级
        Type[] Dependencies { get; } // 依赖的其他模块
    }

    // 数据模块特性
    [AttributeUsage(AttributeTargets.Class)]
    public class DataModuleAttribute : Attribute
    {
        public int Priority { get; set; }
        public DataModuleAttribute(int priority = 0)
        {
            Priority = priority;
        }
    }

    // -----------------------------------------------------------------------------------------

    private readonly Dictionary<Type, IData> dataModules = new Dictionary<Type, IData>();
    private bool isInitialized;

    protected override void OnInit()
    {
        InitializeAllModules();
    }

    private void InitializeAllModules()
    {
        if (isInitialized)
        {
            DLLogger.Log($"[{GetType().Name}] initialized.");
            return;
        }

        try
        {
            // 查找所有标记了 DataModule 特性的类
            var dataTypes = FindDataModuleTypes();

            // 按优先级排序
            var orderedTypes = SortTypesByPriority(dataTypes);

            // 检查依赖关系
            ValidateDependencies(orderedTypes);

            // 创建并注册实例
            foreach (var type in orderedTypes)
            {
                RegisterModuleInstance(type);
            }

            // 初始化所有模块
            InitializeModules();

            isInitialized = true;
            DLLogger.Log($"[DataManager] Successfully initialized {dataModules.Count} modules");
        }
        catch (Exception e)
        {
            DLLogger.LogError($"[DataManager] Initialization failed: {e}");
            throw;
        }
    }

    private IEnumerable<Type> FindDataModuleTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<DataModuleAttribute>() != null
                    && typeof(IData).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract);
    }

    private IEnumerable<Type> SortTypesByPriority(IEnumerable<Type> types)
    {
        return types.OrderByDescending(t =>
        {
            var attr = t.GetCustomAttribute<DataModuleAttribute>();
            return attr?.Priority ?? 0;
        });
    }

    private void ValidateDependencies(IEnumerable<Type> types)
    {
        var allTypes = types.ToList();
        foreach (var type in allTypes)
        {
            var instance = Activator.CreateInstance(type) as IData;
            if (instance?.Dependencies != null)
            {
                foreach (var dependency in instance.Dependencies)
                {
                    if (!allTypes.Contains(dependency))
                    {
                        throw new InvalidOperationException(
                            $"Module {type.Name} depends on {dependency.Name} which is not found!");
                    }
                }
            }
        }
    }

    private void RegisterModuleInstance(Type type)
    {
        try
        {
            if (dataModules.ContainsKey(type))
            {
                DLLogger.LogWarning($"[DataManager] Module {type.Name} already registered");
                return;
            }

            var instance = Activator.CreateInstance(type) as IData;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {type.Name}");
            }

            dataModules.Add(type, instance);
            DLLogger.Log($"[DataManager] Registered module: {type.Name}");
        }
        catch (Exception e)
        {
            DLLogger.LogError($"[DataManager] Failed to register {type.Name}: {e.Message}");
            throw;
        }
    }

    private void InitializeModules()
    {
        foreach (var module in dataModules.Values)
        {
            try
            {
                module.Initialize();
                DLLogger.Log($"[DataManager] Initialized module: {module.GetType().Name}");

                module.Load();
            }
            catch (Exception e)
            {
                DLLogger.LogError($"[DataManager] Failed to initialize {module.GetType().Name}: {e.Message}");
                throw;
            }
        }
    }

    public T GetModule<T>() where T : class, IData
    {
        if (dataModules.TryGetValue(typeof(T), out var module))
        {
            return module as T;
        }
        DLLogger.LogWarning($"[DataManager] Module {typeof(T).Name} not found");
        return null;
    }

    public void SaveAll()
    {
        foreach (var module in dataModules.Values)
        {
            try
            {
                module.Save();
            }
            catch (Exception e)
            {
                DLLogger.LogError($"[DataManager] Failed to save {module.GetType().Name}: {e.Message}");
            }
        }
    }

    public void LoadAll()
    {
        foreach (var module in dataModules.Values)
        {
            try
            {
                module.Load();
            }
            catch (Exception e)
            {
                DLLogger.LogError($"[DataManager] Failed to load {module.GetType().Name}: {e.Message}");
            }
        }
    }

    public void Release()
    {
        dataModules.Clear();
        isInitialized = false;
        DLLogger.Log("[DataManager] Released all modules");
    }
}
