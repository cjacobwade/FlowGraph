using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class FlowFunctionNameAttribute : PropertyAttribute
{
	private string moduleField = string.Empty;
	public string ModuleField => moduleField;

	public FlowFunctionNameAttribute(string moduleField)
	{
		this.moduleField = moduleField;
	}
}
