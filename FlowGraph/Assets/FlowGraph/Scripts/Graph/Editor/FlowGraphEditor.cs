using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlowGraph))]
public class FlowGraphEditor : Editor
{
	public override void OnInspectorGUI()
	{
		if(GUILayout.Button(new GUIContent("Open Flow Graph")))
		{
			FlowGraphWindow.OpenWindow(target as FlowGraph);
		}
	}
}
