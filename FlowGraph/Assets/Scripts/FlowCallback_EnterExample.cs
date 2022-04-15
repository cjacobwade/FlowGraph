using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_EnterExample : FlowCallback
{
    private Coroutine waitForEnterRoutine = null;

    public FlowCallback_EnterExample(FlowEffectInstance effect) : base(effect)
    {
        waitForEnterRoutine = FlowManager.Instance.StartCoroutine(WaitForEnter_Async());
    }

    IEnumerator WaitForEnter_Async()
    {
        yield return null;

        while(true)
        {
            if( Input.GetKeyDown(KeyCode.Return) || 
                Input.GetKeyDown(KeyCode.KeypadEnter))
                break;

            yield return null;
        }

        Complete();
        waitForEnterRoutine = null;

        yield break;
    }

    public override void Cancel()
    {
        base.Cancel();

        if(FlowManager.Instance != null && waitForEnterRoutine != null)
            FlowManager.Instance.StopCoroutine(waitForEnterRoutine);
    }
}
