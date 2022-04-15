using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class FlowModule_Timeline : FlowModule
{
	[SerializeField]
	private PlayableDirector director = null;

	public void Play(FlowEffectInstance effect)
	{
		// Playing twice would create undefined timeline behaviour
		if (director.state == PlayState.Playing || 
			director.state == PlayState.Paused)
			director.Stop();

		new FlowCallback_PlayableDirector(effect, director);
		director.Play();
	}

	public void Stop(FlowEffectInstance effect)
	{
		director.Stop();
		Complete(effect);
	}

	public void Pause(FlowEffectInstance effect)
	{
		director.Pause();
		Complete(effect);
	}

	public void Resume(FlowEffectInstance effect)
	{
		director.Resume();
		Complete(effect);
	}
}
