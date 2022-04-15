using UnityEditor;
using UnityEngine;

namespace Luckshot.Paths
{
	[CustomEditor(typeof(SplinePath), true)]
	public class SplinePathEditor : Editor
	{
		protected const float handleSize = 0.07f;
		protected const float pickSize = 0.06f;

		protected SplinePath Spline => target as SplinePath;

		protected Quaternion handleRotation = Quaternion.identity;
		protected Vector3 handleScale = Vector3.one;
		protected static int selectedIndex = -1;

		private Tool currentTool = Tool.None;
		private PivotRotation currentPivotRotation = PivotRotation.Local;

		private static bool debugMode = false;

		private void OnEnable()
		{
			selectedIndex = -1;

			Undo.undoRedoPerformed += UndoRedoPerformed;

			if(!Application.IsPlaying(Spline))
				Spline.NotifyChanged();
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		private void UndoRedoPerformed()
		{
			ResetTransformGizmo();

			if (Spline != null)
				Spline.NotifyChanged();
			else
				Undo.undoRedoPerformed -= UndoRedoPerformed;
		}

		public override void OnInspectorGUI()
		{
			bool prevLoop = Spline.Loop;

			base.OnInspectorGUI();

			if (GUI.changed)
				Spline.NotifyChanged();

			if (Spline.ControlCount < 3)
			{
				if(Spline.Loop)
					Spline.SetLoop(false);

				EditorGUILayout.LabelField("Need at least 3 controls to loop", EditorStyles.helpBox);

				GUI.enabled = false;
				EditorGUILayout.Toggle("Loop", Spline.Loop);
				GUI.enabled = true;
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				bool loop = EditorGUILayout.Toggle("Loop", Spline.Loop);
				if (EditorGUI.EndChangeCheck())
				{
					Spline.SetLoop(loop);

					if (loop)
					{
						if (selectedIndex == 0)
						{
							Vector3 start = Spline.GetControlPoint(0);
							Spline.Points[Spline.PointCount - 1] = Spline.transform.InverseTransformPoint(start);
							Spline.EnforceAlignment(1);
						}
						else
						{
							Vector3 end = Spline.GetControlPoint(Spline.PointCount - 1);
							Spline.Points[0] = Spline.transform.InverseTransformPoint(end);
							Spline.EnforceAlignment(Spline.PointCount - 2);
						}
					}
				}
			}

			if (selectedIndex >= 0 && selectedIndex < Spline.PointCount)
				DrawSelectedPointInspector();

			debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);

			if (GUILayout.Button("Add Curve"))
			{
				Undo.RecordObject(Spline, "Add Curve");
				EditorUtility.SetDirty(Spline); 
				
				Spline.AddControl();
			}

			if (Spline.CurveCount > 1 && GUILayout.Button("Remove Curve"))
			{
				Undo.RecordObject(Spline, "Remove Curve");
				EditorUtility.SetDirty(Spline);

				Spline.RemoveControl();
			}

			bool anyModifiedScalars = false;
			for(int i = 0; i < Spline.Scalars.Count; i++)
            {
				if (Spline.Scalars[i] != Vector2.one)
				{
					anyModifiedScalars = true;
					break;
				}
            }

			if(anyModifiedScalars && GUILayout.Button("Reset Scalars"))
            {
				Undo.RecordObject(Spline, "Reset Scalars");
				EditorUtility.SetDirty(Spline);

				for (int i = 0; i < Spline.Scalars.Count; i++)
					Spline.Scalars[i] = Vector2.one;

				Spline.NotifyChanged();
            }
		}

		protected virtual void DrawSelectedPointInspector()
		{
			GUILayout.Label("Selected Point", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", Spline.GetControlPoint(selectedIndex));

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(Spline, "Move Point");
				EditorUtility.SetDirty(Spline);
				Spline.SetControlPoint(selectedIndex, point, true);
			}

			// Draw Node Data if ADV SPLINE
			// Would be in AdvSplineInspector, except AdvSpline is generic meaning
			// we can't use the CustomEditor attribute and force child types to use its inspector
			// which means either this is here or every AdvSpline child needs an associated TypeInspector script
			int advNodeIndex = selectedIndex / 3;
			SerializedObject so = serializedObject;
			SerializedProperty nodeDataProp = so.FindProperty(string.Format("{0}.Array.data[{1}]", "_nodeData", advNodeIndex));
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
			if (currentTool != Tools.current ||
				currentPivotRotation != Tools.pivotRotation)
			{ 
				currentTool = Tools.current;
				currentPivotRotation = Tools.pivotRotation;

				ResetTransformGizmo();
			}

			Vector3 p0 = ShowPoint(0);

			// Draw lines before controls
			for (int i = 1; i < Spline.PointCount; i += 3)
			{
				Vector3 p1 = Spline.GetControlPoint(i);
				Vector3 p2 = Spline.GetControlPoint(i + 1);
				Vector3 p3 = Spline.GetControlPoint(i + 2);

				Handles.color = Color.white.SetA(0.7f);
				Handles.DrawDottedLine(p0, p1, 5f);
				Handles.DrawDottedLine(p2, p3, 5f);

				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}

			for (int i = 1; i < Spline.PointCount; i += 3)
			{
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);

				p0 = p3;
			}

