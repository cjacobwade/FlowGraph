using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Luckshot.Callbacks
{
	public class AAction
	{
		private List<Action> actions = new List<Action>(5);

		public static AAction operator +(AAction callback, Action action)
		{
			callback.actions.Add(action);
			return callback;
		}

		public static AAction operator -(AAction callback, Action action)
		{
			callback.actions.Remove(action);
			return callback;
		}

		public void Invoke()
		{
			for (int i = 0; i < actions.Count; i++)
				actions[i].Invoke();
		}

		public void Clear()
		{
			actions.Clear();
		}
	}

	public class AAction<T>
	{
		private List<Action<T>> actions = new List<Action<T>>(5);

		public static AAction<T> operator +(AAction<T> callback, Action<T> action)
		{
			callback.actions.Add(action);
			return callback;
		}

		public static AAction<T> operator -(AAction<T> callback, Action<T> action)
		{
			callback.actions.Remove(action);
			return callback;
		}

		public void Invoke(T t)
		{
			for (int i = 0; i < actions.Count; i++)
				actions[i].Invoke(t);
		}

		public void Clear()
		{
			actions.Clear();
		}
	}

	public class AAction<T, K>
	{
		private List<Action<T, K>> actions = new List<Action<T, K>>(5);

		public static AAction<T, K> operator +(AAction<T, K> callback, Action<T, K> action)
		{
			callback.actions.Add(action);
			return callback;
		}

		public static AAction<T, K> operator -(AAction<T, K> callback, Action<T, K> action)
		{
			callback.actions.Remove(action);
			return callback;
		}

		public void Invoke(T t, K k)
		{
			for (int i = 0; i < actions.Count; i++)
				actions[i].Invoke(t, k);
		}

		public void Clear()
		{
			actions.Clear();
		}
	}

	public class FFunc<T, R>
	{
		private List<Func<T, R>> actions = new List<Func<T, R>>(5);

		public static FFunc<T, R> operator +(FFunc<T, R> callback, Func<T, R> action)
		{
			callback.actions.Add(action);
			return callback;
		}

		public static FFunc<T, R> operator -(FFunc<T, R> callback, Func<T, R> action)
		{
			callback.actions.Remove(action);
			return callback;
		}

		public R Invoke(T t)
		{
			R result = default(R);

			for (int i = 0; i < actions.Count; i++)
				result = actions[i].Invoke(t);

			return result;
		}

		public void Clear()
		{
			actions.Clear();
		}
	}
}