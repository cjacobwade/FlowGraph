using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowRunner : MonoBehaviour
{
	[SerializeField]
	private FlowGraph flowGraph = null;

	private void OnEnable()
	{
		FlowManager.Instance.PlayFlow(flowGraph);
	}
	
	private void OnDisable()
	{
		if(FlowManager.Instance != null)
			FlowManager.Instance.StopFlow(flowGraph);
	}
}
