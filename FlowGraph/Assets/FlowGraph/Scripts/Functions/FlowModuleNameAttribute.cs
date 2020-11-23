using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class FlowModuleNameAttribute : PropertyAttribute
{
	public FlowModuleNameAttribute()
	{ }
}
