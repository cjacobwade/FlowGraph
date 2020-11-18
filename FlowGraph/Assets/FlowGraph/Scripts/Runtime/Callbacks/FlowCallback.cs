using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowCallback
{
	protected FlowEffectInstance effectInstance = null;
	public FlowEffectInstance EffectInstance => effectInstance;

	public System.Action OnComplete = delegate {};
	public System.Action OnCancel = delegate {};

	public FlowCallback(FlowEffectInstance effect)
	{
		effectInstance = effect;
	}

	public virtual void Complete()
	{
		OnComplete();
	}

	public virtual void Cancel()
	{
		OnCancel();
	}
}
