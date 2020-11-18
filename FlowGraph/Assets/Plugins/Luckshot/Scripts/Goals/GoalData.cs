using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoalData", menuName = "Luckshot/Goal Data")]
public class GoalData : ScriptableObject
{
	[SerializeField]
	public List<GoalData> subgoals = new List<GoalData>();
}
