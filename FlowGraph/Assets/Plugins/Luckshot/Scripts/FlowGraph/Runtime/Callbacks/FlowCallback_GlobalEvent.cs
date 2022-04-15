using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Dynamic;

public class FlowCallback_GlobalEvent : FlowCallback
{
	private GlobalEventDefinition eventDefinition = null;

	private EventInfo eventInfo = null;
	private Delegate genericDelegate = null;

	public FlowCallback_GlobalEvent(FlowEffectInstance effect, GlobalEventDefinition eventDefinition) : base(effect)
	{
		this.eventDefinition = eventDefinition;
		this.eventInfo = GlobalEvents.GetEventInfo(eventDefinition.typeName, eventDefinition.eventName);

		MethodInfo invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
		ParameterInfo[] parameterInfos = invokeMethod.GetParameters();

		int maxNumTypes = Mathf.Min(parameterInfos.Length, 3);
		if(parameterInfos.Length > maxNumTypes)
			Debug.LogError("More than 3 arguments in GlobalEvent not supported. Will use first 3 params for now");

		Type[] types = new Type[maxNumTypes];
		for (int i = 0; i < maxNumTypes; i++)
			types[i] = parameterInfos[i].ParameterType;

		string eventHandlerName = nameof(EventHandler);

		if (types.Length == 1)
			eventHandlerName = nameof(EventHandler1Arg);
		else if (types.Length == 2)
			eventHandlerName = nameof(EventHandler2Arg);
		else if (types.Length == 3)
			eventHandlerName = nameof(EventHandler3Arg);

		MethodInfo handlerMethod = typeof(FlowCallback_GlobalEvent).GetMethod(eventHandlerName);
		if (types.Length > 0)
			handlerMethod = handlerMethod.MakeGenericMethod(types);

		genericDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethod);
		eventInfo.AddEventHandler(null, genericDelegate);
	}

	public override void Cancel()
	{
		base.Cancel();

		if(eventInfo != null && genericDelegate != null)
			eventInfo.RemoveEventHandler(null, genericDelegate);
	}

	private bool EvaluateEventDefinition(params object[] values)
	{
		if (values == null)
			return true;

		for(int i = 0; i < eventDefinition.parameters.Count; i++)
		{
			EventParameterBase param = eventDefinition.parameters[i];
			if (param.requirement == EventParameterBase.Requirement.Any)
				continue;

			bool desiredEqual = param.requirement == EventParameterBase.Requirement.Equal;
			bool isEqual = param.Value.Equals(values[i]);

			if (desiredEqual != isEqual)
				return false;
		}

		return true;
	}

	private void TryReceiveEvent(object[] parameters = null)
	{
		if (EvaluateEventDefinition(parameters))
		{
			Debug.Log("Event Complete");
			eventInfo.RemoveEventHandler(null, genericDelegate);
			Complete();
		}
	}

	public void EventHandler() { TryReceiveEvent(); }
	public void EventHandler1Arg<T>(T arg) { TryReceiveEvent(new object[] { arg }); }
	public void EventHandler2Arg<T, K>(T arg, K arg2) { TryReceiveEvent(new object[] { arg, arg2 }); }
	public void EventHandler3Arg<T, K, O>(T arg, K arg2, O arg3) { TryReceiveEvent(new object[] { arg, arg2, arg3 }); }
}
