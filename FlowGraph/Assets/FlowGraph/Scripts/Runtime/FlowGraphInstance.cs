using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowGraphInstance
{
	private FlowGraph graph = null;
	public FlowGraph Graph => graph;

	public List<FlowNodeInstance> nodeInstances = new List<FlowNodeInstance>();

	public event Action<FlowNodeInstance> OnNodeComplete = delegate { };
	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectComplete = delegate { };

	public FlowGraphInstance(FlowGraph graph)
	{
		this.graph = graph;

		foreach (var node in graph.nodes)
		{
			var nodeInstance = new FlowNodeInstance(node);
			nodeInstance.OnEffectComplete += NodeInstance_OnEffectComplete;

			nodeInstances.Add(nodeInstance);
		}
	}

	private void NodeInstance_OnEffectComplete(FlowNodeInstance nodeInstance, FlowEffectInstance effectInstance)
	{
		OnEffectComplete(nodeInstance, effectInstance);

		if (effectInstance.effect.nextNodeID != -1)
		{
			foreach (var iter in nodeInstances)
			{
				if (iter.node.id == effectInstance.effect.nextNodeID)
				{
					nodeInstance.Play();
					break;
				}
			}
		}
	}

	public void Play()
	{
		foreach (var nodeInstance in nodeInstances)
		{
			if (nodeInstance.node.id == graph.startNodeID)
			{
				nodeInstance.Play();
				break;
			}
		}
	}

	public void Stop()
	{
		foreach (var nodeInstance in nodeInstances)
			nodeInstance.Stop();
	}
}
