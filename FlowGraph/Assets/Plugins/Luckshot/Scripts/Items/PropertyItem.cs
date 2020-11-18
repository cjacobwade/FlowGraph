using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Luckshot.Callbacks;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(Item))]
public class PropertyItem : MonoBehaviour
{
	[System.Serializable]
	public class ArrWrapper : System.Object
	{
		public string type = string.Empty;
		public List<string> values = new List<string>();
	}

	private bool hasItem = false;

	private Item item = null;
	public Item Item
	{
		get 
		{
			if (!hasItem)
			{
				item = GetComponent<Item>();
				hasItem = item != null;
			}

			return item; 
		}
	}

	private static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public event Action<PropertyItem> OnPropertyItemAdded = delegate { };
	public event Action<PropertyItem> OnPropertyItemDestroyed = delegate { };

	public event Action<PropertyItem> OnStateChanged = delegate {};

	protected virtual void Awake()
	{
		if (!hasItem)
		{
			item = GetComponent<Item>();
			hasItem = item != null;
		}

		OnPropertyItemAdded(this);
		item.RegisterProperty(this);
	}

	protected virtual void OnLoaded()
	{
	}

	protected void StateChanged()
	{
		OnStateChanged(this);
	}

	public PropertyState BuildPropertyState()
	{
		PropertyState propertyState = new PropertyState();

		Type type = GetType();
		propertyState.type = type.Name;

		while (type != typeof(MonoBehaviour))
		{
			PropertyInfo[] properties = type.GetProperties(bindingFlags);
			for (int i = 0; i < properties.Length; i++)
			{
				if (properties[i].GetMethod == null)
					continue;

				if (Attribute.IsDefined(properties[i], typeof(SaveLoad)))
				{
					PropertyState.Field property = new PropertyState.Field();
					property.name = properties[i].Name;
					property.type = properties[i].PropertyType.AssemblyQualifiedName;
					property.value = ValueToString(properties[i].GetValue(this));

					propertyState.properties.Add(property);
				}
			}

			FieldInfo[] fields = type.GetFields(bindingFlags);
			for (int i = 0; i < fields.Length; i++)
			{
				if (Attribute.IsDefined(fields[i], typeof(SaveLoad)))
				{
					PropertyState.Field field = new PropertyState.Field();
					field.name = fields[i].Name;
					field.type = fields[i].FieldType.AssemblyQualifiedName;
					field.value = ValueToString(fields[i].GetValue(this));

					propertyState.fields.Add(field);
				}
			}

			type = type.BaseType;
		}

		return propertyState;
	}

	public void ApplyPropertyState(PropertyState state)
	{
		Type type = GetType();
		for (int i = 0; i < state.properties.Count; i++)
		{
			PropertyState.Field propertyProperty = state.properties[i];
			PropertyInfo property = type.GetProperty(propertyProperty.name, bindingFlags);

			if (property == null ||
				property.SetMethod == null)
			{
				continue;
			}

			if (string.IsNullOrEmpty(propertyProperty.value))
			{
				property.SetValue(this, GetDefaultValue(Type.GetType(propertyProperty.type)));
			}
			else
			{
				Type propertyType = Type.GetType(propertyProperty.type);
				object value = StringToValue(propertyProperty.value, propertyType);
				property.SetValue(this, value);
			}
		}

		for (int i = 0; i < state.fields.Count; i++)
		{
			PropertyState.Field propertyField = state.fields[i];
			FieldInfo field = type.GetField(propertyField.name, bindingFlags);

			if (string.IsNullOrEmpty(propertyField.value))
			{
				field.SetValue(this, GetDefaultValue(Type.GetType(propertyField.type)));
			}
			else
			{
				Type fieldType = Type.GetType(propertyField.type);
				object value = StringToValue(propertyField.value, fieldType);
				field.SetValue(this, value);
			}
		}

		OnLoaded();
	}

	public static string ValueToString(object value)
	{
		if (value == null)
			return string.Empty;

		Type type = value.GetType();

		if(type.IsArray)
		{
			ArrWrapper wrapper = new ArrWrapper();
			wrapper.type = value.GetType().AssemblyQualifiedName;

			int arrLength = 0;
			IEnumerable enumerable = value as IEnumerable;
			foreach (var obj in enumerable)
			{
				wrapper.values.Add(ValueToString(obj));
				arrLength++;
			}

			return JsonUtility.ToJson(wrapper);
		}
		else if(type == typeof(string))
		{
			return value.ToString();
		}
		else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
		{
			return JsonUtility.ToJson(Convert.ChangeType(value, type));
		}
		else
		{
			return value.ToString();
		}
	}

	public static object StringToValue(string text, Type type)
	{
		if(type.IsArray)
		{
			ArrWrapper wrapper = JsonUtility.FromJson<ArrWrapper>(text);

			Type elementType = Type.GetType(wrapper.type).GetElementType();
			Array arr = Array.CreateInstance(elementType, wrapper.values.Count);
			for(int i = 0; i < wrapper.values.Count; i++)
				arr.SetValue(StringToValue(wrapper.values[i], elementType), i);

			return arr;
		}
		else if(type == typeof(string))
		{
			return Convert.ChangeType(text, type);
		}
		else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
		{
			return JsonUtility.FromJson(text, type);
		}
		else
		{
			return Convert.ChangeType(text, type);
		}
	}

	object GetDefaultValue(Type t)
	{
		if (t.IsValueType)
			return Activator.CreateInstance(t);

		return null;
	}

	protected virtual void OnDestroy()
	{
		OnPropertyItemDestroyed(this);

		if(item != null)
			item.DeregisterProperty(this);
	}
}
