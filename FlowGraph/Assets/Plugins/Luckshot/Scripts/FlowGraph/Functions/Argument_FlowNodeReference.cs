using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Argument_FlowNodeReference : Argument<FlowNodeReference>
{
	public override object Clone()
	{
		var argument = (Argument_FlowNodeReference)base.Clone();
		argument.Value = new FlowNodeReference((FlowNodeReference)argument.Value);
		return argument;
	}
}