			int numMidpoints = Spline.CurveCount;
			float alphaPerControl = 1f / (float)numMidpoints;
			float midpointAlpha = alphaPerControl/2f;

			for (int i = 0; i < numMidpoints; i++)
			{
				Vector3 midpoint = Spline.GetPoint(midpointAlpha);
				Vector3 toCamera = Camera.current.transform.position - midpoint;

				float size = HandleUtility.GetHandleSize(midpoint);
				Handles.color = Color.cyan;
				Handles.DrawSolidDisc(midpoint, toCamera, size * handleSize);

				if (Handles.Button(midpoint, Quaternion.LookRotation(toCamera), size * handleSize, size * pickSize, Handles.CircleHandleCap))
				{
					Undo.RecordObject(Spline, "Insert Curve");
					EditorUtility.SetDirty(Spline);

					Spline.InsertControl(i, midpoint);

					selectedIndex = i * 3 + 3;

					ResetTransformGizmo();
					Repaint();
					return;
				}

				midpointAlpha += alphaPerControl;
			}

			if(	Event.current.control && 
				Event.current.keyCode == KeyCode.E &&
				Event.current.type == EventType.KeyDown)
			{
				Undo.RecordObject(Spline, "Add Curve");
				EditorUtility.SetDirty(Spline);

				Spline.AddControl();
				Event.current.Use();
			}

			if (debugMode)
			{
				// Draw forward / normal
				for (int i = 0; i < Spline.PointCount; i++)
				{
					if (i % 3 == 0)
					{
						Vector3 pos = Spline.GetControlPoint(i);

						float alpha = i / (float)(Spline.PointCount - 1);
						Vector3 direction = Spline.GetDirection(alpha);

						int endpointIndex = i / 3;
						Vector3 normal = Spline.GetNormal(alpha);

						Handles.color = Color.red;
						Handles.DrawLine(pos, pos + normal * 0.5f);

						Handles.color = Color.blue;
						Handles.DrawLine(pos, pos + direction * 0.5f);
					}
				}

				// Draw many normals
				int numIterations = 10 * Spline.PointCount;
				for (int i = 1; i < numIterations; i++)
				{
					float alpha = i / (float)numIterations;

					Vector3 pos = Spline.GetPoint(alpha);
					Vector3 normal = Spline.GetNormal(alpha);

					Handles.color = Color.white;
					Handles.DrawLine(pos, pos + normal * 0.25f);
				}
			}
		}

		private void ResetTransformGizmo()
		{
			if (selectedIndex == -1)
				return;

			handleScale = Vector3.one;

			float alpha = selectedIndex / (float)(Spline.PointCount - 1);
			Vector3 normal = Spline.GetNormal(alpha);

			Vector3 point = Spline.GetControlPoint(selectedIndex);

			Vector3 guidePos = Vector3.zero;
			if (selectedIndex == 0)
				guidePos = Spline.GetControlPoint(selectedIndex + 1);
			else
				guidePos = Spline.GetControlPoint(selectedIndex - 1);

			Vector3 toGuide = (guidePos - point).normalized;

			Vector3 right = Vector3.Cross(normal, toGuide).normalized;
			normal = Vector3.Cross(-right, toGuide).normalized;

			handleRotation = Quaternion.LookRotation(toGuide, normal);

			if (Tools.pivotRotation == PivotRotation.Global)
				handleRotation = Quaternion.identity;
		}

