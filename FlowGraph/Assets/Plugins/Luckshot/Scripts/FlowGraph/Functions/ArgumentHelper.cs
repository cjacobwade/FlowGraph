using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class ArgumentHelper
{
	private static Dictionary<Type, Type> typeToArgumentTypeMap = null;
	private static Dictionary<Type, Type> TypeToArgumentTypeMap
	{
		get
		{
			if (typeToArgumentTypeMap == null)
			{
				typeToArgumentTypeMap = new Dictionary<Type, Type>();

				Type argumentType = typeof(ArgumentBase);
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetExportedTypes())
					{
						if (argumentType.IsAssignableFrom(type) && type.BaseType != null)
						{
							Type[] genericArgs = type.BaseType.GetGenericArguments();
							if(genericArgs.Length > 0)
								typeToArgumentTypeMap[genericArgs[0]] = type;
						}
					}
				}
			}

			return typeToArgumentTypeMap;
		}
	}

	public static Type[] GetSourceTypes()
	{
		List<Type> types = new List<Type>();

		foreach (var kvp in TypeToArgumentTypeMap)
			types.Add(kvp.Key);

		return types.ToArray();
	}

	public static Type[] GetArgumentTypes()
	{
		List<Type> types = new List<Type>();

		foreach(var kvp in TypeToArgumentTypeMap)
			types.Add(kvp.Value);

		return types.ToArray();
	}

	private static readonly Type objectType = typeof(UnityEngine.Object);
	private static readonly Type objectArrayType = typeof(UnityEngine.Object[]);

	public static ArgumentBase GetArgumentOfType(Type type)
	{
		ArgumentBase argument = null;

		if (TypeToArgumentTypeMap.TryGetValue(type, out Type argumentType))
		{
			argument = (ArgumentBase)Activator.CreateInstance(argumentType, null);
		}
		else if (type.IsEnum)
		{
			argument = new Argument_Enum();
		}
		else if (objectType.IsAssignableFrom(type))
		{
			argument = new Argument_Object();
		}
		else if(type == objectArrayType || objectType.IsAssignableFrom(type.GetElementType()))
		{
			argument = new Argument_ObjectArray();
		}

		if (argument != null)
		{
			argument.type = type.AssemblyQualifiedName;
		}
		else
		{
			Debug.LogErrorFormat("Unsupported type passed to Argument: {0}", type.Name);
		}

		return argument;
	}
}
