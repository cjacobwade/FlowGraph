using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GoalData))]
public class GoalDataEditor : Editor
{
	private GoalData GoalData => target as GoalData;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if(Application.isPlaying && GoalManager.Instance != null)
		{
			if (GoalManager.Instance.IsGoalComplete(GoalData))
				GUI.enabled = false;

			if(GUILayout.Button("Complete Goal"))
			{
				GoalManager.Instance.CompleteGoal(GoalData);
			}
		}
	}
}
