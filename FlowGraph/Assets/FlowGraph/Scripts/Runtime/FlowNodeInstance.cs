using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowNodeInstance
{
	public FlowNode node = null;

	public List<FlowEffectInstance> effectInstances = new List<FlowEffectInstance>();

	private List<FlowEffectInstance> activeEffects = new List<FlowEffectInstance>();
	public List<FlowEffectInstance> ActiveEffects => activeEffects;

	public event Action<FlowNodeInstance> OnStarted = delegate { };
	public event Action<FlowNodeInstance> OnStopped = delegate { };
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
		OnStarted(this);

		for(int i = 0; i < effectInstances.Count; i++)
		{
			FlowEffectInstance effectInstance = effectInstances[i];
			if (effectInstance.effect.sequenceMode == FlowEffect.SequenceMode.After)
			{
				effectInstance.Play();
				activeEffects.Add(effectInstance);
			}
		}
	}

	public void Stop()
	{
		foreach (var effectInstance in effectInstances)
		{
			effectInstance.Stop();
			activeEffects.Remove(effectInstance);
		}

		OnStopped(this);
	}

	private void EffectInstance_OnStarted(FlowEffectInstance effectInstance)
	{
		//Debug.Log(string.Format("{0}: {1}", node.name, effectInstance.effect.function.function));

		OnEffectStarted(this, effectInstance);

		int effectIndex = effectInstances.IndexOf(effectInstance);
		for (int i = effectIndex + 1; i < effectInstances.Count; i++)
		{
			FlowEffectInstance nextEffectInstance = effectInstances[i];
			if (nextEffectInstance.effect.sequenceMode == FlowEffect.SequenceMode.WithPrev)
			{
				nextEffectInstance.Play();
				activeEffects.Add(nextEffectInstance);
			}
			else
			{
				break;
			}
		}
	}

	private void EffectInstance_OnCompleted(FlowEffectInstance effect)
	{
		int effectIndex = effectInstances.IndexOf(effect);
		if (effectIndex < effectInstances.Count - 1)
		{
			FlowEffectInstance nextEffectInstance = effectInstances[effectIndex + 1];
			if (nextEffectInstance.effect.sequenceMode == FlowEffect.SequenceMode.AfterPrev)
			{
				nextEffectInstance.Play();
				activeEffects.Add(nextEffectInstance);
			}
		}
		else
		{
			OnComplete(this);
		}

		OnEffectComplete(this, effect);
	}
}
