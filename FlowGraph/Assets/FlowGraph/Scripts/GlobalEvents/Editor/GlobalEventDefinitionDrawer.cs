using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GlobalEventDefinition))]
public class GlobalEventDefinitionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		property.serializedObject.Update();

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

			int typeIndex = -1;
			int eventIndex = -1;

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				List<string> typeNames = new List<string>();
				foreach (var info in globalEventInfos)
					typeNames.Add(info.type.Name);

				typeIndex = Mathf.Max(0, typeNames.IndexOf(typeProp.stringValue));
				typeIndex = EditorGUI.Popup(position, typeProp.displayName, typeIndex, typeNames.ToArray());

				string typeName = typeNames[typeIndex];
				if (typeProp.stringValue != typeName)
					typeProp.stringValue = typeName;

				position.y += EditorGUIUtility.singleLineHeight;

				List<string> eventNames = new List<string>();
				foreach (var eventInfo in globalEventInfos[typeIndex].eventInfos)
					eventNames.Add(eventInfo.Name);
				
				eventIndex = Mathf.Max(0, eventNames.IndexOf(eventProp.stringValue));
				eventIndex = EditorGUI.Popup(position, eventProp.displayName, eventIndex, eventNames.ToArray());

				string eventName = eventNames[eventIndex];
				if (eventProp.stringValue != eventName)
					eventProp.stringValue = eventName;

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();
			}

			// Args

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;
		if (property.isExpanded)
			height += EditorGUIUtility.singleLineHeight * 2f;

		return height; 
	}
}
