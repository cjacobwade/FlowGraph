using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;


[InitializeOnLoad]
public static class PropertyDrawerFinder
{
	private static Dictionary<string, PropertyDrawer> customDrawers = new Dictionary<string, PropertyDrawer>();
	private static List<Type> propertyDrawerTypes = new List<Type>();

	private static readonly FieldInfo typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
	private static readonly FieldInfo childField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);

	static PropertyDrawerFinder()
	{
		propertyDrawerTypes.Clear();
		customDrawers.Clear();
	}

	public static PropertyDrawer Find(SerializedProperty property)
	{
		Type propertyType = EditorUtils.GetTypeOfProperty(property);

		if (!customDrawers.TryGetValue(property.propertyPath, out PropertyDrawer drawer) || drawer == null)
		{
			drawer = PropertyDrawerFinder.Find(propertyType);
			customDrawers[property.propertyPath] = drawer;
		}
		else if(drawer != null)
		{
			var attribute = drawer.GetType().GetCustomAttribute(typeof(CustomPropertyDrawer));
			Type drawerType = (Type)typeField.GetValue(attribute);
			if (drawerType != propertyType)
			{
				drawer = PropertyDrawerFinder.Find(propertyType);
				customDrawers[property.propertyPath] = drawer;
			}
		}

		return drawer;
	}

	public static PropertyDrawer Find(Type propertyType)
	{
		if (propertyDrawerTypes.Count == 0)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!ReflectionUtils.ProjectAssemblies.Contains(assembly.FullName))
					continue;

				foreach (Type candidate in assembly.GetTypes())
				{
					foreach (Attribute a in candidate.GetCustomAttributes(typeof(CustomPropertyDrawer)))
					{
						if (a.GetType().IsSubclassOf(typeof(CustomPropertyDrawer)) ||
							a.GetType() == typeof(CustomPropertyDrawer))
						{
							propertyDrawerTypes.Add(candidate);
						}
					}
				}
			}
		}

		foreach(var type in propertyDrawerTypes)
		{
			foreach (Attribute a in type.GetCustomAttributes(typeof(CustomPropertyDrawer)))
			{
				if (a.GetType().IsSubclassOf(typeof(CustomPropertyDrawer)) || 
					a.GetType() == typeof(CustomPropertyDrawer))
				{
					CustomPropertyDrawer drawerAttribute = (CustomPropertyDrawer)a;
					Type drawerType = (Type)typeField.GetValue(drawerAttribute);
					if (drawerType == propertyType ||
						((bool)childField.GetValue(drawerAttribute) && propertyType.IsSubclassOf(drawerType)) ||
						((bool)childField.GetValue(drawerAttribute) && IsGenericSubclass(drawerType, propertyType)))
					{
						if (type.IsSubclassOf(typeof(PropertyDrawer)))
						{
							return (PropertyDrawer)Activator.CreateInstance(type);
						}
					}
				}
			}
		}

		return null;
	}

	private static bool IsGenericSubclass(Type parent, Type child)
	{
		if (!parent.IsGenericType)
			return false;

		Type currentType = child;
		bool isAccessor = false;
		while (!isAccessor && currentType != null)
		{
			if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == parent.GetGenericTypeDefinition())
			{
				isAccessor = true;
				break;
			}
			currentType = currentType.BaseType;
		}
		return isAccessor;
	}
}