using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowRunner : MonoBehaviour
{
	[SerializeField]
	private FlowTemplate flowTemplate = null;
	private FlowGraphInstance graphInstance = null;

    private void Start()
    {
		if (flowTemplate != null && flowTemplate.flowGraph != null)
		{
			graphInstance = FlowManager.Instance.PlayFlow(flowTemplate, flowTemplate.GetDirectArguments());
		}
	}

    private void OnDisable()
	{
		if (graphInstance != null)
		{
			Debug.Log("Cancel");

			graphInstance.Cancel();
			graphInstance = null;
		}
	}
}
