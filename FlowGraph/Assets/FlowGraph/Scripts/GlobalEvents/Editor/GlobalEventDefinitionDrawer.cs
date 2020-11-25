using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(GlobalEventDefinition))]
public class GlobalEventDefinitionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		using (var check = new EditorGUI.ChangeCheckScope())
		{
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property, false);
			position.y += EditorGUIUtility.singleLineHeight;

			if (GUI.changed)
				property.serializedObject.ApplyModifiedProperties();
		}

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			var globalEventInfos = GlobalEvents.GetGlobalEventInfos();

			var typeProp = property.FindPropertyRelative("typeName");
			var eventProp = property.FindPropertyRelative("eventName");

			if (typeProp != null && eventProp != null)
			{
				int typeIndex = -1;
				int eventIndex = -1;

				bool changedEvent = false;

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					List<string> typeNames = new List<string>();
					foreach (var info in globalEventInfos)
						typeNames.Add(info.type.Name);

					typeIndex = Mathf.Max(0, typeNames.IndexOf(typeProp.stringValue));
					typeIndex = EditorGUI.Popup(position, typeProp.displayName, typeIndex, typeNames.ToArray());

					string typeName = typeNames[typeIndex];
					if (typeProp.stringValue != typeName)
					{
						typeProp.stringValue = typeName;
						changedEvent = true;
					}

					position.y += EditorGUIUtility.singleLineHeight;

					List<string> eventNames = new List<string>();
					foreach (var eventInfo in globalEventInfos[typeIndex].eventInfos)
						eventNames.Add(eventInfo.Name);

					eventIndex = Mathf.Max(0, eventNames.IndexOf(eventProp.stringValue));
					eventIndex = EditorGUI.Popup(position, eventProp.displayName, eventIndex, eventNames.ToArray());

					string eventName = eventNames[eventIndex];
					if (eventProp.stringValue != eventName)
					{
						eventProp.stringValue = eventName;
						changedEvent = true;
					}

					if (GUI.changed)
						property.serializedObject.ApplyModifiedProperties();
				}

				// Args

				var parametersProp = property.FindPropertyRelative("parameters");

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					Type classType = globalEventInfos[typeIndex].type;
					EventInfo eventInfo = globalEventInfos[typeIndex].eventInfos[eventIndex];

					ParameterInfo[] parameterInfos = eventInfo.EventHandlerType
						.GetMethod("Invoke")
						.GetParameters();

					// Changed module or function so lets refresh arguments
					if (changedEvent || parameterInfos.Length != parametersProp.arraySize)
					{
						var existingParams = (List<EventParameterBase>)EditorUtils.GetTargetObjectOfProperty(parametersProp);

						List<Type> types = new List<Type>();
						List<EventParameterBase> parameters = new List<EventParameterBase>();

						for (int i = 0; i < parameterInfos.Length; i++)
						{
							var parameter = GlobalEventParameterHelper.GetParameterOfType(parameterInfos[i].ParameterType);
							parameter.name = parameterInfos[i].Name;
							types.Add(parameterInfos[i].ParameterType);
							parameters.Add(parameter);
						}

						// Retain existing arguments if new arguments are same type
						for (int i = 0; i < parameters.Count && i < existingParams.Count; i++)
						{
							if (parameters[i] != null && existingParams[i] != null &&
								existingParams[i].Value != null &&
								existingParams[i].type == parameters[i].type)
							{
								parameters[i].Value = existingParams[i].Value;
							}
						}

						EditorUtils.SetTargetObjectOfProperty(parametersProp, parameters);
					}
				}

				parametersProp = property.FindPropertyRelative("parameters");
				if (parametersProp.arraySize > 0)
				{
					position.y += EditorGUIUtility.singleLineHeight;

					using (var check = new EditorGUI.ChangeCheckScope())
					{
						EditorGUI.PropertyField(position, parametersProp, false);

						if (parametersProp.isExpanded)
						{
							position.y += EditorGUIUtility.singleLineHeight;
							EditorGUI.indentLevel++;

							for (int i = 0; i < parametersProp.arraySize; i++)
							{
								var elementProp = parametersProp.GetArrayElementAtIndex(i);
								EditorGUI.PropertyField(position, elementProp, true);
								position.y += EditorGUI.GetPropertyHeight(elementProp, true);
							}

							EditorGUI.indentLevel--;
						}

						if (GUI.changed)
							property.serializedObject.ApplyModifiedProperties();
					}
				}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;
		if (property.isExpanded)
		{
			height += EditorGUIUtility.singleLineHeight * 2;

			var parametersProp = property.FindPropertyRelative("parameters");
			if (parametersProp != null && parametersProp.arraySize > 0)
			{
				height += EditorGUIUtility.singleLineHeight;

				if (parametersProp.isExpanded)
				{
					for(int i = 0; i < parametersProp.arraySize; i++)
					{
						var elementProp = parametersProp.GetArrayElementAtIndex(i);
						height += EditorGUI.GetPropertyHeight(elementProp, true);
					}
				}
			}
		}

		return height; 
	}
}
