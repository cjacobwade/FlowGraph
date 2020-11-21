using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(ModuleFunction))]
public class ModuleFunctionDrawer : PropertyDrawer
{
	private SerializedProperty moduleProp = null;
	private SerializedProperty functionProp = null;
	private SerializedProperty argsProp = null;

	private VisualElement container = null;
	private List<FlowTypeCache.ModuleInfo> moduleInfos = new List<FlowTypeCache.ModuleInfo>();

	private PopupField<string> moduleField = null;
	private PopupField<string> functionField = null;

	private PropertyField argsField = null;

	public override VisualElement CreatePropertyGUI(SerializedProperty property)
	{
		moduleProp = property.FindPropertyRelative("module");
		functionProp = property.FindPropertyRelative("function");
		argsProp = property.FindPropertyRelative("arguments");

		container = new VisualElement();

		ObjectField contextField = new ObjectField("Context");
		contextField.objectType = typeof(UniqueObjectData);
		container.Add(contextField);

		moduleInfos = FlowTypeCache.GetModuleInfos();

		RebuildModuleFunction();

		return container;
	}

	private void OnModuleNameChanged(ChangeEvent<string> change)
	{
		moduleProp.stringValue = change.newValue;
		RebuildModuleFunction();
	}

	private void OnFunctionNameChanged(ChangeEvent<string> change)
	{
		functionProp.stringValue = change.newValue;
		RebuildModuleFunction();
	}

	private void RebuildModuleFunction()
	{
		if (moduleField != null)
			moduleField.RemoveFromHierarchy();

		if (functionField != null)
			functionField.RemoveFromHierarchy();

		List<string> modules = moduleInfos.Select(m => m.typeInfo.Name).ToList();
		int moduleIndex = modules.IndexOf(moduleProp.stringValue);

		moduleField = new PopupField<string>(
			moduleProp.displayName, modules, moduleIndex,
			(s) => s.Replace("FlowModule_", ""),
			(s) => s.Replace("FlowModule_", ""));

		moduleField.Bind(moduleProp.serializedObject);
		moduleField.RegisterCallback<ChangeEvent<string>>(OnModuleNameChanged);
		container.Add(moduleField);

		List<string> functions = new List<string>();

		int functionIndex = -1;
		if (moduleIndex >= 0)
		{
			functions = moduleInfos[moduleIndex].methodInfos.Select(m => m.Name).ToList();
			functionIndex = Mathf.Max(0, functions.IndexOf(functionProp.stringValue));
		}

		functionField = new PopupField<string>(functionProp.displayName, functions, functionIndex);
		functionField.Bind(functionProp.serializedObject);
		functionField.RegisterCallback<ChangeEvent<string>>(OnFunctionNameChanged);
		container.Add(functionField);

		RebuildArguments();
	}

	private void RebuildArguments()
	{
		MethodInfo methodInfo = FlowTypeCache.GetModuleFunction(
			moduleProp.stringValue, functionProp.stringValue);

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

			argsProp.arraySize = parameters.Length - 1;
			var obj = EditorUtils.SetTargetObjectOfProperty(argsProp, arguments);
			argsProp.serializedObject.Update();	
		}

		if (argsField != null)
			argsField.RemoveFromHierarchy();

		if (argsProp.arraySize > 0)
		{
			argsField = new PropertyField(argsProp);
			argsField.Bind(argsProp.serializedObject);
			container.Add(argsField);
		}

		container.MarkDirtyRepaint();
	}
}
