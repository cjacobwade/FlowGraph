using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class GlobalEventDefinition
{
	public GlobalEventDefinition() {}

	public GlobalEventDefinition(GlobalEventDefinition other)
	{
		typeName = other.typeName;
		eventName = other.eventName;

		parameters = new List<EventParameterBase>();
		foreach (var parameter in other.parameters)
			parameters.Add((EventParameterBase)parameter.Clone());
	}

	public string typeName = string.Empty;
	public string eventName = string.Empty;

	[SerializeReference]
	public List<EventParameterBase> parameters = new List<EventParameterBase>();

	public override string ToString()
	{
		Type type = Type.GetType(typeName);
		if (type != null)
			return string.Format("{0}.{1}", type.Name, eventName);
		else
			return "None";
	}
}
