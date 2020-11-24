using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GlobalEventDefinition
{
	public string typeName = string.Empty;
	public string eventName = string.Empty;

	[SerializeReference]
	public List<EventParameterBase> parameters = new List<EventParameterBase>();
}
