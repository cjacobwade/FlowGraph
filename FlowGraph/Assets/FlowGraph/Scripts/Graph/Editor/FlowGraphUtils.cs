using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public static class FlowGraphUtils
{
	[OnOpenAsset]
	public static bool OnOpenAsset(int instanceID, int line)
	{
		FlowGraph flowGraph = Selection.activeObject as FlowGraph;
		if(flowGraph != null)
		{
			FlowGraphWindow.OpenWindow(flowGraph);
			return true;
		}

		return false;
	}
}
