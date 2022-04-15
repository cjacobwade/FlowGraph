using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes.Editor;

namespace Luckshot.PathShape
{
	[CustomEditor(typeof(PathMesh_StyleData))]
	public class PathMesh_StyleDataEditor : Editor
	{
		private PathMesh_StyleData Style => target as PathMesh_StyleData;

		private static List<PathMesh> allPathMeshes = new List<PathMesh>();
		private static List<PathMesh> pathMeshesToUpdate = new List<PathMesh>();
		private int updateShapesPerFrame = 10;

		private void OnEnable()
		{
			Undo.undoRedoPerformed -= PropagateChanges;
			Undo.undoRedoPerformed += PropagateChanges;

			allPathMeshes.Clear();
			allPathMeshes.AddRange(FindObjectsOfType<PathMesh>());
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= PropagateChanges;
			allPathMeshes.Clear();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (GUI.changed)
				PropagateChanges();
		}

		private void SceneViewUpdate(SceneView sceneView)
		{
			if (pathMeshesToUpdate.Count > 0)
			{
				int numUpdated = 0;
				for (int i = 0; i < pathMeshesToUpdate.Count; i++)
				{
					if (pathMeshesToUpdate[i] == null)
					{
						pathMeshesToUpdate.RemoveAt(i--);
					}
					else
					{
						pathMeshesToUpdate[i].Regenerate();
						pathMeshesToUpdate.RemoveAt(i);

						if (++numUpdated > updateShapesPerFrame)
							break;
					}
				}

				if (pathMeshesToUpdate.Count == 0)
					SceneView.duringSceneGui -= SceneViewUpdate;
			}
		}

		private int Sort(PathMesh a, PathMesh b)
		{
			Vector3 camPos = Vector3.zero;
			if (SceneView.currentDrawingSceneView != null)
				camPos = SceneView.currentDrawingSceneView.camera.transform.position;

			Vector3 aDist = a.transform.position - camPos;
			Vector3 bDist = b.transform.position - camPos;

			return aDist.magnitude.CompareTo(bDist.magnitude);
		}

		private void PropagateChanges()
		{
			int prevCount = pathMeshesToUpdate.Count;

			allPathMeshes.Sort(Sort);

			foreach (var pathMesh in allPathMeshes)
			{
				if (pathMesh.Style != null && Style != null &&
					Style.Equals(pathMesh.Style))
				{
					if (!pathMeshesToUpdate.Contains(pathMesh))
						pathMeshesToUpdate.Add(pathMesh);
				}
			}

			if (prevCount == 0 && pathMeshesToUpdate.Count > 0)
				SceneView.duringSceneGui += SceneViewUpdate;
		}
	}
}