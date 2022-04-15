using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyManager : Singleton<DestroyManager>
{
	public static HashSet<Object> markedForDestroy = new HashSet<Object>();
	
	public static bool IsMarkedForDestroy(Object obj)
	{ return markedForDestroy.Contains(obj); }

	public static bool IsMarkedForDestroy(GameObject go)
	{
		bool isMarkedForDestroy = false;

		Transform parent = go.transform;
		while (parent != null)
		{
			if (markedForDestroy.Contains(parent.gameObject))
			{
				isMarkedForDestroy = true;
				break;
			}

			parent = parent.parent;
		}

		return isMarkedForDestroy; 
	}

    public static new void Destroy(Object obj)
	{
		MonoBehaviour.Destroy(obj);
		markedForDestroy.Add(obj);

		if(Instance != null)
			Instance.enabled = true;
	}

	private void LateUpdate()
	{
		markedForDestroy.Clear();
		enabled = false;
	}
}
