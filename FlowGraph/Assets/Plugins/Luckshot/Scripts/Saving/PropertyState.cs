using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Dynamic;
using System.Reflection;
using System;

[System.Serializable]
public class PropertyState
{
	[System.Serializable]
	public struct Field
	{
		public string name;
		public string type;
		public string value;
	}

	[System.Serializable]
	public class ArrWrapper : System.Object
	{
		public string type = string.Empty;
		public List<string> values = new List<string>();
	}

	public string type = string.Empty;
	public List<Field> properties = new List<Field>();
	public List<Field> fields = new List<Field>();

	private static BindingFlags allInstanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
	private static Type unityObjType = typeof(UnityEngine.Object);

	public void ApplyStateToPropertyItem(PropertyItem propertyItem)
	{
		Type propertyItemType = propertyItem.GetType();
		for (int i = 0; i < properties.Count; i++)
		{
			Field propertyProperty = properties[i];
			PropertyInfo property = propertyItemType.GetProperty(propertyProperty.name, allInstanceBindingFlags);

			if (property == null ||
				property.SetMethod == null)
			{
				continue;
			}

			if (string.IsNullOrEmpty(propertyProperty.value))
			{
				property.SetValue(propertyItem, GetDefaultValue(Type.GetType(propertyProperty.type)));
			}
			else
			{
				Type propertyType = Type.GetType(propertyProperty.type);
				object value = StringToValue(propertyProperty.value, propertyType);
				property.SetValue(propertyItem, value);
			}
		}

		for (int i = 0; i < fields.Count; i++)
		{
			Field propertyField = fields[i];
			FieldInfo field = propertyItemType.GetField(propertyField.name, allInstanceBindingFlags);

			if (string.IsNullOrEmpty(propertyField.value))
			{
				field.SetValue(propertyItem, GetDefaultValue(Type.GetType(propertyField.type)));
			}
			else
			{
				Type fieldType = Type.GetType(propertyField.type);
				object value = StringToValue(propertyField.value, fieldType);
				field.SetValue(propertyItem, value);
			}
		}

		propertyItem.OnLoaded();
	}

	public static string ValueToString(object value)
	{
		if (value == null)
			return string.Empty;

		Type type = value.GetType();

		if (type.IsArray)
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
		else if (type == typeof(string))
		{
			return value.ToString();
		}
		else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
		{
			if (unityObjType.IsAssignableFrom(type))
				return ((UnityEngine.Object)value).name;

			return JsonUtility.ToJson(Convert.ChangeType(value, type));
		}
		else
		{
			return value.ToString();
		}
	}

	public static object StringToValue(string text, Type type)
	{
		if (type.IsArray)
		{
			ArrWrapper wrapper = JsonUtility.FromJson<ArrWrapper>(text);

			Type elementType = Type.GetType(wrapper.type).GetElementType();
			Array arr = Array.CreateInstance(elementType, wrapper.values.Count);
			for (int i = 0; i < wrapper.values.Count; i++)
				arr.SetValue(StringToValue(wrapper.values[i], elementType), i);

			return arr;
		}
		else if (type == typeof(string))
		{
			return Convert.ChangeType(text, type);
		}
		else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
		{
			if (unityObjType.IsAssignableFrom(type))
			{
				var attrib = type.GetCustomAttribute(typeof(SaveLoadAssetAttribute)) as SaveLoadAssetAttribute;
				if (attrib != null)
					return Resources.Load(attrib.resourcePath + text, type);
			}

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

	public object this[string fieldName]
	{
		get
		{
			for(int i = 0; i < fields.Count; i++)
			{
				var field = fields[i];
				if(field.name == fieldName)
				{
					Type propertyType = Type.GetType(field.type);
					object value = StringToValue(field.value, propertyType);
					return value;
				}
			}

			for(int i = 0;i < properties.Count; i++)
			{
				var property = properties[i];
				if (property.name == fieldName)
				{
					Type fieldType = Type.GetType(property.type);
					object value = StringToValue(property.value, fieldType);
					return value;
				}
			}

			return null;
		}
	}
}

public static class PropertyStateUtils
{
	public static PropertyState BuildPropertyState(this PropertyItem propertyItem)
	{
		PropertyState propertyState = new PropertyState();

		Type propertyItemType = propertyItem.GetType();
		propertyState.type = propertyItemType.Name;

		BindingFlags allInstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		while (propertyItemType != typeof(MonoBehaviour))
		{
			PropertyInfo[] properties = propertyItemType.GetProperties(allInstanceBindingFlags);
			for (int i = 0; i < properties.Length; i++)
			{
				if (properties[i].GetMethod == null)
					continue;

				if (Attribute.IsDefined(properties[i], typeof(SaveLoadAttribute)))
				{
					PropertyState.Field property = new PropertyState.Field();
					property.name = properties[i].Name;
					property.type = properties[i].PropertyType.AssemblyQualifiedName;
					property.value = PropertyState.ValueToString(properties[i].GetValue(propertyItem));

					propertyState.properties.Add(property);
				}
			}

			FieldInfo[] fields = propertyItemType.GetFields(allInstanceBindingFlags);
			for (int i = 0; i < fields.Length; i++)
			{
				if (Attribute.IsDefined(fields[i], typeof(SaveLoadAttribute)))
				{
					PropertyState.Field field = new PropertyState.Field();
					field.name = fields[i].Name;
					field.type = fields[i].FieldType.AssemblyQualifiedName;
					field.value = PropertyState.ValueToString(fields[i].GetValue(propertyItem));

					propertyState.fields.Add(field);
				}
			}

			propertyItemType = propertyItemType.BaseType;
		}

		return propertyState;
	}
}