using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowCallback
{
	protected FlowEffectInstance effectInstance = null;

	protected bool canceled = false;

	public FlowCallback(FlowEffectInstance effectInstance)
	{
		this.effectInstance = effectInstance;
		this.effectInstance.OnCanceled += (e) => Cancel();
	}

	public virtual void Complete()
	{
		effectInstance.Complete();
	}

	public virtual void Cancel()
	{
		canceled = true;
	}
}
