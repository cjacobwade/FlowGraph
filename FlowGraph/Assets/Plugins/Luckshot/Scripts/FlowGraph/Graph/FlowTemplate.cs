using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NOTE: This is NOT meant to be a scriptableObject
// it exists as an argument for flow graph which lets you define
// a graph / template and which arguments get passed into that template

[System.Serializable]
public class FlowTemplate
{
	public FlowTemplate(FlowTemplate other)
	{
		flowGraph = other.flowGraph;

		arguments = new List<ArgumentBase>();
		foreach (var argument in other.arguments)
			arguments.Add((ArgumentBase)argument.Clone());
	}

	public FlowGraph flowGraph = null;

	[SerializeReference]
	public List<ArgumentBase> arguments = new List<ArgumentBase>();

	public List<ArgumentBase> GetDirectArguments(FlowGraphInstance graphInstance = null)
	{
		List<ArgumentBase> sourceArguments = null;
		FlowTemplate sourceTemplate = this;

		if(graphInstance != null)
        {
			sourceTemplate = graphInstance.Template;
			sourceArguments = graphInstance.DirectArguments;
        }

		List<ArgumentBase> directArguments = new List<ArgumentBase>();
		foreach (var argument in arguments)
		{
			ArgumentBase directArgument = (ArgumentBase)argument.Clone();

			object value = directArgument.GetValueFromSource(sourceTemplate, sourceArguments);
			directArgument.source = ArgumentSource.Value;
			directArgument.Value = value;

			directArguments.Add(directArgument);
		}

		return directArguments;
	}
}
