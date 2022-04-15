using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PersistentItem : PropertyItem
{
    protected override void AwakeIfNeeded()
    {
        base.AwakeIfNeeded();
		SaveManager.Instance.RegisterItem(Item);
	}

    protected override void OnDestroy()
	{
		base.OnDestroy();

		if(SaveManager.Instance != null)
			SaveManager.Instance.DeregisterItem(Item);
	}
}
