using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowCallback
{
	protected FlowEffectInstance effectInstance = null;

	public FlowCallback(FlowEffectInstance effectInstance)
	{
		this.effectInstance = effectInstance;
		this.effectInstance.OnStopped += (e) => Cancel();
	}

	public virtual void Complete()
	{
		effectInstance.Complete();
	}

	public virtual void Cancel()
	{
		
	}
}
