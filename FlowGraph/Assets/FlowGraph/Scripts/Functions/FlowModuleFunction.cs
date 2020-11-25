using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

[System.Serializable]
public class FlowModuleFunction 
{
	[System.Serializable]
	public class Outcome
	{
		public string name = string.Empty;
		public int nextNodeID = 0;
	}

	public FlowModuleFunction()
	{
		if(FlowTypeCache.FlowModuleSettings != null)
			context = FlowTypeCache.FlowModuleSettings.GetDefaultUOD(module);
	}

	public UniqueObjectData context = null;

    public string module = "FlowModule_Core";
    public string function = "GOTO";

	[SerializeReference]
    public List<ArgumentBase> arguments = new List<ArgumentBase>();

    public List<Outcome> outcomes = new List<Outcome>();

	public void Invoke(FlowEffectInstance effect = null)
	{
		System.Type moduleType = FlowTypeCache.GetModuleType(module);
		if (moduleType != null)
		{
			UniqueObject uniqueObject = UniqueObjectManager.Instance.LookupUniqueObject(context);
			if(uniqueObject != null)
			{
				Component moduleComponent = uniqueObject.gameObject.GetComponent(moduleType);
				if(moduleComponent != null)
				{
					MethodInfo methodInfo = FlowTypeCache.GetModuleFunction(module, function);
					ParameterInfo[] paramsInfo = methodInfo.GetParameters();

					object[] argumentArr = new object[arguments.Count + 1];
					argumentArr[0] = effect;

					for (int i = 1; i < argumentArr.Length; i++)
					{
						object value = arguments[i - 1].Value;

						System.Type paramType = paramsInfo[i].ParameterType;
						if (paramType.IsSubclassOf(typeof(Object)))
						{
							if(((Object)value) != null)
								argumentArr[i] = value;
						}
						else
						{
							argumentArr[i] = value;
						}
					}

					methodInfo.Invoke(moduleComponent, argumentArr);
				}
				else
				{
					Debug.LogErrorFormat("Module function execute failed. {0} not found on {1}", moduleType, uniqueObject.name, uniqueObject);
				}
			}
			else
			{
				Debug.LogErrorFormat("Module function execute failed. No uniqueObject found matching {0}", context);
			}
		}
		else
		{
			Debug.LogErrorFormat("Module function execute failed. {0}.{1} function lookup failed", module, function);
		}
	}
}
