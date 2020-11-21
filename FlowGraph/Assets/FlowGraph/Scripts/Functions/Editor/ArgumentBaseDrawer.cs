using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(ArgumentBase), true)]
public class ArgumentBaseDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.serializedObject.Update();

		EditorGUI.BeginChangeCheck();

		if (property.serializedObject.FindProperty(property.propertyPath) == null)
			return;

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

		if (EditorGUI.EndChangeCheck())
		{
			property.serializedObject.ApplyModifiedProperties();
		}
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