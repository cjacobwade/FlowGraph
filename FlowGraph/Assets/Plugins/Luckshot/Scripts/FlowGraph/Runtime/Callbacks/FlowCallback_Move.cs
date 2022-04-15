using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlowCallback_Move : FlowCallback
{
	private Transform mover = null;

	public FlowCallback_Move(FlowEffectInstance effect, Transform mover, Transform location, float time, Ease easeType) : base(effect)
	{
		this.mover = mover;

		// TODO: wish I could combine these in some kind of position/rotation in one call

		mover.DOMove(location.transform.position, time)
			.SetUpdate(UpdateType.Fixed)
			.SetEase(easeType);

		mover.DORotate(location.transform.eulerAngles, time)
			.SetUpdate(UpdateType.Fixed)
			.SetEase(easeType);

		TimeManager.Invoke(Complete, time);
	}

	public override void Cancel()
	{
		mover.DOKill();

		TimeManager.CancelInvoke(Complete);

		base.Cancel();
	}
}
