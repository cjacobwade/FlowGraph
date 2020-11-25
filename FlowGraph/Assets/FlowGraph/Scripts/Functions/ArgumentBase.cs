using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ArgumentBase
{
	// Prevent instantiating base class
	protected ArgumentBase()
	{ }

	public string name = string.Empty;
	public string type = string.Empty;

	public virtual object Value 
	{ get; set; }
}

public class Argument<T> : ArgumentBase
{
	public Argument() : base()
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
public class Argument_Bool : Argument<bool> { }

[Serializable]
public class Argument_Int : Argument<int> { }

[Serializable]
public class Argument_Float : Argument<float> { }

[Serializable]
public class Argument_String : Argument<string> { }

[Serializable]
public class Argument_Color : Argument<Color> { }

[Serializable]
public class Argument_Vector2 : Argument<Vector2> { }

[Serializable]
public class Argument_Vector3 : Argument<Vector3> { }

[Serializable]
public class Argument_Vector4 : Argument<Vector4> { }

[Serializable]
public class Argument_Object : Argument<UnityEngine.Object> { }

[Serializable]
public class Argument_ObjectArray : Argument<UnityEngine.Object[]> { }

[Serializable]
public class Argument_GlobalEventDefintion : Argument<GlobalEventDefinition>
{ 
	public Argument_GlobalEventDefintion()
	{
		Value = new GlobalEventDefinition();
	}
}
