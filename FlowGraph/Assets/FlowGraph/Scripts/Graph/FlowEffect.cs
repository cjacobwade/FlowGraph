using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlowEffect
{
	public enum SequenceMode
	{
		AfterPrev = 0,
		WithPrev = 1,
		After = 2
	}

	public int nextNodeID = -1;

	public SequenceMode sequenceMode = SequenceMode.After;
	public float wait = 0f;

	public FlowModuleFunction function = new FlowModuleFunction();
}
