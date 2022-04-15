using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlowModule_Core : FlowModule
{
	public void GOTO(FlowEffectInstance effect)
	{
		Complete(effect);
	}

	public void WaitRandomTime(FlowEffectInstance effect, float min = 0f, float max = 1f)
	{
		TimeManager.Invoke(() => Complete(effect), Random.Range(min, max));
	}

	public void WaitForKey(FlowEffectInstance effect, KeyCode keyCode)
    {
		new FlowCallback_WaitForKey(effect, keyCode);
    }

	public void PlayFlow(FlowEffectInstance effect, FlowTemplate template)
	{
		new FlowCallback_PlayFlow(effect, template);
	}

	public void CancelNode(FlowEffectInstance effect, FlowNodeReference nodeReference)
	{
		effect.Owner.Owner.CancelNode(nodeReference.nodeID);
		Complete(effect);
	}

	public void CancelAllOtherNodes(FlowEffectInstance effect)
	{
		List<FlowNodeInstance> nodesToCancel = new List<FlowNodeInstance>();

		var graphInstance = effect.Owner.Owner;
		foreach (var nodeInstance in graphInstance.ActiveNodes)
		{
			if(nodeInstance != effect.Owner)
				nodesToCancel.Add(nodeInstance);
		}

		for(int i = 0; i < nodesToCancel.Count; i++)
			nodesToCancel[i].Cancel();

		Complete(effect);
	}

	public void CompleteFlow(FlowEffectInstance effect)
	{
		Complete(effect);
		effect.Owner.Owner.Complete();
	}

	public void IfElse(FlowEffectInstance effect, bool boolean, FlowNodeReference trueNext, FlowNodeReference falseNext)
	{
		FlowNodeReference nextNodeRef = boolean ? trueNext : falseNext;
		effect.Owner.Owner.PlayNode(nextNodeRef.nodeID);

		Complete(effect);
	}

	public void IsObjectNull(FlowEffectInstance effect, Object uObject, FlowNodeReference nullNext, FlowNodeReference notNullNext)
	{
		FlowNodeReference nextNodeRef = (uObject != null) ? notNullNext : nullNext;
		effect.Owner.Owner.PlayNode(nextNodeRef.nodeID);

		Complete(effect);
	}

	public void DebugLog(FlowEffectInstance effect, string log)
	{
		Debug.Log(log, this);
		Complete(effect);
	}

	public void WaitForGlobalEvent(FlowEffectInstance effect, GlobalEventDefinition eventDefinition)
	{
		new FlowCallback_GlobalEvent(effect, eventDefinition);
	}

	public void SetUniqueObjectToNull(FlowEffectInstance effect, UniqueObjectData valueStore)
    {
		var graphInstance = effect.Owner.Owner;
		var function = effect.effect.function;

		var storeArg = function.arguments[0]; // valueStore
		if (storeArg.source == ArgumentSource.Template)
			graphInstance.DirectArguments[storeArg.templateIndex].Value = null;

		Complete(effect);
	}

	public void SetUniqueObjectToItemWithState(FlowEffectInstance effect, 
		ItemStateDefinition itemStateDefinition, 
		UniqueObjectData location, float maxDistance = 50f, 
		UniqueObjectData valueStore = null)
	{
		UniqueObject locationUO = UniqueObjectManager.Instance.LookupUniqueObject(location);
		if(locationUO != null)
		{
			Item item = ItemManager.Instance.TryFindItemMatchingItemStateDefinition(
				itemStateDefinition, locationUO.transform.position, searchRange: maxDistance);

			if(item != null)
			{
				UniqueObject itemUO = item.GetComponent<UniqueObject>();
				if(itemUO != null)
				{
					var graphInstance = effect.Owner.Owner;
					var function = effect.effect.function;

					var storeArg = function.arguments[3]; // valueStore
					if(storeArg.source == ArgumentSource.Template)
						graphInstance.DirectArguments[storeArg.templateIndex].Value = itemUO.Data;
				}
			}
		}

		Complete(effect);
	}

	public void SetUniqueObjectToItemWithPropertyState(FlowEffectInstance effect, 
		PropertyItemStateDefinition propertyStateDefinition,
		UniqueObjectData location, float maxDistance = 50f, 
		UniqueObjectData valueStore = null)
	{
		UniqueObject locationUO = UniqueObjectManager.Instance.LookupUniqueObject(location);
		if (locationUO != null)
		{
			Item item = ItemManager.Instance.TryFindItemMatchingPropertyStateDefinition(
				propertyStateDefinition, locationUO.transform.position, searchRange: maxDistance);

			if (item != null)
			{
				UniqueObject itemUO = item.GetComponent<UniqueObject>();
				if (itemUO != null)
				{
					var graphInstance = effect.Owner.Owner;
					var function = effect.effect.function;

					var storeArg = function.arguments[3]; // valueStore
					if (storeArg.source == ArgumentSource.Template)
						graphInstance.DirectArguments[storeArg.templateIndex].Value = itemUO.Data;
				}
			}
		}

		Complete(effect);
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

	public void SetActive(FlowEffectInstance effect, UniqueObjectData uod, bool setActive = true)
	{
		UniqueObject uo = UniqueObjectManager.Instance.LookupUniqueObject(uod);
		uo.gameObject.SetActive(setActive);

		Complete(effect);
	}

	public void SnapToLocation(FlowEffectInstance effect, UniqueObjectData uod, UniqueObjectData locationData)
	{
		UniqueObject uo = UniqueObjectManager.Instance.LookupUniqueObject(uod);
		UniqueObject location = UniqueObjectManager.Instance.LookupUniqueObject(locationData);

		uo.transform.SetPositionAndRotation(location.transform.position, location.transform.rotation);
	}

	public void MoveToLocation(FlowEffectInstance effect, UniqueObjectData uod, UniqueObjectData locationData, float time = 1f, Ease easeType = Ease.Linear)
	{
		UniqueObject uo = UniqueObjectManager.Instance.LookupUniqueObject(uod);
		UniqueObject location = UniqueObjectManager.Instance.LookupUniqueObject(locationData);

		new FlowCallback_Move(effect, uo.transform, location.transform, time, easeType);
	}
}
