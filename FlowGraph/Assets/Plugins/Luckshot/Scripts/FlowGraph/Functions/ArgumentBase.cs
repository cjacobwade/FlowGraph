using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public enum ArgumentSource
{
	Value,
	Template
}

[Serializable]
public class ArgumentBase : ICloneable
{
	// Prevent instantiating base class
	protected ArgumentBase() { }

	public string name = string.Empty;
	public string type = string.Empty;

	public ArgumentSource source = ArgumentSource.Value;
	public int templateIndex = 0;

	public int enumValue = 0;

	public virtual object Value 
	{ get; set; }

	public virtual object Clone()
	{ return null; }

	public object GetValueFromSource(FlowTemplate template, List<ArgumentBase> overrideArguments = null)
	{
		bool isEnum = this is Argument_Enum;
		object value = null;

		// TODO: it's weird that we're treating enums different like this
		// should be able to fix it on the ArgumentBase side of things

		if (source == ArgumentSource.Template)
		{
			ArgumentBase templateArg = null;
			if (overrideArguments != null)
				templateArg = overrideArguments[templateIndex];
			else
				templateArg = template.arguments[templateIndex];

			value = isEnum ? templateArg.enumValue : templateArg.Value;
		}
		else
		{
			value = isEnum ? enumValue : Value;
		}

		return value;
	}
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
		set
		{
			if (value != null &&
				value.GetType().IsEnum)
				enumValue = (int)value;

			this.value = (T)value;
		}
	}

	public override object Clone()
	{
		var constructor = GetType().GetConstructors()[0];
		var argument = (Argument<T>)constructor.Invoke(new object[]{});

		argument.name = name;
		argument.type = type;
		argument.source = source;
		argument.templateIndex = templateIndex;
		argument.enumValue = enumValue;
		argument.value = value;

		return argument;
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
public class Argument_Enum : Argument<Enum> { }

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
