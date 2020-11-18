using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowGraph", menuName = "Luckshot/Flow Graph")]
public class FlowGraph : ScriptableObject
{
	public int startNodeID = -1;
	public List<FlowNode> nodes = new List<FlowNode>();
}
