using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowGraph", menuName = "Luckshot/Flow/Flow Graph")]
public class FlowGraph : ScriptableObject
{
	public int startNodeID = -1;
	public List<FlowNode> nodes = new List<FlowNode>();

	[SerializeReference]
	public List<ArgumentBase> templateArguments = new List<ArgumentBase>();
	public List<string> argumentTypeNames = new List<string>();
}
