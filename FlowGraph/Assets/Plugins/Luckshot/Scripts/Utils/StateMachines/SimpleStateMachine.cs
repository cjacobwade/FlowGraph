using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleStateMachine<T> where T : System.Enum
{
	private Dictionary<T, IEnumerator> enumValueToEnumeratorMap = new Dictionary<T, IEnumerator>();

	private T currentEnumValue = default;
	private Coroutine currentRoutine = null;

	private MonoBehaviour context = null;

	public void RegisterState(T enumValue, IEnumerator enumerator)
	{
		enumValueToEnumeratorMap[enumValue] = enumerator;
	}

	public void ChangeState(T enumValue)
	{
		if(!currentEnumValue.Equals(enumValue) || currentRoutine == null)
		{
			if (currentRoutine != null)
				context.StopCoroutine(currentRoutine);

			currentEnumValue = enumValue;
			currentRoutine = context.StartCoroutine(enumValueToEnumeratorMap[enumValue]);
		}
	}

	public void Setup(MonoBehaviour context)
	{
		this.context = context;
	}

	public void Stop()
	{
		if (currentRoutine != null)
		{
			context.StopCoroutine(currentRoutine);
			currentRoutine = null;
		}
	}
}
