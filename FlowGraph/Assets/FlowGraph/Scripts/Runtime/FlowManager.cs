using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : Singleton<FlowManager>
{
	[SerializeField]
	private List<FlowGraphInstance> activeGraphs = new List<FlowGraphInstance>();
	public List<FlowGraphInstance> ActiveGraphs => activeGraphs;

	public event System.Action<FlowGraphInstance> OnGraphPlayed = delegate {};
	public event System.Action<FlowGraphInstance> OnGraphStopped = delegate {};

	public void PlayFlow(FlowGraph graph)
	{
		var graphInstance = new FlowGraphInstance(graph);
		activeGraphs.Add(graphInstance);

		graphInstance.Play();
		OnGraphPlayed(graphInstance);
	}

	public void StopFlow(FlowGraph graph)
	{
		for(int i = 0; i < activeGraphs.Count; i++)
		{ 
			var activeGraph = activeGraphs[i];
			if (activeGraph.Graph == graph)
			{
				activeGraph.Stop();
				OnGraphStopped(activeGraph);

				activeGraphs.RemoveAt(i--);
			}
		}
	}

	private void StopAll()
	{
		foreach (var activeFlow in activeGraphs)
		{
			activeFlow.Stop();
			OnGraphStopped(activeFlow);
		}

		activeGraphs.Clear();
	}

	private void OnDisable()
	{
		StopAll();
	}
}
