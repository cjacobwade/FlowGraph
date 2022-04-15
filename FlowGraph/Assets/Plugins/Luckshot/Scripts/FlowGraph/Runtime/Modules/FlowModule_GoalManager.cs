using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowModule_GoalManager : FlowModule
{
	public void CompleteGoal(FlowEffectInstance effect, GoalData goalData)
	{
		GoalManager.Instance.CompleteGoal(goalData);
		Complete(effect);
	}

	public void WaitForGoal(FlowEffectInstance effect, GoalData goalData)
	{
		if(GoalManager.Instance.IsGoalComplete(goalData))
		{
			Complete(effect);
		}
		else
		{
			new FlowCallback_WaitForGoal(effect, goalData);
		}
	}
}
