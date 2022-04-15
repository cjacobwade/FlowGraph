using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class FlowNodeReference
{
	public FlowNodeReference() {}
	public FlowNodeReference(FlowNodeReference other)
	{ nodeID = other.nodeID; }

	public int nodeID = -1;
}
