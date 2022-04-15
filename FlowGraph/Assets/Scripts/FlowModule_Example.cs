using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_Example : FlowModule
{
    // NOTE: When a class inherits from FlowModule,
    // all its functions where the first argument is of type FlowEffectInstance
    // are exposed for use in FlowGraph

    public void SimpleExample(FlowEffectInstance effect, string message)
    {
        Debug.Log(message);

        // NOTE: FlowGraph effects won't move on until Complete is called
        Complete(effect);
    }

    public void CallbackEnterExample(FlowEffectInstance effect)
    {
        new FlowCallback_EnterExample(effect);
    }
}
