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

	public FlowEffect()
	{
		function = FlowTypeCache.GetDefaultModuleFunction();
	}

	public int nextNodeID = -1;

	public SequenceMode sequenceMode = SequenceMode.AfterPrev;
	public float wait = 0f;

	public ModuleFunction function = new ModuleFunction();
}
