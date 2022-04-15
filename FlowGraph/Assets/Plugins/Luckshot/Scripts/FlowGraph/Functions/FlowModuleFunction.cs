using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

using Object = UnityEngine.Object;

[System.Serializable]
public class FlowModuleFunction 
{
	public FlowModuleFunction(FlowModuleFunction other)
	{
		context = (Argument_Object)other.context.Clone();
		module = other.module;
		function = other.function;

		arguments = new List<ArgumentBase>();
		foreach (var argument in other.arguments)
			arguments.Add((ArgumentBase)argument.Clone());
	}

	public FlowModuleFunction()
	{
		if (context == null)
			context = (Argument_Object)ArgumentHelper.GetArgumentOfType(typeof(Argument_Object));

		if(context.Value == null && FlowTypeCache.FlowGraphSettings != null)
			context.Value = FlowTypeCache.FlowGraphSettings.GetDefaultUOD(module);
	}

	//public UniqueObjectData context = null;

	public Argument_Object context = new Argument_Object();

    public string module = "FlowModule_Core";
    public string function = "GOTO";

	[NonReorderable]
	[SerializeReference]
    public List<ArgumentBase> arguments = new List<ArgumentBase>();

	public void Invoke(FlowEffectInstance effectInstance = null)
	{
		Type moduleType = FlowTypeCache.GetModuleType(module);
		if (moduleType != null)
		{
			FlowTemplate template = null;
			List<ArgumentBase> directArguments = null;

			if (effectInstance != null)
			{
				template = effectInstance.Owner.Owner.Template;
				directArguments = effectInstance.Owner.Owner.DirectArguments;
			}

			UniqueObjectData contextUOD = (UniqueObjectData)context.GetValueFromSource(template, directArguments);

			UniqueObject uniqueObject = UniqueObjectManager.Instance.LookupUniqueObject(contextUOD);
			if(uniqueObject != null)
			{
				Component moduleComponent = uniqueObject.gameObject.GetComponent(moduleType);
				if(moduleComponent != null)
				{
					MethodInfo methodInfo = FlowTypeCache.GetModuleFunction(module, function);
					ParameterInfo[] paramsInfo = methodInfo.GetParameters();

					object[] argumentArr = new object[arguments.Count + 1];
					argumentArr[0] = effectInstance;

					FlowTemplate ownerTemplate = null;
					List<ArgumentBase> ownerDirectArguments = null;

					if(effectInstance != null)
					{
						ownerTemplate = effectInstance.Owner.Owner.Template;
						ownerDirectArguments = effectInstance.Owner.Owner.DirectArguments;
					}

					for (int i = 1; i < argumentArr.Length; i++)
					{
						var argument = arguments[i - 1];
						object value = argument.GetValueFromSource(template, directArguments);

						Type paramType = paramsInfo[i].ParameterType;
						Type elementType = paramType.IsArray ? paramType.GetElementType() : null;

						if (typeof(Object).IsAssignableFrom(paramType))
						{
							if(((Object)value) != null)
								argumentArr[i] = value;
						}
						else if(elementType != null && typeof(Object).IsAssignableFrom(elementType))
						{
							// make array of correct type
							var objectArr = (Object[])value;
							var typedArray = Array.CreateInstance(elementType, objectArr.Length);
							for (int j = 0; j < typedArray.Length; j++)
								typedArray.SetValue(objectArr[j], j);

							argumentArr[i] = typedArray;
						}
						else if(paramType.IsEnum)
                        {
							argumentArr[i] = (Enum)Enum.ToObject(paramType, value);
                        }
						else if(paramType == typeof(ItemStateDefinition))
                        {
							var itemStateDefinition = (ItemStateDefinition)value;
							var directArgCopy = itemStateDefinition.GetCopyWithDirectArguments(ownerTemplate, ownerDirectArguments);

							argumentArr[i] = directArgCopy;
                        }
						else if(paramType == typeof(PropertyItemStateDefinition))
                        {
							var propertyItemStateDefinition = (PropertyItemStateDefinition)value;
							var directArgCopy = propertyItemStateDefinition.GetCopyWithDirectArguments(ownerTemplate, ownerDirectArguments);

							argumentArr[i] = directArgCopy;
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
				Debug.LogErrorFormat("Module function execute failed. No uniqueObject found matching {0}", context.Value);
			}
		}
		else
		{
			Debug.LogErrorFormat("Module function execute failed. {0}.{1} function lookup failed", module, function);
		}
	}
}
