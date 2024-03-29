﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.Profiling;

[CustomPropertyDrawer(typeof(EventParameterBase), true)]
public class EventParameterBaseDrawer : PropertyDrawer
{
	private readonly Type unityObjType = typeof(UnityEngine.Object);

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		var typeProp = property.FindPropertyRelative("type");
		if (typeProp != null)
		{
			string displayName = typeProp.stringValue;

			Type type = Type.GetType(typeProp.stringValue);
			if (type != null)
				displayName = type.Name;

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				position.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(position, property, new GUIContent(string.Format("{0} ({1})", label.text, displayName)), false);
				position.y += EditorGUIUtility.singleLineHeight;

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();
			}

			if (property.isExpanded)
			{
				EditorGUI.indentLevel++;

				var requirementProp = property.FindPropertyRelative("requirement");
				if (requirementProp != null)
				{
					using (var check = new EditorGUI.ChangeCheckScope())
					{
						position.height = EditorGUIUtility.singleLineHeight;

						int requirement = (int)(EventParameterBase.Requirement)EditorGUI.EnumPopup(position,
							new GUIContent(requirementProp.displayName),
							(EventParameterBase.Requirement)requirementProp.intValue);

						position.y += EditorGUIUtility.singleLineHeight;

						if (requirementProp.intValue != requirement)
							requirementProp.intValue = requirement;

						if (GUI.changed)
							property.serializedObject.ApplyModifiedProperties();
					}

					if (requirementProp.intValue != (int)EventParameterBase.Requirement.Any)
					{
						using (var check = new EditorGUI.ChangeCheckScope())
						{
							var argProp = property.FindPropertyRelative("value");
							if (argProp != null)
							{
								if (type != null && typeof(UnityEngine.Object).IsAssignableFrom(type))
								{
									bool allowSceneObjects = argProp.serializedObject.targetObject.GetType() == typeof(MonoBehaviour);
									UnityEngine.Object obj = EditorGUI.ObjectField(position, displayName, argProp.objectReferenceValue, type, allowSceneObjects);
									if (argProp.objectReferenceValue != obj)
										argProp.objectReferenceValue = obj;
								}
								else
								{
									EditorGUI.PropertyField(position, argProp, new GUIContent(displayName), true);
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
					}
				}

				EditorGUI.indentLevel--;
			}
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUI.GetPropertyHeight(property, false);

		if (property.isExpanded)
		{
			var requirementProp = property.FindPropertyRelative("requirement");
			if (requirementProp != null)
			{
				height += EditorGUIUtility.singleLineHeight;

				if (requirementProp.intValue != (int)EventParameterBase.Requirement.Any)
				{
					var argProp = property.FindPropertyRelative("value");
					if (argProp != null)
						height += EditorGUI.GetPropertyHeight(argProp, argProp.isExpanded);
				}
			}
		}

		return height;
	}
}