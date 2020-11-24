using System.Collections;
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

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				position.height = EditorGUIUtility.singleLineHeight;

				int requirement = (int)(EventParameterBase.Requirement)EditorGUI.EnumPopup(position,
					new GUIContent(requirementProp.displayName),
					(EventParameterBase.Requirement)requirementProp.enumValueIndex);

				position.y += EditorGUIUtility.singleLineHeight;

				if (requirementProp.enumValueIndex != requirement)
					requirementProp.enumValueIndex = requirement;

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();
			}

			if (requirementProp.enumValueIndex != (int)EventParameterBase.Requirement.Any)
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var argProp = property.FindPropertyRelative("value");
					if (argProp != null)
					{
						if (type != null && (type == unityObjType || type.IsSubclassOf(unityObjType)))
						{
							EditorGUI.ObjectField(position, argProp, type, new GUIContent(displayName));
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

			EditorGUI.indentLevel--;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUI.GetPropertyHeight(property, false);

		if (property.isExpanded)
		{
			var requirementProp = property.FindPropertyRelative("requirement");
			height += EditorGUIUtility.singleLineHeight;

			if (requirementProp.enumValueIndex != (int)EventParameterBase.Requirement.Any)
			{
				var argProp = property.FindPropertyRelative("value");
				if (argProp != null)
					height += EditorGUI.GetPropertyHeight(argProp, argProp.isExpanded);
			}
		}

		return height;
	}
}