		protected virtual Vector3 ShowPoint(int index)
		{
			Vector3 point = Spline.GetControlPoint(index);
			float size = HandleUtility.GetHandleSize(point);
			if (index == 0)
				size *= 1.7f;

			if (index % 3 == 0)
			{
				if (Event.current.control)
					Handles.color = Color.red;
				else
					Handles.color = Color.white;
			}

			int controlIndex = (index + 1) / 3;
			Vector3 toCamera = Camera.current.transform.position - point;

			if (index % 3 == 0)
			{
				bool isLoop = index == 0 || index == Spline.PointCount - 1;
				bool loopSelected = selectedIndex == 0 || selectedIndex == Spline.PointCount - 1;

				if (Event.current.control || (selectedIndex != index && (!Spline.Loop || !loopSelected || !isLoop)))
				{
					Handles.DrawSolidDisc(point, toCamera, size * handleSize);

					if (Handles.Button(point, Quaternion.LookRotation(toCamera), size * handleSize, size * pickSize, Handles.CircleHandleCap))
					{
						if (Event.current.control)
						{
							if (Spline.CurveCount > 1)
							{
								Undo.RecordObject(Spline, "Remove Curve");
								EditorUtility.SetDirty(Spline);
								Spline.RemoveControl(controlIndex);
								return point;
							}
						}
						else
						{
							selectedIndex = index;
							ResetTransformGizmo();
							Repaint();
						}
					}
				}
			}
			else
			{
				Handles.color = Color.white.SetA(0.3f);
				Handles.DrawWireDisc(point, toCamera, size * handleSize);
			}

			if (selectedIndex == index)
			{
				if (Tools.current == Tool.Rotate)
				{
					EditorGUI.BeginChangeCheck();

					Quaternion newRotation = Handles.DoRotationHandle(handleRotation, point);
					Quaternion relativeRotation = newRotation * Quaternion.Inverse(handleRotation);
					handleRotation = newRotation;

					/*
					Handles.color = Color.green;
					Handles.DrawLine(point, point + handleRotation * Vector3.up);

					Handles.color = Color.blue;
					Handles.DrawLine(point, point + handleRotation * Vector3.forward);

					Handles.color = Color.red;
					Handles.DrawLine(point, point + handleRotation * Vector3.right);
					*/

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(Spline, "Rotate Point");
						EditorUtility.SetDirty(Spline);

						Vector3 normal = Spline.GetControlNormal(controlIndex);
						normal = relativeRotation * normal;
						Spline.SetControlNormal(controlIndex, normal);

						if (index == 0)
						{
							Vector3 p2 = Spline.GetControlPoint(index + 1);
							Vector3 toPos = relativeRotation * (p2 - point);
							Spline.SetControlPoint(index + 1, point + toPos, false);
						}
						else
						{
							Vector3 p1 = Spline.GetControlPoint(index - 1);
							Vector3 toPos = relativeRotation * (p1 - point);
							Spline.SetControlPoint(index - 1, point + toPos, false);
						}
					}

					return point;
				}
				else if (Tools.current == Tool.Scale)
				{
					EditorGUI.BeginChangeCheck();

					// This is to resolve a Unity bug that is storing current scale and multiplying the updated
					// scale against that
					if (Event.current.type == EventType.MouseDown)
						handleScale = new Vector3(Mathf.Sqrt(handleScale.x), 1f, Mathf.Sqrt(handleScale.z));

					Vector3 newScale = Handles.ScaleHandle(handleScale, point, handleRotation, HandleUtility.GetHandleSize(point));

					Vector3 scaleDiff = new Vector3(
						newScale.x / handleScale.x,
						newScale.y / handleScale.y,
						newScale.z / handleScale.z);

					handleScale = newScale;

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(Spline, "Scale Point");
						EditorUtility.SetDirty(Spline);

						if (index > 0)
						{
							Vector3 p1 = Spline.GetControlPoint(index - 1);
							Vector3 toPos = scaleDiff.z * (p1 - point);
							Spline.SetControlPoint(index - 1, point + toPos, true);							
						}

						if (index + 1 < Spline.PointCount)
						{
							Vector3 p2 = Spline.GetControlPoint(index + 1);
							Vector3 toPos2 = scaleDiff.z * (p2 - point);
							Spline.SetControlPoint(index + 1, point + toPos2, true);
						}

						Vector2 scalar = Spline.Scalars[controlIndex];
						scalar.Scale(scaleDiff);

						Spline.SetControlScalar(controlIndex, scalar);
					}


					return point;
				}
				else if (Tools.current == Tool.Move)
				{
					EditorGUI.BeginChangeCheck();

					point = Handles.DoPositionHandle(point, handleRotation);
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(Spline, "Move Point");
						EditorUtility.SetDirty(Spline);

						Spline.SetControlPoint(index, point, true);
					}
				}
			}

			return point;
		}
	}
}