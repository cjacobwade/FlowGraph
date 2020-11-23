using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule : MonoBehaviour
{
	protected void Complete(FlowEffectInstance effect)
	{
		if(effect != null)
			effect.Complete();
	}
}
