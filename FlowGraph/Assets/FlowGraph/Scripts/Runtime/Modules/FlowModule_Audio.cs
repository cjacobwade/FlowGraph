using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Audio : FlowModule
{
	public void PlaySound(FlowEffectInstance effectInstance, AudioClip clip)
	{
		AudioManager.Instance.PlaySound(clip);
		Complete(effectInstance);
	}

	public void PlayRandomSound(FlowEffectInstance effectInstance, AudioClip[] clips)
	{
		AudioManager.Instance.PlaySound(clips.Random());
		Complete(effectInstance);
	}

	public void PlaySoundAtLocation(FlowEffectInstance effectInstance, AudioClip clip, UniqueObjectData locationData)
	{
		Transform location = UniqueObjectManager.Instance.LookupUniqueObject<Transform>(locationData);
		AudioManager.Instance.PlaySound(clip, location);
		Complete(effectInstance);
	}
}
