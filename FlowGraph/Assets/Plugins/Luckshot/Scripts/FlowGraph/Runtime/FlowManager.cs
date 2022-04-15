using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : Singleton<FlowManager>
{
	[SerializeField]
	private List<FlowGraphInstance> activeGraphs = new List<FlowGraphInstance>();
	public List<FlowGraphInstance> ActiveGraphs => activeGraphs;

	public event System.Action<FlowGraphInstance> OnGraphPlayed = delegate {};
	public event System.Action<FlowGraphInstance> OnGraphCanceled = delegate {};
	public event System.Action<FlowGraphInstance> OnGraphComplete = delegate {};

	protected override void Awake()
	{
		base.Awake();
		FlowTypeCache.InitializeIfNeeded();
	}

	public FlowGraphInstance PlayFlow(FlowTemplate template, List<ArgumentBase> directArguments = null)
	{
		var graphInstance = new FlowGraphInstance(template, directArguments);
		PlayInternal(graphInstance);
		return graphInstance;
	}

	private void PlayInternal(FlowGraphInstance graphInstance)
	{
		graphInstance.OnComplete += GraphInstance_OnComplete;
		graphInstance.OnCanceled += GraphInstance_OnCanceled;

		activeGraphs.Add(graphInstance);

		graphInstance.Play();
		OnGraphPlayed(graphInstance);
	}

	private void GraphInstance_OnCanceled(FlowGraphInstance graphInstance)
	{
		activeGraphs.Remove(graphInstance);
		OnGraphCanceled(graphInstance);
	}

	private void GraphInstance_OnComplete(FlowGraphInstance graphInstance)
	{
		activeGraphs.Remove(graphInstance);
		OnGraphComplete(graphInstance);
	}

	private void CancelAll()
	{
		List<FlowGraphInstance> graphsToCancel = new List<FlowGraphInstance>();
		graphsToCancel.AddRange(activeGraphs);

		foreach (var graph in graphsToCancel)
			graph.Cancel();
	}

	private void OnDisable()
	{
		CancelAll();
	}
}
