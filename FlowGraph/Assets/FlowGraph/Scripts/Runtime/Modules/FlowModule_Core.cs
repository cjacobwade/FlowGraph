using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Core : FlowModule
{
	public void GOTO(FlowEffectInstance effect)
	{
		Complete(effect);
	}

	public void CancelNode(FlowEffectInstance effect, int nodeIndex)
	{
		// How to get reference to running flow instance from effect?
	}

	public void SpawnPrefab(FlowEffectInstance effect, GameObject prefab)
	{
		Instantiate(prefab);
		Complete(effect);
	}

	public void SpawnPrefabAtLocation(FlowEffectInstance effect, GameObject prefab, UniqueObjectData uod)
	{
		UniqueObject uo = UniqueObjectManager.Instance.LookupUniqueObject(uod);
		Instantiate(prefab, uo.transform.position, uo.transform.rotation);

		Complete(effect);
	}

	public void SetActive(FlowEffectInstance effect, UniqueObjectData uod, bool setActive)
	{
		UniqueObject uo = UniqueObjectManager.Instance.LookupUniqueObject(uod);
		uo.gameObject.SetActive(setActive);

		Complete(effect);
	}
}
