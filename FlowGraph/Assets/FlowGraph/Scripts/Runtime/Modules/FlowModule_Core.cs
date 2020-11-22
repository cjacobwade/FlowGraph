using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Core : FlowModule
{
	public void WaitForSeconds(FlowEffectInstance effect, float time)
	{
		if (time <= 0f)
		{
			TimeManager.Invoke(Complete, effect, 0);
		}
		else
		{
			var callback = new FlowCallback_WaitForSeconds(effect, time);
			RegisterCallback(callback);
		}
	}
}
