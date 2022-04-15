using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlowModule), true)]
public class FlowModuleEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		FlowModule flowModule = target as FlowModule;

		if (Application.IsPlaying(flowModule))
		{
			var so = serializedObject;
			so.Update();
			
			var functionProp = so.FindProperty("function");

			var contextProp = functionProp.FindPropertyRelative("context");
			var contextValueProp = contextProp.FindPropertyRelative("value");

			UniqueObject uo = flowModule.GetComponent<UniqueObject>();
			if (uo != null)
			{
				if (contextValueProp.objectReferenceValue != uo.Data)
				{
					contextValueProp.objectReferenceValue = uo.Data;
					so.ApplyModifiedProperties();
				}
			}

			var moduleProp = functionProp.FindPropertyRelative("module");

			var typeIter = flowModule.GetType();
			List<string> typeNames = new List<string>();
			while(typeIter != typeof(FlowModule))
			{
				typeNames.Add(typeIter.Name);
				typeIter = typeIter.BaseType;
			}

			if (!typeNames.Contains(moduleProp.stringValue))
			{
				moduleProp.stringValue = flowModule.GetType().Name;
				so.ApplyModifiedProperties();
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(functionProp, new GUIContent(functionProp.displayName), true);

			if (EditorGUI.EndChangeCheck())
				so.ApplyModifiedProperties();

			if (GUILayout.Button("Run Function"))
			{
				flowModule.Function.Invoke();
			}
		}
	}
}
