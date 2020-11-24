using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EventParameterBase
{
	protected EventParameterBase()
	{ }

	public enum Requirement
	{
		Any,
		Equal,
		NotEqual
	}

	public string name = string.Empty;
	public string type = string.Empty;

	public Requirement requirement = Requirement.Any;

	public virtual object Value
	{ get; set; }
}

public class EventParameter<T> : EventParameterBase
{
	public EventParameter() : base()
	{
		type = typeof(T).AssemblyQualifiedName;
	}

	[SerializeField]
	private T value = default;
	public override object Value
	{
		get { return value; }
		set { this.value = (T)value; }
	}
}

[Serializable]
public class EventParameter_Bool : EventParameter<bool> { }

[Serializable]
public class EventParameter_Int : EventParameter<int> { }

[Serializable]
public class EventParameter_Float : EventParameter<float> { }

[Serializable]
public class EventParameter_String : EventParameter<string> { }

[Serializable]
public class EventParameter_Color : EventParameter<Color> { }

[Serializable]
public class EventParameter_Vector2 : EventParameter<Vector2> { }

[Serializable]
public class EventParameter_Vector3 : EventParameter<Vector3> { }

[Serializable]
public class EventParameter_Vector4 : EventParameter<Vector4> { }

[Serializable]
public class EventParameter_Object : EventParameter<UnityEngine.Object> { }