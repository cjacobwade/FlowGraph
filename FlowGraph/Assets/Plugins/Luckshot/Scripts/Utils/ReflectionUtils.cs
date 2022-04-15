using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public static class ReflectionUtils
{
	// TODO: Do this betterly
	// hardcoded list of assemblies for core and plugin stuff
	// note that asmdef things won't be included here
	private static List<string> projectAssemblies = new List<string>()
	{
		"Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
		"Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" ,
		"Assembly-CSharp-Editor-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" ,
		"Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
	};

	public static List<string> ProjectAssemblies => projectAssemblies;

	public static string SimplifyTypeName(string name)
    {
		int index = name.LastIndexOf(',');
		return (index > 0) ? name.Substring(0, index).Trim() : name;
	}

	public static System.Object DoInvoke(Type type, string methodName, System.Object[] parameters)
	{
		Type[] types = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
			types[i] = parameters[i].GetType();

		MethodInfo method = type.GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public), null, types, null);
		return DoInvoke2(type, method, parameters);
	}

	public static System.Object DoInvoke2(Type type, MethodInfo method, System.Object[] parameters)
	{
		if (method.IsStatic)
			return method.Invoke(null, parameters);

		System.Object obj = type.InvokeMember(null,
		BindingFlags.DeclaredOnly |
		BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Instance | BindingFlags.CreateInstance, null, null, new System.Object[0]);

		return method.Invoke(obj, parameters);
	}
}