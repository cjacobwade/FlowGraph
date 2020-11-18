using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UniqueObject : MonoBehaviour
{
	[SerializeField]
	private UniqueObjectData data = null;
	public UniqueObjectData Data
	{ get { return data; } }

#if UNITY_EDITOR
	[Button("Find Or Create UniqueObjectData")]
	public void FindOrCreateItemData()
	{ data = UniqueObjectData.CreateItemData("Assets/Data/UniqueObjectDatas", gameObject.name); }
#endif

	private bool registered = false;

	private void Awake()
	{
		RegisterIfNeeded();
	}

	public void RegisterIfNeeded()
	{
		if (registered)
			return;

		registered = true;

		if (data == null)
		{
			Debug.LogWarning("No UniqueObjectData assigned. Please fix", this);
		}
		else
		{
			UniqueObjectManager.Instance.RegisterUniqueObject(this);
		}
	}

	private void OnDestroy()
	{
		if (UniqueObjectManager.Instance != null)
			UniqueObjectManager.Instance.DeregisterUniqueObject(this);
	}
}
