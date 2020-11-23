using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEngine.Profiling;

[CustomPropertyDrawer(typeof(ModuleFunction))]
public class ModuleFunctionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Profiler.BeginSample("Module Function Drawer");

		EditorGUI.BeginProperty(position, label, property);

		position.height = EditorGUIUtility.singleLineHeight;

		using (var check = new EditorGUI.ChangeCheckScope())
		{
			EditorGUI.PropertyField(position, property, label, false);

			if (GUI.changed)
			{
				property.serializedObject.ApplyModifiedProperties();
				EditorGUI.EndProperty();

				Profiler.EndSample(); // Module Function Drawer
				return;
			}
		}

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			position.y += EditorGUIUtility.singleLineHeight;

			var moduleInfos = FlowTypeCache.GetModuleInfos();

			List<string> modules = moduleInfos.Select(m => m.typeInfo.Name).ToList();
			List<string> moduleDisplayNames = moduleInfos.Select(m => m.typeInfo.Name.Replace("FlowModule_", "")).ToList();

			var moduleProp = property.FindPropertyRelative("module");

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var contextProp = property.FindPropertyRelative("context");

				var moduleUOD = FlowModuleSettings.Instance.GetDefaultUOD(moduleProp.stringValue);
				if (moduleUOD != null)
				{
					if (contextProp.objectReferenceValue != moduleUOD)
					{
						contextProp.objectReferenceValue = moduleUOD;
						GUI.changed = true;
					}

					GUI.enabled = false;
				}

				EditorGUI.ObjectField(position, contextProp, typeof(UniqueObjectData));
				position.y += EditorGUIUtility.singleLineHeight;

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();

				GUI.enabled = true;
			}

			int moduleIndex = Mathf.Max(0, modules.IndexOf(moduleProp.stringValue));

			var argsProp = property.FindPropertyRelative("arguments"); ;

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				moduleIndex = EditorGUI.Popup(position, moduleProp.displayName, moduleIndex, moduleDisplayNames.ToArray());

				var functionProp = property.FindPropertyRelative("function");

				List<string> functions = moduleInfos[moduleIndex].methodInfos.Select(m => m.Name).ToList();
				int functionIndex = Mathf.Max(0, functions.IndexOf(functionProp.stringValue));

				position.y += EditorGUIUtility.singleLineHeight;

				functionIndex = EditorGUI.Popup(position, functionProp.displayName, functionIndex, functions.ToArray());

				MethodInfo methodInfo = moduleInfos[moduleIndex].methodInfos[functionIndex];
				ParameterInfo[] parameters = methodInfo.GetParameters();

				if (GUI.changed || parameters.Length != argsProp.arraySize + 1)
				{
					if (GUI.changed)
					{
						moduleProp.stringValue = modules[moduleIndex];
						functionProp.stringValue = functions[functionIndex];

						property.serializedObject.ApplyModifiedProperties();
					}

					// Changed module or function so lets refresh arguments
					if (methodInfo != null)
					{
						List<ArgumentBase> arguments = new List<ArgumentBase>();

						for (int i = 1; i < parameters.Length; i++) // skip first param because we know this will be effectinstance
						{
							var argument = ArgumentHelper.GetArgumentOfType(parameters[i].ParameterType);
							argument.name = parameters[i].Name;
							arguments.Add(argument);
						}

						EditorUtils.SetTargetObjectOfProperty(argsProp, arguments);
					}
				}
			}

			argsProp = property.FindPropertyRelative("arguments");
			if (argsProp.arraySize > 0)
			{
				position.y += EditorGUIUtility.singleLineHeight;

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUI.PropertyField(position, argsProp, false);

					if (argsProp.isExpanded)
					{
						position.y += EditorGUIUtility.singleLineHeight;
						EditorGUI.indentLevel++;

						for (int i = 0; i < argsProp.arraySize; i++)
						{
							var elementProp = argsProp.GetArrayElementAtIndex(i);

							EditorGUI.PropertyField(position, elementProp, true);

							position.y += EditorGUI.GetPropertyHeight(elementProp, true);
						}

						EditorGUI.indentLevel--;
					}

					if(GUI.changed)
						property.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.indentLevel--;

			EditorGUI.EndProperty();

			Profiler.EndSample(); // Module Function Drawer
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
