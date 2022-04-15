using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class FlowNode
{
	public FlowNode(Vector2 position)
	{
		this.position = position;
	}

	public FlowNode(FlowNode other)
	{
		position = other.position;
		id = other.id;
		name = other.name;

		effects = new List<FlowEffect>();
		foreach (var effect in other.effects)
			effects.Add(new FlowEffect(effect));
	}

	public int id = 0;
	public string name = string.Empty;

	public Vector2 position = Vector2.zero;

	public List<FlowEffect> effects = new List<FlowEffect>();
}
