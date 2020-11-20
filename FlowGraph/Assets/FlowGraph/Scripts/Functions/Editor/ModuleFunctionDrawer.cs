using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomPropertyDrawer(typeof(ModuleFunction))]
public class ModuleFunctionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		property.serializedObject.Update();

		EditorGUI.BeginChangeCheck(); 
		
		position.height = EditorGUIUtility.singleLineHeight;

		EditorGUI.PropertyField(position, property, false);

		if (EditorGUI.EndChangeCheck())
		{
			property.serializedObject.ApplyModifiedProperties();
			return;
		}

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();

			position.y += EditorGUIUtility.singleLineHeight;

			var contextProp = property.FindPropertyRelative("context");
			EditorGUI.ObjectField(position, contextProp, typeof(UniqueObjectData));

			List<string> modules = new List<string>();
			List<string> moduleDisplayNames = new List<string>();

			var moduleInfos = FlowTypeCache.GetModuleInfos();
			foreach (var moduleInfo in moduleInfos)
			{
				modules.Add(moduleInfo.typeInfo.Name);
				moduleDisplayNames.Add(moduleInfo.typeInfo.Name.Replace("FlowModule_", ""));
			}

			var moduleProp = property.FindPropertyRelative("module");
			int moduleIndex = modules.IndexOf(moduleProp.stringValue);

			position.y += EditorGUIUtility.singleLineHeight;
			moduleIndex = EditorGUI.Popup(position, moduleProp.displayName, moduleIndex, moduleDisplayNames.ToArray());

			if (moduleIndex != -1 && modules.Count > moduleIndex)
				moduleProp.stringValue = modules[moduleIndex];

			var functionProp = property.FindPropertyRelative("function");

			int functionIndex = -1;

			List<string> functions = new List<string>();
			if (moduleIndex >= 0)
			{
				var selectedModuleInfo = moduleInfos[moduleIndex];
				foreach (var function in selectedModuleInfo.methodInfos)
					functions.Add(function.Name);

				functionIndex = functions.IndexOf(functionProp.stringValue);
			}

			position.y += EditorGUIUtility.singleLineHeight;
			functionIndex = EditorGUI.Popup(position, functionProp.displayName, functionIndex, functions.ToArray());

			MethodInfo methodInfo = null;

			if (functionIndex != -1 && functions.Count > functionIndex)
			{ 
				functionProp.stringValue = functions[functionIndex];
				methodInfo = moduleInfos[moduleIndex].methodInfos[functionIndex];
			}

			var argsProp = property.FindPropertyRelative("arguments");

			if (EditorGUI.EndChangeCheck())
			{
				property.serializedObject.ApplyModifiedProperties();

				// Changed module or function so lets refresh arguments

				if (methodInfo != null)
				{
					List<ArgumentBase> arguments = new List<ArgumentBase>();

					var parameters = methodInfo.GetParameters();
					for (int i = 1; i < parameters.Length; i++) // skip first param because we know this will be effectinstance
					{
						var argument = ArgumentHelper.GetArgumentOfType(parameters[i].ParameterType);
						argument.name = parameters[i].Name;
						arguments.Add(argument);
					}

					EditorUtils.SetTargetObjectOfProperty(argsProp, arguments);
				}
			}

			argsProp = property.FindPropertyRelative("arguments");
			if (argsProp.arraySize > 0)
			{
				position.y += EditorGUIUtility.singleLineHeight;

				EditorGUI.PropertyField(position, argsProp, false);

				if (argsProp.isExpanded)
				{
					EditorGUI.BeginChangeCheck();

					position.y += EditorGUIUtility.singleLineHeight;
					EditorGUI.indentLevel++;

					for (int i = 0; i < argsProp.arraySize; i++)
					{
						var elementProp = argsProp.GetArrayElementAtIndex(i);

						EditorGUI.PropertyField(position, elementProp, true);

						position.y += EditorGUI.GetPropertyHeight(elementProp, true);
					}

					EditorGUI.indentLevel--;

					if (EditorGUI.EndChangeCheck())
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
		if (property.isExpanded)
		{
			float height = EditorGUIUtility.singleLineHeight * 4f;

			var argsProp = property.FindPropertyRelative("arguments");
			if (argsProp.arraySize > 0f)
			{
				height += EditorGUI.GetPropertyHeight(argsProp, argsProp.isExpanded);

				if (argsProp.isExpanded)
					height -= EditorGUIUtility.singleLineHeight; // cancel array size line since we're skipping that
			}

			return height;
		}
		else
			return EditorGUIUtility.singleLineHeight;
	}
}
