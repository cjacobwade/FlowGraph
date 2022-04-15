using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_WaitForSeconds : FlowCallback
{
	public FlowCallback_WaitForSeconds(FlowEffectInstance effect, float time) : base(effect)
	{
		TimeManager.Invoke(Complete, time);
	}

	public override void Cancel()
	{
		TimeManager.CancelInvoke(Complete);
		base.Cancel();
	}
}
