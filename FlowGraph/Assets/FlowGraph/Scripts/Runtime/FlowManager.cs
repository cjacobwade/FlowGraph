using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : Singleton<FlowManager>
{
	private List<FlowGraphInstance> activeFlows = new List<FlowGraphInstance>();

	public void PlayFlow(FlowGraph graph)
	{
		var flowInstance = new FlowGraphInstance(graph);
		activeFlows.Add(flowInstance);

		flowInstance.Play();
	}

	public void StopFlow(FlowGraph graph)
	{
		for(int i = 0; i < activeFlows.Count; i++)
		{ 
			var activeFlow = activeFlows[i];
			if (activeFlow.Graph == graph)
			{
				activeFlow.Stop();
				activeFlows.RemoveAt(i--);
			}
		}
	}

	private void StopAll()
	{
		foreach (var activeFlow in activeFlows)
			activeFlow.Stop();

		activeFlows.Clear();
	}

	private void OnDisable()
	{
		StopAll();
	}
}
