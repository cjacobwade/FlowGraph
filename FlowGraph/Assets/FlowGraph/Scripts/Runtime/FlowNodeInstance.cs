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

	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectStarted = delegate { };
	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectComplete = delegate { };

	public FlowNodeInstance(FlowNode node)
	{
		this.node = node;

		foreach (var effect in node.effects)
		{
			var effectInstance = new FlowEffectInstance(effect);

			effectInstance.OnStarted += EffectInstance_OnStarted;
			effectInstance.OnComplete += EffectInstance_OnCompleted;

			effectInstances.Add(effectInstance);
		}
	}

	public void Play()
	{
		for(int i = 0; i < effectInstances.Count; i++)
		{
			FlowEffectInstance effectInstance = effectInstances[i];
			if(effectInstance.effect.sequenceMode == FlowEffect.SequenceMode.After)
				effectInstance.Play();
		}
	}

	public void Stop()
	{
		foreach (var effectInstance in effectInstances)
			effectInstance.Stop();
	}

	private void EffectInstance_OnStarted(FlowEffectInstance effectInstance)
	{
		//Debug.Log(string.Format("{0}: {1}", node.name, effectInstance.effect.function.function));

		OnEffectStarted(this, effectInstance);

		int effectIndex = effectInstances.IndexOf(effectInstance);
		for (int i = effectIndex + 1; i < effectInstances.Count; i++)
		{
			FlowEffectInstance nextEffectIndex = effectInstances[i];
			if (nextEffectIndex.effect.sequenceMode == FlowEffect.SequenceMode.WithPrev)
			{
				nextEffectIndex.Play();
			}
			else
			{
				break;
			}
		}
	}

	private void EffectInstance_OnCompleted(FlowEffectInstance effect)
	{
		OnEffectComplete(this, effect);

		int effectIndex = effectInstances.IndexOf(effect);
		if (effectIndex < effectInstances.Count - 1)
		{
			FlowEffectInstance nextEffectInstance = effectInstances[effectIndex + 1];
			if (nextEffectInstance.effect.sequenceMode == FlowEffect.SequenceMode.AfterPrev)
				nextEffectInstance.Play();
		}
		else
		{
			OnComplete(this);
		}
	}
}
