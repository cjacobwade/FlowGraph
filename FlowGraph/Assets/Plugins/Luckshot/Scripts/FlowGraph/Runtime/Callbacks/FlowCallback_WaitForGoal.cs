using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_WaitForGoal : FlowCallback
{
	private GoalData goalData = null;

	public FlowCallback_WaitForGoal(FlowEffectInstance effect, GoalData goalData) : base(effect)
	{
		this.goalData = goalData;
		GoalManager.Instance.OnGoalCompleted += GoalManager_OnGoalCompleted;
	}

	private void GoalManager_OnGoalCompleted(GoalData goalData)
	{
		if (this.goalData == goalData)
		{
			Cleanup();
			Complete();
		}
	}

	public override void Cancel()
	{
		base.Cancel();
		Cleanup();
	}

	private void Cleanup()
    {
		if (GoalManager.Instance != null)
			GoalManager.Instance.OnGoalCompleted -= GoalManager_OnGoalCompleted;
	}
}