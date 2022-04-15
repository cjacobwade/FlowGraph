using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedUniqueObject : MonoBehaviour
{
	[SerializeField]
	private UniqueObjectData uniqueObjectData = null;
	public UniqueObjectData UniqueObjectData => uniqueObjectData;

	private static Dictionary<UniqueObjectData, UniqueObject> dataToObjectMap = new Dictionary<UniqueObjectData, UniqueObject>();

	private void Awake()
	{
		if (!dataToObjectMap.ContainsKey(uniqueObjectData))
			ReparentSharedUniqueObject(transform);
	}

    public UniqueObject GetSharedUniqueObject()
	{
		if (!dataToObjectMap.TryGetValue(uniqueObjectData, out UniqueObject uniqueObject) || uniqueObject == null)
		{
			uniqueObject = new GameObject("UniqueObject_" + uniqueObjectData.name, typeof(UniqueObject)).GetComponent<UniqueObject>();
			uniqueObject.SetData(uniqueObjectData);
			
			dataToObjectMap[uniqueObjectData] = uniqueObject;
		}

		return uniqueObject;
	}

	public void ReparentSharedUniqueObject(Transform parent)
	{
		UniqueObject uniqueObject = GetSharedUniqueObject();
		uniqueObject.transform.SetParent(parent);
		uniqueObject.transform.ResetLocals();
	}
}
