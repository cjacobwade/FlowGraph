using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class UniqueObjectManager : Singleton<UniqueObjectManager>
{
	[SerializeField]
	private Transform managersRoot = null;

	private Dictionary<UniqueObjectData, UniqueObject> dataToObjectMap = new Dictionary<UniqueObjectData, UniqueObject>();
	private Dictionary<string, UniqueObject> nameToObjectMap = new Dictionary<string, UniqueObject>(StringComparer.OrdinalIgnoreCase);

#if UNITY_EDITOR
	[System.Serializable]
	private class UniquePair
	{
		public UniqueObject uniqueObject = null;
		public UniqueObjectData data = null;

		public UniquePair(UniqueObject uo, UniqueObjectData uod)
		{
			uniqueObject = uo;
			data = uod;
		}
	}

	[SerializeField]
	private List<UniquePair> uniquePairs = new List<UniquePair>();
	private Dictionary<UniqueObjectData, UniquePair> dataToPairMap = new Dictionary<UniqueObjectData, UniquePair>();
#endif

	public static bool IsActive()
	{ return Instance != null && Instance.dataToObjectMap.Count > 0; }

	protected override void Awake()
	{
		base.Awake();

		if (UniqueObjectManager.Instance != this)
		{
			// Will be destroyed elsewhere
		}
		else
		{
			UniqueObject[] uniqueObjects = managersRoot.GetComponentsInChildren<UniqueObject>(true);
			for (int i = 0; i < uniqueObjects.Length; i++)
				uniqueObjects[i].RegisterIfNeeded();
		}
	}

	public T LookupUniqueObject<T>(string name) where T : Component
	{
		if (!string.IsNullOrEmpty(name) && nameToObjectMap.TryGetValue(name, out UniqueObject uniqueObject))
			return uniqueObject.GetComponent<T>();

		Debug.LogWarningFormat("UniqueObjectLookup for data {0} failed.", name);
		return null;
	}

	public UniqueObject LookupUniqueObject(string name)
	{
		if (!string.IsNullOrEmpty(name) && nameToObjectMap.TryGetValue(name, out UniqueObject uniqueObject))
			return uniqueObject;

		Debug.LogWarningFormat("UniqueObjectLookup for data {0} failed.", name);
		return null;
	}

	public T LookupUniqueObject<T>(UniqueObjectData data) where T : Component
	{
		if(data != null && dataToObjectMap.TryGetValue(data, out UniqueObject uniqueObject))
			return uniqueObject.GetComponent<T>();

		Debug.LogWarningFormat("UniqueObjectLookup for data {0} failed.", data);
		return null;
	}

	public UniqueObject LookupUniqueObject(UniqueObjectData data)
	{
		if (data != null && dataToObjectMap.TryGetValue(data, out UniqueObject uniqueObject))
			return uniqueObject;

		Debug.LogWarningFormat("UniqueObjectLookup for data {0} failed.", data);
		return null;
	}

	public void RegisterUniqueObject(UniqueObject uniqueObject)
	{
		if(dataToObjectMap.TryGetValue(uniqueObject.Data, out UniqueObject existingObj))
		{
			Debug.LogWarningFormat(existingObj, "UniqueObjectData {0} already registered. This is not allowed.", uniqueObject.Data);
		}

		dataToObjectMap[uniqueObject.Data] = uniqueObject;
		nameToObjectMap[uniqueObject.Data.name] = uniqueObject;

#if UNITY_EDITOR
		if(dataToPairMap.TryGetValue(uniqueObject.Data, out UniquePair uniquePair))
		{
			dataToPairMap[uniqueObject.Data].uniqueObject = uniqueObject;
		}
		else
		{
			uniquePair = new UniquePair(uniqueObject, uniqueObject.Data);
			uniquePairs.Add(uniquePair);

			dataToPairMap[uniqueObject.Data] = uniquePair;
		}
#endif
	}

	public void UnregisterUniqueObject(UniqueObject uniqueObject)
	{
		if (uniqueObject == null)
			return;

		if (dataToObjectMap.TryGetValue(uniqueObject.Data, out UniqueObject storedUO) &&
			uniqueObject == storedUO)
		{
			dataToObjectMap.Remove(uniqueObject.Data);
			nameToObjectMap.Remove(uniqueObject.Data.name);
		}

#if UNITY_EDITOR
		if(dataToPairMap.TryGetValue(uniqueObject.Data, out UniquePair pair))
		{
			uniquePairs.Remove(pair);
			dataToPairMap.Remove(uniqueObject.Data);
		}
#endif
	}

	public void ForceRegisterUniqueObjectsInScene(Scene scene)
	{
		GameObject[] rootGOs = scene.GetRootGameObjects();
		for(int i = 0; i < rootGOs.Length; i++)
		{
			UniqueObject[] uniqueObjects = rootGOs[i].GetComponentsInChildren<UniqueObject>(true);
			for (int j = 0; j < uniqueObjects.Length; j++)
			{
				UniqueObject uo = uniqueObjects[j];

				if(!DestroyManager.IsMarkedForDestroy(uo))
					uo.RegisterIfNeeded();
			}
		}
	}
}
