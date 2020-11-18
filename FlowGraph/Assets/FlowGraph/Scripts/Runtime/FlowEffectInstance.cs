using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowEffectInstance
{
	public FlowEffect effect = null;

	public event Action<FlowEffectInstance> OnComplete = delegate { };

	public FlowEffectInstance(FlowEffect effect)
	{
		this.effect = effect;
	}

	public void Play()
	{
		effect.function.Invoke(this);
	}

	public void Complete()
	{
		OnComplete(this);
	}

	public void Stop()
	{

	}
}