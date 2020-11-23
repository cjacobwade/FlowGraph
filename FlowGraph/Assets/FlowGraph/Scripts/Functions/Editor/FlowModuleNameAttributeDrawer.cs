
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(FlowModuleNameAttribute))]
public class FlowModuleNameAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		property.serializedObject.Update();

		var moduleInfos = FlowTypeCache.GetModuleInfos();

		List<string> modules = moduleInfos.Select(m => m.typeInfo.Name).ToList();
		List<string> moduleDisplayNames = moduleInfos.Select(m => m.typeInfo.Name.Replace("FlowModule_", "")).ToList();

		int moduleIndex = Mathf.Max(0, modules.IndexOf(property.stringValue));
		moduleIndex = EditorGUI.Popup(position, property.displayName, moduleIndex, moduleDisplayNames.ToArray());

		string moduleName = modules[moduleIndex];
		if (property.stringValue != moduleName)
		{
			property.stringValue = moduleName;
			GUI.changed = true;
		}

		if (GUI.changed)
		{
			property.serializedObject.ApplyModifiedProperties();
			GUI.changed = false;
		}

		EditorGUI.EndProperty();
	}
}
