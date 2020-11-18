using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniqueObjectManager : Singleton<UniqueObjectManager>
{
	private Dictionary<UniqueObjectData, UniqueObject> dataToObjectMap = new Dictionary<UniqueObjectData, UniqueObject>();
	private Dictionary<string, UniqueObject> nameToObjectMap = new Dictionary<string, UniqueObject>();

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
			Debug.LogWarning("UniqueObjectData already registered. This is not allowed.", existingObj);
		}

		dataToObjectMap[uniqueObject.Data] = uniqueObject;
		nameToObjectMap[uniqueObject.Data.name] = uniqueObject;
	}

	public void DeregisterUniqueObject(UniqueObject uniqueObject)
	{
		if (uniqueObject == null)
			return;

		dataToObjectMap.Remove(uniqueObject.Data);
		nameToObjectMap.Remove(uniqueObject.Data.name);
	}

	protected override void Awake()
	{
		base.Awake();
		ForceRegisterAllUniqueObjects();
		SceneManager.sceneLoaded += SceneManager_SceneLoaded;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		ForceRegisterAllUniqueObjects();
	}

	private void ForceRegisterAllUniqueObjects()
	{
		UniqueObject[] uniqueObjects = Resources.FindObjectsOfTypeAll<UniqueObject>();
		for (int i = 0; i < uniqueObjects.Length; i++)
		{
			if (uniqueObjects[i].gameObject.scene.IsValid())
				uniqueObjects[i].RegisterIfNeeded();
		}
	}
}
