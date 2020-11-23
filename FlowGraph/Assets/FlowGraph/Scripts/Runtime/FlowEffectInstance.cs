using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowEffectInstance
{
	public FlowEffect effect = null;
	public FlowCallback callback = null;

	public event Action<FlowEffectInstance> OnInvoked = delegate { };
	public event Action<FlowEffectInstance> OnStarted = delegate { };

	public event Action<FlowEffectInstance> OnStopped = delegate { };
	public event Action<FlowEffectInstance> OnComplete = delegate { };

	public FlowEffectInstance(FlowEffect effect)
	{
		this.effect = effect;
	}

	public void Play()
	{
		TimeManager.Invoke(Invoke, effect.wait);
		OnInvoked(this);
	}

	public void Stop()
	{
		TimeManager.CancelInvoke(Invoke);

		if (callback != null)
		{
			callback.Cancel();
			callback = null;
		}

		OnStopped(this);
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