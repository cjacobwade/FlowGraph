using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Animation : FlowModule
{
	[SerializeField]
	private new Animation animation = null;

	public void PlayAnimation(FlowEffectInstance effect, AnimationClip clip)
	{
		animation.clip = clip;
		animation.Play();

		if (clip.length <= 0f || clip.isLooping)
		{
			Complete(effect);
		}
		else
		{
			var callback = new FlowCallback_WaitForSeconds(effect, clip.length);
			RegisterCallback(callback);
		}
	}

	public void StopAnimation(FlowEffectInstance effect)
	{
		animation.Stop();
		Complete(effect);
	}
}
