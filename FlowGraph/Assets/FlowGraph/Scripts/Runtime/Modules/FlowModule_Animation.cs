using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Animation : FlowModule
{
	[SerializeField]
	private new Animation animation = null;

	public void Play(FlowEffectInstance effect)
	{ PlayAnimation(effect, null); }

	public void PlayBackwards(FlowEffectInstance effect)
	{ PlayAnimationBackwards(effect, null); }

	public void PlayAnimation(FlowEffectInstance effect, AnimationClip clip)
	{
		if(clip != null)
			animation.clip = clip;

		Debug.Assert(animation.clip != null, "No animation found", this);

		var state = animation[animation.clip.name];
		state.speed = 1f;
		state.normalizedTime = 0f;
		state.enabled = true;

		PlayInternal(effect);
	}

	public void PlayAnimationBackwards(FlowEffectInstance effect, AnimationClip clip)
	{
		if(clip != null)
			animation.clip = clip;

		Debug.Assert(animation.clip != null, "No animation found", this);

		var state = animation[animation.clip.name];
		state.speed = -1f;
		state.normalizedTime = 1f;
		state.enabled = true;

		PlayInternal(effect);
	}

	private void PlayInternal(FlowEffectInstance effect)
	{
		animation.Play();

		if (animation.clip.length <= 0f || 
			animation.clip.wrapMode == WrapMode.Loop)
		{
			Complete(effect);
		}
		else
		{
			new FlowCallback_WaitForSeconds(effect, animation.clip.length);
		}
	}

	public void SampleAnimation(FlowEffectInstance effect, AnimationClip clip, float normalizedTime)
	{
		if (clip != null)
			animation.clip = clip;

		animation.Stop();

		var state = animation[animation.clip.name];
		state.normalizedTime = normalizedTime;
		state.enabled = true;

		Complete(effect);
	}

	public void StopAnimation(FlowEffectInstance effect)
	{
		animation.Stop();
		Complete(effect);
	}
}
