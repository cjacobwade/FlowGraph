using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowNodeInstance
{
	public FlowNode node = null;

	public List<FlowEffectInstance> effectInstances = new List<FlowEffectInstance>();

	public event Action<FlowNodeInstance> OnComplete = delegate { };
	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectComplete = delegate { };

	private int effectIndex = 0;

	public FlowNodeInstance(FlowNode node)
	{
		this.node = node;

		foreach (var effect in node.effects)
		{
			var effectInstance = new FlowEffectInstance(effect);
			effectInstance.OnComplete += EffectInstance_OnCompleted;

			effectInstances.Add(effectInstance);
		}
	}

	private void EffectInstance_OnCompleted(FlowEffectInstance effect)
	{
		OnEffectComplete(this, effect);

		if (effectInstances[effectInstances.Count - 1] != effect)
		{
			PlayNext();
		}
		else
		{
			OnComplete(this);
		}
	}

	public void Play()
	{
		effectIndex = 0;
		PlayNext();
	}

	private void PlayNext()
	{
		if (effectIndex < effectInstances.Count)
			effectInstances[effectIndex].Play();

		effectIndex++;
	}

	public void Stop()
	{
		foreach (var effectInstance in effectInstances)
			effectInstance.Stop();
	}
}
