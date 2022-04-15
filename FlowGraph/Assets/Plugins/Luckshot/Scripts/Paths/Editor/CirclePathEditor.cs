using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CirclePath))]
public class CirclePathEditor : Editor
{
	private CirclePath CirclePath => target as CirclePath;

	private void OnEnable()
	{
		Undo.undoRedoPerformed += UndoRedoPerformed;
	}

	private void OnDisable()
	{
		Undo.undoRedoPerformed -= UndoRedoPerformed;
	}

	private void UndoRedoPerformed()
	{
		if (CirclePath != null)
			CirclePath.NotifyChanged();
	}

	public override void OnInspectorGUI()
	{
		float prevFillAmount = CirclePath.FillAmount;
		bool prevLoop = CirclePath.Loop;

		base.OnInspectorGUI();

		serializedObject.Update();

		if (GUI.changed)
		{
			var fillAmountProp = serializedObject.FindProperty("fillAmount");
			var loopProp = serializedObject.FindProperty("loop");

			if (CirclePath.Loop && CirclePath.FillAmount != 1f)
				fillAmountProp.floatValue = 1f;

			if (!CirclePath.Loop && CirclePath.FillAmount == 1f)
				fillAmountProp.floatValue = 0.99f;

			if (CirclePath.FillAmount == 1f && !CirclePath.Loop)
				loopProp.boolValue = true;

			if (CirclePath.FillAmount < 1f && CirclePath.Loop)
				loopProp.boolValue = false;

			serializedObject.ApplyModifiedProperties();
			CirclePath.NotifyChanged();
		}
	}

	protected virtual void OnSceneGUI()
	{
		Vector3 prevPathPos = CirclePath.GetPoint(0f);

		int iterations = 100;
		for (int i = 1; i <= iterations; i++)
		{
			Vector3 pathPos = CirclePath.GetPoint(i / (float)iterations);
			Handles.DrawLine(pathPos, prevPathPos);

			prevPathPos = pathPos;
		}

		int numIterations = 10 * Mathf.CeilToInt(CirclePath.CircleRadius);
		for (int i = 1; i < numIterations; i++)
		{
			float alpha = i / (float)numIterations;

			Vector3 pos = CirclePath.GetPoint(alpha);
			Vector3 normal = CirclePath.GetNormal(alpha);

			Handles.color = Color.green;
			Handles.DrawLine(pos, pos + normal * 0.4f);
		}
	}
}
