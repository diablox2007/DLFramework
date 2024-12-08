using UnityEngine;
using System;
using System.Text;

namespace com.dl.framework
{
	public static class DLLogger
	{
		private const string PREFIX = "<color=#00FFFF>[DLFramework]</color>::";
		private static StringBuilder logBuilder = new StringBuilder();
		private static LogLevel logLevel = LogLevel.All;

		public enum LogLevel
		{
			All = 0,
			Debug = 1,
			Info = 2,
			Warning = 3,
			Error = 4,
			None = 5
		}

		public static void Log(object message)
		{
			if (logLevel <= LogLevel.Info)
			{
				string log = FormatLog("INFO", message);
				Debug.Log($"{PREFIX} {log}");
				logBuilder.AppendLine(log);
			}
		}

		public static void LogDebug(object message)
		{
			if (logLevel <= LogLevel.Debug)
			{
				string log = FormatLog("DEBUG", message);
				Debug.Log($"{PREFIX} {log}");
				logBuilder.AppendLine(log);
			}
		}

		public static void LogWarning(object message)
		{
			if (logLevel <= LogLevel.Warning)
			{
				string log = FormatLog("WARNING", message);
				Debug.LogWarning($"{PREFIX} {log}");
				logBuilder.AppendLine(log);
			}
		}

		public static void LogError(object message)
		{
			if (logLevel <= LogLevel.Error)
			{
				string log = FormatLog("ERROR", message);
				Debug.LogError($"{PREFIX} {log}");
				logBuilder.AppendLine(log);
			}
		}

		public static void LogException(Exception exception)
		{
			if (logLevel <= LogLevel.Error)
			{
				string log = FormatLog("EXCEPTION", exception.ToString());
				Debug.LogException(exception);
				logBuilder.AppendLine(log);
			}
		}

		private static string FormatLog(string type, object message)
		{
			return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}][{type}] {message}";
		}

		public static void SetLogLevel(LogLevel level)
		{
			logLevel = level;
			Log($"Log level changed to: {level}");
		}

		public static string GetLogHistory()
		{
			return logBuilder.ToString();
		}

		public static void ClearLogHistory()
		{
			logBuilder.Clear();
			Log("Log history cleared.");
		}

#if UNITY_EDITOR
		public static void SaveLogToFile()
		{
			try
			{
				string path = $"Logs/DLLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
				System.IO.Directory.CreateDirectory("Logs");
				System.IO.File.WriteAllText(path, logBuilder.ToString());
				Log($"Log saved to file: {path}");
			}
			catch (Exception e)
			{
				LogError($"Failed to save log file: {e.Message}");
			}
		}
#endif
	}
}
