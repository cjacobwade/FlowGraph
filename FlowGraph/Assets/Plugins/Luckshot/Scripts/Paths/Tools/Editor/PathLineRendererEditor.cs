using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathLineRenderer))]
public class PathLineRendererEditor : Editor
{
	PathLineRenderer PathLineRenderer => target as PathLineRenderer;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUI.changed)
			PathLineRenderer.Regenerate();
	}
}
