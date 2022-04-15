using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Argument_FlowTemplate : Argument<FlowTemplate>
{
	public override object Clone()
	{
		var argument = (Argument_FlowTemplate)base.Clone();
		argument.Value = new FlowTemplate((FlowTemplate)argument.Value);
		return argument;
	}
}
