
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(FlowFunctionNameAttribute))]
public class FlowFunctionNameAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		property.serializedObject.Update();

		FlowFunctionNameAttribute attrib = attribute as FlowFunctionNameAttribute;

		string modulePropPath = property.propertyPath;
		modulePropPath = modulePropPath.Replace(property.propertyPath.Split('.').Last(), attrib.ModuleField);

		bool validModule = false;
		SerializedProperty moduleProp = property.serializedObject.FindProperty(modulePropPath);
		if (moduleProp != null)
		{
			var moduleInfo = FlowTypeCache.GetModuleInfo(moduleProp.stringValue);
			if (moduleInfo != null)
			{
				List<string> functions = moduleInfo.MethodInfos.Select(m => m.Name).ToList();

				int functionIndex = Mathf.Max(0, functions.IndexOf(property.stringValue));
				functionIndex = EditorGUI.Popup(position, property.displayName, functionIndex, functions.ToArray());

				string functionName = functions[functionIndex];
				if (property.stringValue != functionName)
				{
					property.stringValue = functionName;
					GUI.changed = true;
				}

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();

				validModule = true;
			}
		}
		
		if(!validModule)
		{
			EditorGUI.LabelField(position, label, new GUIContent(string.Format("Invalid Module ({0})", attrib.ModuleField)), EditorStyles.helpBox);
		}

		EditorGUI.EndProperty();
	}
}
