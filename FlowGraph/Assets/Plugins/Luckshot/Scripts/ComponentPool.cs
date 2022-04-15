using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPool<T> where T : Component
{
	private T prefab = null;
	private List<T> pool = new List<T>();

	private int size = 0;

	private Transform parent = null;
	public Transform Parent => parent;

	public ComponentPool(T inPrefab, int inSize, Transform inParent)
	{
		prefab = inPrefab;
		size = inSize;
		parent = inParent;

		bool wasActive = prefab.gameObject.activeSelf;
		prefab.gameObject.SetActive(false);

		pool.Capacity = size;
		for(int i =0; i < size; i++)
		{
			T instance = MonoBehaviour.Instantiate(prefab, parent);
			pool.Add(instance);
		}

		prefab.gameObject.SetActive(wasActive);
	}

	public T Fetch()
	{
		T instance = null;

		if (pool.Count > 0)
		{
			instance = pool[0];
			instance.transform.SetParent(null, true);
			pool.RemoveAt(0);
		}
		else
		{
			instance = MonoBehaviour.Instantiate(prefab, null);
		}

		instance.gameObject.SetActive(true);
		return instance;
	}

	public void Return(T instance)
	{
		pool.Add(instance);
		instance.transform.SetParent(parent, true);
		instance.gameObject.SetActive(false);
	}
}
