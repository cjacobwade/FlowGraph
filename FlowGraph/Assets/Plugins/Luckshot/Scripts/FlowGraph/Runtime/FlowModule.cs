using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule : MonoBehaviour
{
	[SerializeField, HideInInspector]
	private FlowModuleFunction function = null;
	public FlowModuleFunction Function => function;

	protected void Complete(FlowEffectInstance effect)
	{
		if(effect != null)
			effect.Complete();

	}
}
