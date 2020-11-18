using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationUtils
{
	public static void PlayForwards(this Animation animation, float normalizedTime = 0f, float speed = 1f)
	{
		AnimationState state = animation[animation.clip.name];

		state.enabled = true;
		state.normalizedTime = normalizedTime;
		state.speed = speed;
		state.weight = 1f;

		animation.Play();
	}

	public static void PlayBackwards(this Animation animation, float normalizedTime = 1f, float speed = -1f)
	{
		AnimationState state = animation[animation.clip.name];

		state.enabled = true;
		state.normalizedTime = normalizedTime;
		state.speed = speed;
		state.weight = 1f;

		animation.Play();
	}

	public static void SetNormalizedTime(this Animation animation, float normalizedTime)
	{
		AnimationState state = animation[animation.clip.name];

		bool wasEnabled = state.enabled;

		state.enabled = true;
		state.normalizedTime = normalizedTime;
		state.speed = 1f;
		state.weight = 1f;

		animation.Sample();

		state.enabled = wasEnabled;
	}
}
