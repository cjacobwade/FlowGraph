using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowCallback
{
	protected FlowEffectInstance effectInstance = null;

	public event System.Action<FlowEffectInstance> OnComplete = delegate {};
	public event System.Action<FlowEffectInstance> OnCancel = delegate {};

	public FlowCallback(FlowEffectInstance effectInstance)
	{
		this.effectInstance = effectInstance;
	}

	public virtual void Complete()
	{
		OnComplete(effectInstance);
	}

	public virtual void Cancel()
	{
		OnCancel(effectInstance);
	}
}
