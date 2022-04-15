using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_WaitForKey : FlowCallback
{
	private KeyCode keyCode = KeyCode.None;

	public FlowCallback_WaitForKey(FlowEffectInstance effect, KeyCode keyCode) : base(effect)
	{
		this.keyCode = keyCode;
		UpdateManager.Instance.RegisterUpdate(Tick); // This adds dependency on UpdateManager; kinda cursed
	}

	private void Tick()
    {
		if(Input.GetKeyDown(keyCode))
        {
			Cleanup();
			Complete();
        }
    }

	public override void Cancel()
	{
		base.Cancel();
		Cleanup();
	}

	private void Cleanup()
    {
		UpdateManager.Instance.UnregisterUpdate(Tick);
	}
}
