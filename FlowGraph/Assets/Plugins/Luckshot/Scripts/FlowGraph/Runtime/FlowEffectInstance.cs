using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlowEffectInstance
{
	private FlowNodeInstance owner = null;
	public FlowNodeInstance Owner => owner;

	public FlowEffect effect = null;

	public FlowCallback callback = null;

	public event Action<FlowEffectInstance> OnInvoked = delegate { };
	public event Action<FlowEffectInstance> OnStarted = delegate { };

	public event Action<FlowEffectInstance> OnCanceled = delegate { };
	public event Action<FlowEffectInstance> OnComplete = delegate { };

	private bool invoked = false;
	private bool running = false;

	public FlowEffectInstance(FlowEffect effect, FlowNodeInstance owner)
	{
		this.effect = effect;
		this.owner = owner;
	}

	public void Play()
	{
		invoked = true;

		TimeManager.Invoke(Invoke, effect.wait);
		OnInvoked(this);
	}

	private void Invoke()
	{
		invoked = false;
		running = true;

		effect.function.Invoke(this);
		OnStarted(this);
	}

	public void Cancel()
	{
		// it's actually fine if this gets called when not running

		if (invoked)
		{
			TimeManager.CancelInvoke(Invoke);
			invoked = false;
		}

		running = false;

		if (callback != null)
		{
			callback.Cancel();
			callback = null;
		}

		OnCanceled(this);
	}

	public void Complete()
	{
		if (running)
		{
			running = false;
			OnComplete(this);
		}
	}
}