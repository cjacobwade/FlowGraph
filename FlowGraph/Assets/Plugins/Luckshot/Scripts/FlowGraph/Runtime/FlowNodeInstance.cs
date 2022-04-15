using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlowNodeInstance
{
	private FlowGraphInstance owner = null;
	public FlowGraphInstance Owner => owner;
	
	public FlowNode node = null;

	private List<FlowEffectInstance> effectInstances = new List<FlowEffectInstance>();
	public IReadOnlyCollection<FlowEffectInstance> EffectInstances => effectInstances;

	private List<FlowEffectInstance> activeEffects = new List<FlowEffectInstance>();
	public IReadOnlyCollection<FlowEffectInstance> ActiveEffects => activeEffects;

	public event Action<FlowNodeInstance> OnStarted = delegate { };
	public event Action<FlowNodeInstance> OnCanceled = delegate { };
	public event Action<FlowNodeInstance> OnComplete = delegate { };

	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectStarted = delegate { };
	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectComplete = delegate { };

	public FlowNodeInstance(FlowNode node, FlowGraphInstance graphInstance)
	{
		this.node = node;
		this.owner = graphInstance;

		foreach (var effect in node.effects)
		{
			var effectInstance = new FlowEffectInstance(effect, this);

			effectInstance.OnInvoked += EffectInstance_OnInvoked;
			effectInstance.OnStarted += EffectInstance_OnStarted;
			effectInstance.OnComplete += EffectInstance_OnCompleted;
			effectInstance.OnCanceled += EffectInstance_OnCanceled;

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
				effectInstance.Play();
		}
	}

	public void Cancel()
	{
		while (activeEffects.Count > 0)
			activeEffects[0].Cancel();

		OnCanceled(this);
	}

	private void EffectInstance_OnInvoked(FlowEffectInstance effectInstance)
	{
		activeEffects.Add(effectInstance);
	}

	private void EffectInstance_OnCanceled(FlowEffectInstance effectInstance)
	{
		activeEffects.Remove(effectInstance);
	}

	private void EffectInstance_OnStarted(FlowEffectInstance effectInstance)
	{
		//Debug.Log(string.Format("{0}: {1}", node.name, effectInstance.effect.function.function));

		OnEffectStarted(this, effectInstance);

		int effectIndex = effectInstances.IndexOf(effectInstance);
		if (effectIndex < effectInstances.Count - 1)
		{
			FlowEffectInstance nextEffectInstance = effectInstances[effectIndex + 1];
			if (nextEffectInstance.effect.sequenceMode == FlowEffect.SequenceMode.WithPrev)
				nextEffectInstance.Play();
		}
	}

	private void EffectInstance_OnCompleted(FlowEffectInstance effectInstance)
	{
		activeEffects.Remove(effectInstance);

		int effectIndex = effectInstances.IndexOf(effectInstance);
		if (effectIndex < effectInstances.Count - 1)
		{
			FlowEffectInstance nextEffectInstance = effectInstances[effectIndex + 1];
			if (nextEffectInstance.effect.sequenceMode == FlowEffect.SequenceMode.AfterPrev)
				nextEffectInstance.Play();
		}

		OnEffectComplete(this, effectInstance);

		if(activeEffects.Count == 0)
			OnComplete(this);
	}
}
