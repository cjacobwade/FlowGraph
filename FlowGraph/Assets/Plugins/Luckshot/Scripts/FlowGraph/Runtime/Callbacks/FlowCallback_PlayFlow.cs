using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_PlayFlow : FlowCallback
{
	private FlowGraphInstance graphInstance = null;

	public FlowCallback_PlayFlow(FlowEffectInstance effect, FlowTemplate template) : base(effect)
	{
		FlowTemplate ownerTemplate = effect.Owner.Owner.Template;

		graphInstance = FlowManager.Instance.PlayFlow(template, 
			template.GetDirectArguments(effect.Owner.Owner));

		graphInstance.OnComplete += GraphInstance_OnComplete;
		graphInstance.OnCanceled += GraphInstance_OnCanceled;
	}

	private void GraphInstance_OnComplete(FlowGraphInstance graphInstance)
	{
		Complete();
	}

	private void GraphInstance_OnCanceled(FlowGraphInstance graphInstance)
	{
		Cancel();
	}

	public override void Cancel()
	{
		if (!canceled)
		{
			Cleanup();
			graphInstance.Cancel();
			canceled = true;
		}
	}

	private void Cleanup()
	{
		graphInstance.OnComplete -= GraphInstance_OnComplete;
		graphInstance.OnCanceled -= GraphInstance_OnCanceled;
	}
}
