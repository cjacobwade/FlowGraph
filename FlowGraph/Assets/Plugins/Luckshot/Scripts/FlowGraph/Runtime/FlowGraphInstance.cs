using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class FlowGraphInstance
{
	private FlowTemplate template = null;
	public FlowTemplate Template => template;

	[SerializeReference]
	private List<ArgumentBase> directArguments = new List<ArgumentBase>();
	public List<ArgumentBase> DirectArguments => directArguments;

	private List<FlowNodeInstance> nodeInstances = new List<FlowNodeInstance>();
	public IReadOnlyCollection<FlowNodeInstance> NodeInstances => nodeInstances;

	private List<FlowNodeInstance> activeNodes = new List<FlowNodeInstance>();
	public IReadOnlyCollection<FlowNodeInstance> ActiveNodes => activeNodes;

	public event Action<FlowGraphInstance> OnComplete = delegate { };
	public event Action<FlowGraphInstance> OnCanceled = delegate { };

	public event Action<FlowNodeInstance> OnNodeComplete = delegate { };
	public event Action<FlowNodeInstance, FlowEffectInstance> OnEffectComplete = delegate { };

	public FlowGraphInstance(FlowTemplate template, List<ArgumentBase> directArguments = null)
	{
		this.template = template;
		this.directArguments = directArguments;

		Initialize();
	}

	private void Initialize()
	{
		foreach (var node in template.flowGraph.nodes)
		{
			var nodeInstance = new FlowNodeInstance(node, this);

			nodeInstance.OnStarted += NodeInstance_OnStarted;
			nodeInstance.OnComplete += NodeInstance_OnCanceledOrComplete;
			nodeInstance.OnCanceled += NodeInstance_OnCanceledOrComplete;

			nodeInstance.OnEffectComplete += NodeInstance_OnEffectComplete;

			nodeInstances.Add(nodeInstance);
		}
	}

	public void Play()
	{
		foreach (var nodeInstance in nodeInstances)
		{
			if (nodeInstance.node.id == template.flowGraph.startNodeID)
			{
				nodeInstance.Play();
				break;
			}
		}
	}

	public void PlayNode(int nodeID)
	{
		foreach (var nodeInstance in nodeInstances)
		{
			if (nodeInstance.node.id == nodeID)
			{
				nodeInstance.Play();
				break;
			}
		}
	}

	public void CancelNode(int nodeID)
	{
		foreach (var nodeInstance in nodeInstances)
		{
			if (nodeInstance.node.id == nodeID)
			{
				nodeInstance.Cancel();
				break;
			}
		}
	}

	public void Cancel()
	{
		while (activeNodes.Count > 0)
			activeNodes[0].Cancel();

		OnCanceled(this);
	}

	public void Complete()
	{
		while (activeNodes.Count > 0)
			activeNodes[0].Cancel();

		OnComplete(this);
	}

	private void NodeInstance_OnStarted(FlowNodeInstance nodeInstance)
	{
		activeNodes.Add(nodeInstance);
	}

	private void NodeInstance_OnCanceledOrComplete(FlowNodeInstance nodeInstance)
	{
		activeNodes.Remove(nodeInstance);
	}

	private void NodeInstance_OnEffectComplete(FlowNodeInstance nodeInstance, FlowEffectInstance effectInstance)
	{
		OnEffectComplete(nodeInstance, effectInstance);

		if (effectInstance.effect.nextNodeID != -1)
		{
			FlowNodeInstance nextNodeInstance = null;
			foreach (var iter in nodeInstances)
			{
				if (iter.node.id == effectInstance.effect.nextNodeID)
					nextNodeInstance = iter;
			}

			if (nextNodeInstance != null)
				nextNodeInstance.Play();
		}
	}
}
