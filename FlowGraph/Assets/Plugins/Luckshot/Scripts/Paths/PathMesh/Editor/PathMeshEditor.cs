using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Luckshot.Paths;

[CustomEditor(typeof(PathMesh))]
public class PathMeshEditor : Editor
{
	public PathMesh PathMesh => target as PathMesh;

	private void OnEnable()
	{
		if(PathMesh.Path == null)
		{
			var sp = serializedObject.FindProperty("path");
			sp.objectReferenceValue = PathMesh.gameObject.GetComponent<PathBase>();

			serializedObject.ApplyModifiedProperties();
		}

		if (PathMesh.Path == null)
			return;

		PathMesh.PathMeshChanged -= PathMesh_PathMeshChanged;
		PathMesh.PathMeshChanged += PathMesh_PathMeshChanged;

		PathMesh.Path.PathChanged -= Path_PathChanged;
		PathMesh.Path.PathChanged += Path_PathChanged;

		PathMesh[] pathMeshes = FindObjectsOfType<PathMesh>();
		for (int i = 0; i < pathMeshes.Length; i++)
		{
			if (pathMeshes[i] == PathMesh)
				continue;

			if (pathMeshes[i].MeshFilter.sharedMesh == PathMesh.MeshFilter.sharedMesh)
			{
				PathMesh.ClearSerializedMesh();
				break;
			}
		}

		PathMesh.RegenerateIfNeeded();
	}

	private void OnDisable()
	{
		PathMesh.PathMeshChanged -= PathMesh_PathMeshChanged;
		PathMesh.Path.PathChanged -= Path_PathChanged;
	}

	private void PathMesh_PathMeshChanged(PathMesh pathMesh)
	{ PathMesh.Regenerate(); }

	private void Path_PathChanged(PathBase path)
	{ PathMesh.Regenerate(); }

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUI.changed)
			PathMesh.Regenerate();
	}
}
