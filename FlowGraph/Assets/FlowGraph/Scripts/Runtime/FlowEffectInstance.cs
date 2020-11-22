using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowEffectInstance
{
	public FlowEffect effect = null;

	public event Action<FlowEffectInstance> OnStarted = delegate { };
	public event Action<FlowEffectInstance> OnComplete = delegate { };

	public FlowEffectInstance(FlowEffect effect)
	{
		this.effect = effect;
	}

	public void Play()
	{
		TimeManager.Invoke(Invoke, effect.wait);
	}

	public void Stop()
	{
		TimeManager.CancelInvoke(Invoke);
	}

	private void Invoke()
	{
		OnStarted(this);
		effect.function.Invoke(this);
	}

	public void Complete()
	{
		OnComplete(this);
	}
}