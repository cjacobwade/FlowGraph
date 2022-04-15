using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Argument_GlobalEventDefintion : Argument<GlobalEventDefinition>
{
	public override object Clone()
	{
		var argument = (Argument_GlobalEventDefintion)base.Clone();
		argument.Value = new GlobalEventDefinition((GlobalEventDefinition)argument.Value);
		return argument;
	}
}
