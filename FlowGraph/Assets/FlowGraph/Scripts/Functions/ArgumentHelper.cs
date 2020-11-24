using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class ArgumentHelper
{
	public static ArgumentBase GetArgumentOfType(Type type)
	{
		ArgumentBase argument = null;

		if(type == typeof(bool))
		{
			argument = new Argument_Bool();
		}
		else if(type == typeof(int))
		{
			argument = new Argument_Int();
		}
		else if (type == typeof(float))
		{
			argument = new Argument_Float();
		}
		else if (type == typeof(string))
		{
			argument = new Argument_String();
		}
		else if (type == typeof(Vector2))
		{
			argument = new Argument_Vector2();
		}
		else if (type == typeof(Vector3))
		{
			argument = new Argument_Vector3();
		}
		else if (type == typeof(Vector4))
		{
			argument = new Argument_Vector4();
		}
		else if (type == typeof(Color))
		{
			argument = new Argument_Color();
		}
		else if (type == typeof(UnityEngine.Object) ||
			type.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			argument = new Argument_Object();
		}
		else if(type == typeof(UnityEngine.Object[]) ||
			type.GetElementType().IsSubclassOf(typeof(UnityEngine.Object)))
		{
			argument = new Argument_ObjectArray();
		}

		if(argument != null)
			argument.type = type.AssemblyQualifiedName;
		else
			Debug.LogErrorFormat("Unsupported type passed to Argument: {0}", type.Name);

		return argument;
	}
}
