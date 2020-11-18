using UnityEngine;
using System.Collections;

public class TagBase : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		TagManager.Instance.RegisterTag(this);
	}

	protected virtual void OnDisable()
	{
		if(TagManager.Instance != null)
			TagManager.Instance.DeregisterTag(this);
	}
}
