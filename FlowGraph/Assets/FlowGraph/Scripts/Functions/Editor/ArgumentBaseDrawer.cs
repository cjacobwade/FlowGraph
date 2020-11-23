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
	private readonly Type unityObjType = typeof(UnityEngine.Object);

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

				var easyTypeLabel = new GUIContent(string.Format("{0} ({1})", label.text, displayName));

				Type elementType = null;
				if (type != null && type.IsArray)
					elementType = type.GetElementType();

				if (type != null && (type == unityObjType || type.IsSubclassOf(unityObjType)))
				{
					EditorGUI.ObjectField(position, argProp, type, easyTypeLabel);
				}
				else if(type != null && elementType != null &&
					(type == unityObjType || elementType.IsSubclassOf(unityObjType)))
				{
					EditorGUI.PropertyField(position, argProp, easyTypeLabel, false);
					position.y += EditorGUI.GetPropertyHeight(argProp, false);

					if (argProp.isExpanded)
					{
						EditorGUI.indentLevel++;
						EditorGUI.indentLevel++;

						int arraySize = Mathf.Max(0, EditorGUI.IntField(position, new GUIContent("Size"), argProp.arraySize));
						if (arraySize != argProp.arraySize)
							argProp.arraySize = arraySize;

						position.y += EditorGUIUtility.singleLineHeight;

						for (int i = 0; i < arraySize; i++)
						{
							var elementProp = argProp.GetArrayElementAtIndex(i);
							position.height = EditorGUI.GetPropertyHeight(elementProp, true);

							EditorGUI.ObjectField(position, elementProp, elementType, new GUIContent(elementProp.displayName));
							position.y += position.height;
						}

						EditorGUI.indentLevel--;
						EditorGUI.indentLevel--;
					}
				}
				else
				{
					EditorGUI.PropertyField(position, argProp, easyTypeLabel, true);
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
			return EditorGUI.GetPropertyHeight(argProp, argProp.isExpanded);
		else
			return 0f;
	}
}