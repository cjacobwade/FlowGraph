using UnityEditor;
using UnityEngine;

namespace Luckshot.Paths
{
	[CustomEditor(typeof(LinePath), true)]
	public class LinePathEditor : Editor
	{
		protected const float handleSize = 0.07f;
		protected const float pickSize = 0.06f;

		private LinePath LinePath => target as LinePath;

		protected int selectedIndex = -1;
		private static bool debugMode = false;

		private void OnEnable()
		{
			Undo.undoRedoPerformed += UndoRedoPerformed;

			if (!Application.IsPlaying(LinePath))
				LinePath.NotifyChanged();
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		private void UndoRedoPerformed()
		{
			LinePath.NotifyChanged();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (LinePath.PointCount < 3)
			{
				if (LinePath.Loop)
					LinePath.SetLoop(false);

				EditorGUILayout.LabelField("Need at least 3 controls to loop", EditorStyles.helpBox);

				GUI.enabled = false;
				EditorGUILayout.Toggle("Loop", LinePath.Loop);
				GUI.enabled = true;
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				bool loop = EditorGUILayout.Toggle("Loop", LinePath.Loop);
				if (EditorGUI.EndChangeCheck())
					LinePath.SetLoop(loop);
			}

			if (selectedIndex >= 0 && selectedIndex < LinePath.PointCount)
				DrawSelectedPointInspector();

			debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);

			if (GUILayout.Button("Add Point"))
			{
				Undo.RecordObject(LinePath, "Add Point");
				EditorUtility.SetDirty(LinePath);

				LinePath.AddControl();
			}

			if (LinePath.PointCount > 2 && GUILayout.Button("Remove Point"))
			{
				Undo.RecordObject(LinePath, "Remove Point");
				EditorUtility.SetDirty(LinePath);

				LinePath.RemoveControl();
			}

			if (GUI.changed)
				LinePath.NotifyChanged();
		}

		protected virtual void DrawSelectedPointInspector()
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", LinePath.Points[selectedIndex]);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(LinePath, "Move Point");
				EditorUtility.SetDirty(LinePath);
				LinePath.Points[selectedIndex] = point;
			}

			// Draw Node Data if ADV SPLINE
			// Would be in AdvSplineInspector, except AdvSpline is generic meaning
			// we can't use the CustomEditor attribute and force child types to use its inspector
			// which means either this is here or every AdvSpline child needs an associated TypeInspector script
			SerializedObject so = serializedObject;
			SerializedProperty nodeDataProp = so.FindProperty(string.Format("{0}.Array.data[{1}]", "_nodeData", selectedIndex));
			if (nodeDataProp != null)
			{
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(nodeDataProp, new GUIContent("Node Data"), true);

				if (EditorGUI.EndChangeCheck())
					so.ApplyModifiedProperties();
			}
		}

		protected virtual void OnSceneGUI()
		{
			for (int i = 1; i < LinePath.PointCount; i++)
			{
				Handles.color = Color.white;
				Handles.DrawLine(
					LinePath.transform.TransformPoint(LinePath.Points[i]),
					LinePath.transform.TransformPoint(LinePath.Points[i - 1]));
			}

			if (LinePath.Loop)
			{
				Handles.DrawLine(
					LinePath.transform.TransformPoint(LinePath.Points[0]),
					LinePath.transform.TransformPoint(LinePath.Points[LinePath.PointCount - 1]));
			}

			for (int i = 0; i < LinePath.PointCount; i++)
				ShowPoint(i);

			int numMidpoints = LinePath.LineCount;
			float alphaPerControl = 1f / (float)numMidpoints;
			float midpointAlpha = alphaPerControl / 2f;

			for (int i = 0; i < numMidpoints; i++)
			{
				Vector3 midpoint = LinePath.GetPoint(midpointAlpha);
				Vector3 toCamera = Camera.current.transform.position - midpoint;

				float size = HandleUtility.GetHandleSize(midpoint);
				Handles.color = Color.cyan;
				Handles.DrawSolidDisc(midpoint, toCamera, size * handleSize);

				if (Handles.Button(midpoint, Quaternion.LookRotation(toCamera), size * handleSize, size * pickSize, Handles.CircleHandleCap))
				{
					Undo.RecordObject(LinePath, "Insert Line");
					EditorUtility.SetDirty(LinePath);

					int controlIndex = i + 1;
					LinePath.InsertControl(controlIndex, midpoint);
					selectedIndex = controlIndex;

					Repaint();
					return;
				}

				midpointAlpha += alphaPerControl;
			}

			if (debugMode)
			{
				for (int i = 0; i < LinePath.PointCount; i++)
				{
					Vector3 pos = LinePath.GetPoint(i);
					Vector3 normal = LinePath.GetNormal(i);

					Handles.color = Color.red;
					Handles.DrawLine(pos, pos + normal * 0.8f);
				}

				int numIterations = 10 * LinePath.PointCount;
				for (int i = 1; i < numIterations; i++)
				{
					float alpha = i / (float)numIterations;

					Vector3 pos = LinePath.GetPoint(alpha);
					Vector3 normal = LinePath.GetNormal(alpha);

					Handles.color = Color.green;
					Handles.DrawLine(pos, pos + normal * 0.4f);
				}
			}
		}

		protected virtual Vector3 ShowPoint(int index)
		{
			Vector3 point = LinePath.GetPoint(index);
			float size = HandleUtility.GetHandleSize(point);
			if (index == 0)
				size *= 1.7f;

			if (Event.current.control)
				Handles.color = Color.red;
			else
				Handles.color = Color.white;

			Vector3 toCamera = Camera.current.transform.position - point;
			Quaternion rot = Quaternion.identity;
			if(toCamera != Vector3.zero)
				rot = Quaternion.LookRotation(toCamera);

			Handles.DrawSolidDisc(point, toCamera, size * handleSize);

			if (Handles.Button(point, rot, size * handleSize, size * pickSize, Handles.CircleHandleCap))
			{
				if(Event.current.control)
				{
					if (LinePath.PointCount > 2)
					{
						Undo.RecordObject(LinePath, "Remove Control");
						EditorUtility.SetDirty(LinePath);
						LinePath.RemoveControl(index);
						return point;
					}
				}
				else
				{
					selectedIndex = index;
					Repaint();
				}
			}

			if (selectedIndex == index)
			{
				Tools.current = Tool.Move;

				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, Quaternion.identity);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(LinePath, "Move Point");
					EditorUtility.SetDirty(LinePath);

					LinePath.SetControlPoint(index, point);
				}
			}

			return point;
		}
	}
}