using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UniqueObject : MonoBehaviour
{
	[SerializeField]
	private UniqueObjectData data = null;
	public UniqueObjectData Data => data;

#if UNITY_EDITOR
	[Button("Find Or Create UniqueObjectData")]
	public void FindOrCreateItemData()
	{ data = UniqueObjectData.CreateItemData("Assets/Data/UniqueObjectDatas", gameObject.name); }
#endif

	private bool registered = false;

	private void Start()
	{
		RegisterIfNeeded();
	}

	public void RegisterIfNeeded()
	{
		if (registered)
			return;

		if (data != null)
		{
			UniqueObjectManager.Instance.RegisterUniqueObject(this);
			registered = true;
		}
	}

	public void SetData(UniqueObjectData setData)
	{
		if (data != setData)
		{
			if (data != null)
			{
				UniqueObjectManager.Instance.UnregisterUniqueObject(this);
				registered = false;
			}

			data = setData;

			if (data != null)
			{
				UniqueObjectManager.Instance.RegisterUniqueObject(this);
				registered = true;
			}
		}
	}

	private void OnDestroy()
	{
		if (UniqueObjectManager.Instance != null)
		{
			UniqueObjectManager.Instance.UnregisterUniqueObject(this);
			registered = false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue.SetA(0.2f);
		Gizmos.DrawSphere(transform.position, 0.3f);
	}
}
