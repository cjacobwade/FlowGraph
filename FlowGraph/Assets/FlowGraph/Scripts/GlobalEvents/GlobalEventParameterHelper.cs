using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class GlobalEventParameterHelper
{
	public static EventParameterBase GetArgumentOfType(Type type)
	{
		EventParameterBase parameter = null;

		if(type == typeof(bool))
		{
			parameter = new EventParameter_Bool();
		}
		else if(type == typeof(int))
		{
			parameter = new EventParameter_Int();
		}
		else if (type == typeof(float))
		{
			parameter = new EventParameter_Float();
		}
		else if (type == typeof(string))
		{
			parameter = new EventParameter_String();
		}
		else if (type == typeof(Vector2))
		{
			parameter = new EventParameter_Vector2();
		}
		else if (type == typeof(Vector3))
		{
			parameter = new EventParameter_Vector3();
		}
		else if (type == typeof(Vector4))
		{
			parameter = new EventParameter_Vector4();
		}
		else if (type == typeof(UnityEngine.Object) ||
			type.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			parameter = new EventParameter_Object();
		}

		if(parameter != null)
			parameter.type = type.AssemblyQualifiedName;
		else
			Debug.LogErrorFormat("Unsupported type passed to Argument: {0}", type.Name);

		return parameter;
	}
}
