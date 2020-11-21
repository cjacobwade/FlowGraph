using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.Profiling;

[CustomPropertyDrawer(typeof(ArgumentBase), true)]
public class ArgumentBaseDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Profiler.BeginSample("Argument Base Drawer");

		using (var check = new EditorGUI.ChangeCheckScope())
		{

			var argProp = property.FindPropertyRelative("value");
			if (argProp != null)
			{
				var typeProp = property.FindPropertyRelative("type");

				string displayName = typeProp.stringValue;

				Type type = Type.GetType(typeProp.stringValue);
				if (type != null)
					displayName = type.Name;

				var guiContent = new GUIContent(string.Format("{0} ({1})", label.text, displayName));

				if (type != null && (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object))))
				{
					EditorGUI.ObjectField(position, argProp, type, guiContent);
				}
				else
				{
					EditorGUI.PropertyField(position, argProp, guiContent, true);
				}
			}
			else
			{

			}

			if (GUI.changed)
			{
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		Profiler.EndSample(); // Argument Base Drawer
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var argProp = property.FindPropertyRelative("value");
		if (argProp != null)
			return EditorGUI.GetPropertyHeight(argProp);
		else
			return 0f;
	}
}