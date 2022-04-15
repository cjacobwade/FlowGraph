using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : Singleton<GoalManager>
{
	[SerializeField]
	private List<GoalData> allGoals = new List<GoalData>();

	private GoalState[] goalStates = null;

	[SaveLoad]
	private GoalState[] GoalStates
	{
		get { return goalStates; }
		set
		{
			goalStates = value;
			for(int i =0; i < goalStates.Length; i++)
			{
				if (goalStates[i].isComplete)
				{
					CompleteGoalState(goalStates[i]);
				}
			}
		}
	}

	private Dictionary<GoalData, GoalState> goalDataToStateMap = new Dictionary<GoalData, GoalState>();
	private Dictionary<GoalData, List<GoalData>> goalDataToParentMap = new Dictionary<GoalData, List<GoalData>>();

	public event System.Action<GoalData> OnGoalCompleted = delegate { };

	[SerializeField]
	private GoalData[] freeplayGoals = null;

	protected override void Awake()
	{
		base.Awake();

		allGoals.AddRange(Resources.LoadAll<GoalData>("GoalDatas/"));

		goalStates = new GoalState[allGoals.Count];
		for(int i = 0; i < goalStates.Length; i++)
		{
			goalStates[i] = new GoalState(allGoals[i]);
			goalDataToStateMap.Add(allGoals[i], goalStates[i]);
		}

		foreach(var goal in allGoals)
		{
			if (!goalDataToParentMap.ContainsKey(goal))
			{
				foreach (var subgoals in goal.subgoals)
				{
					List<GoalData> parentGoals = null;
					if (!goalDataToParentMap.TryGetValue(subgoals, out parentGoals))
					{
						parentGoals = new List<GoalData>();
						goalDataToParentMap.Add(subgoals, parentGoals);
					}

					if(!parentGoals.Contains(goal))
						parentGoals.Add(goal);
				}
			}
		}

		if (BigHopsPrefs.Instance.ProgressMode == ProgressMode.Freeplay)
		{
			foreach (var goal in freeplayGoals)
				CompleteGoal(goal);
		}
	}

	public bool IsGoalComplete(GoalData goalData)
	{ return goalDataToStateMap[goalData].isComplete; }

	public void CompleteGoal(GoalData goalData)
	{
		GoalState goalState = goalDataToStateMap[goalData];
		if (goalState != null && !goalState.isComplete)
		{
			CompleteGoalState(goalState);

			CompleteSubgoals(goalData);
			CompleteParentGoals(goalData);
		}
	}

	private void CompleteSubgoals(GoalData goalData)
	{
		foreach (var subGoal in goalData.subgoals)
			CompleteGoal(goalData);
	}

	private void CompleteParentGoals(GoalData goalData)
	{
		List<GoalData> parentGoals = new List<GoalData>();
		if(goalDataToParentMap.TryGetValue(goalData, out parentGoals))
		{
			foreach(var parentGoal in parentGoals)
			{
				if (IsGoalComplete(parentGoal))
					continue;

				bool allComplete = true;
				foreach(var parentSubgoal in parentGoal.subgoals)
				{
					if(!IsGoalComplete(parentSubgoal))
					{
						allComplete = false;
						break;
					}
				}

				if (allComplete)
					CompleteGoal(parentGoal);
			}
		}
	}

	internal void CompleteGoalState(GoalState goalState)
	{
		goalState.isComplete = true;
		OnGoalCompleted(goalState.goalData);
	}
}

[System.Serializable]
public class GoalState
{
	public GoalData goalData = null;
	public bool isComplete = false;

	public GoalState(GoalData inGoalData)
	{
		goalData = inGoalData;
		isComplete = false;
	}
}
