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
}
