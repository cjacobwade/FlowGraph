using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class FlowCallback_PlayableDirector : FlowCallback
{
	private PlayableDirector director = null;
	private bool invoked = false;

	public FlowCallback_PlayableDirector(FlowEffectInstance effect, PlayableDirector director) : base(effect)
	{
		this.director = director;
		this.director.paused += Director_Paused;
		this.director.played += Director_Played;
		this.director.stopped += Director_Stopped;
	}

	public override void Complete()
	{
		base.Complete();

		director.paused -= Director_Paused;
		director.played -= Director_Played;
		director.stopped -= Director_Stopped;
	}

	public override void Cancel()
	{
		base.Cancel();

		director.paused -= Director_Paused;
		director.played -= Director_Played;
		director.stopped -= Director_Stopped;
	}

	private void CancelInvoke()
	{
		if (invoked)
		{
			TimeManager.CancelInvoke(Complete);
			invoked = false;
		}
	}

	private void Director_Played(PlayableDirector director)
	{
		CancelInvoke();
		   
		if (director.extrapolationMode != DirectorWrapMode.Loop)
		{
			float remainingTime = (float)(director.duration - director.time);
			TimeManager.Invoke(Complete, remainingTime);

			invoked = true;
		}
	}

	private void Director_Paused(PlayableDirector director)
	{
		CancelInvoke();
	}

	private void Director_Stopped(PlayableDirector director)
	{
		Cancel();
	}
}
