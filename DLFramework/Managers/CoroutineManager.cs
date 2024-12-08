using System.Collections;
using UnityEngine;
using System;

namespace com.dl.framework
{
	public class CoroutineManager : MonoSingleton<CoroutineManager>
	{
		protected override void OnInit()
		{
			base.OnInit();
		}

		/// <summary>
		/// 开启协程
		/// </summary>
		public Coroutine StartRoutine(IEnumerator routine)
		{
			if (routine == null)
			{
				DLLogger.LogWarning("Trying to start a null routine!");
				return null;
			}
			return StartCoroutine(routine);
		}

		/// <summary>
		/// 停止协程
		/// </summary>
		public void StopRoutine(Coroutine routine)
		{
			if (routine != null)
			{
				StopCoroutine(routine);
			}
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public void StopAllRoutines()
		{
			StopAllCoroutines();
		}

		/// <summary>
		/// 延迟执行
		/// </summary>
		public Coroutine DelayExecute(float delay, Action callback)
		{
			return StartRoutine(DelayExecuteRoutine(delay, callback));
		}

		/// <summary>
		/// 延迟执行（帧数）
		/// </summary>
		public Coroutine DelayFrames(int frames, Action callback)
		{
			return StartRoutine(DelayFramesRoutine(frames, callback));
		}

		/// <summary>
		/// 延迟执行，直到条件满足
		/// </summary>
		public Coroutine WaitUntil(Func<bool> condition, Action callback)
		{
			return StartRoutine(WaitUntilRoutine(condition, callback));
		}

		private IEnumerator DelayExecuteRoutine(float delay, Action callback)
		{
			yield return new WaitForSeconds(delay);
			callback?.Invoke();
		}

		private IEnumerator DelayFramesRoutine(int frames, Action callback)
		{
			while (frames > 0)
			{
				frames--;
				yield return null;
			}
			callback?.Invoke();
		}

		private IEnumerator WaitUntilRoutine(Func<bool> condition, Action callback)
		{
			yield return new WaitUntil(condition);
			callback?.Invoke();
		}

		/// <summary>
		/// 在下一帧执行
		/// </summary>
		public Coroutine NextFrame(Action callback)
		{
			return StartRoutine(NextFrameRoutine(callback));
		}

		private IEnumerator NextFrameRoutine(Action callback)
		{
			yield return null;
			callback?.Invoke();
		}

		/// <summary>
		/// 在帧结束时执行
		/// </summary>
		public Coroutine EndOfFrame(Action callback)
		{
			return StartRoutine(EndOfFrameRoutine(callback));
		}

		private IEnumerator EndOfFrameRoutine(Action callback)
		{
			yield return new WaitForEndOfFrame();
			callback?.Invoke();
		}

		protected override void OnDestroy()
		{
			StopAllRoutines();
			base.OnDestroy();
		}
	}
}
