using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using UnityEngine.Profiling;
using System;

[CustomPropertyDrawer(typeof(FlowModuleFunction))]
public class FlowModuleFunctionDrawer : PropertyDrawer
{
	private bool setValuesOnce = false;

	private string prevPropertyPath = null;

	private void OnEnable()
    {
		setValuesOnce = false;
    }

	private void OnDisable()
    {
		prevPropertyPath = null;
    }

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if(property.propertyPath != prevPropertyPath)
        {
			setValuesOnce = false;
			prevPropertyPath = property.propertyPath;
        }

		EditorGUI.BeginProperty(position, label, property);
		position.height = EditorGUIUtility.singleLineHeight;

		using (var check = new EditorGUI.ChangeCheckScope())
		{
			EditorGUI.PropertyField(position, property, label, false);

			if (GUI.changed)
			{
				property.serializedObject.ApplyModifiedProperties();
				EditorGUI.EndProperty();
				EditorGUI.indentLevel--;
				return;
			}
		}

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			position.y += EditorGUIUtility.singleLineHeight;
			var moduleInfos = FlowTypeCache.GetModuleInfos();

			List<string> modules = moduleInfos.Select(m => m.TypeInfo.Name).ToList();
			List<string> moduleDisplayNames = moduleInfos.Select(m => m.TypeInfo.Name.Replace("FlowModule_", "")).ToList();

			var moduleProp = property.FindPropertyRelative("module");

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var contextProp = property.FindPropertyRelative("context");

				Argument_Object contextArg = (Argument_Object)EditorUtils.GetTargetObjectOfProperty(contextProp);

				if (FlowTypeCache.FlowGraphSettings != null)
				{
					var moduleUOD = FlowTypeCache.FlowGraphSettings.GetDefaultUOD(moduleProp.stringValue);
					if (moduleUOD != null)
					{
						bool anyChanged = false;

						var contextSourceProp = contextProp.FindPropertyRelative("source");
						if(contextSourceProp.enumValueIndex != (int)ArgumentSource.Value)
                        {
							contextSourceProp.enumValueIndex = (int)ArgumentSource.Value;
							anyChanged = true;
                        }							

						var contextValueProp = contextProp.FindPropertyRelative("value");
						if (contextValueProp.objectReferenceValue != moduleUOD)
						{
							contextValueProp.objectReferenceValue = moduleUOD;
							anyChanged = true;
							
						}

						if(anyChanged)
							contextProp.serializedObject.ApplyModifiedProperties();

						GUI.changed = true;
						GUI.enabled = false;
					}
				}

				EditorGUI.PropertyField(position, contextProp, true);
				position.y += EditorGUIUtility.singleLineHeight;

				if (GUI.changed)
					property.serializedObject.ApplyModifiedProperties();

				GUI.enabled = true;
			}

			int moduleIndex = Mathf.Max(0, modules.IndexOf(moduleProp.stringValue));

			var argsProp = property.FindPropertyRelative("arguments");

			// Change Function

			bool changedFunction = false;

			EditorGUI.BeginChangeCheck();

			moduleIndex = EditorGUI.Popup(position, moduleProp.displayName, moduleIndex, moduleDisplayNames.ToArray());
			string moduleName = modules[moduleIndex];

			if (EditorGUI.EndChangeCheck() || !setValuesOnce)
            {
				if (moduleProp.stringValue != moduleName)
				{
					moduleProp.stringValue = moduleName;
					moduleProp.serializedObject.ApplyModifiedProperties();

					changedFunction = true;
				}
			}

			var functionProp = property.FindPropertyRelative("function");

			List<string> functions = moduleInfos[moduleIndex].MethodInfos.Select(m => m.Name).ToList();				
			List<string> functionsDisplay = functions.Select(m => m.Replace("_", "/")).ToList();

			int functionIndex = Mathf.Max(0, functions.IndexOf(functionProp.stringValue));

			position.y += EditorGUIUtility.singleLineHeight;

			EditorGUI.BeginChangeCheck();

			functionIndex = EditorGUI.Popup(position, functionProp.displayName, functionIndex, functionsDisplay.ToArray());
			string functionName = functions[functionIndex];

			if(EditorGUI.EndChangeCheck() || !setValuesOnce)
            {
				if(functionProp.stringValue != functionName)
                {
					functionProp.stringValue = functionName;
					functionProp.serializedObject.ApplyModifiedProperties();

					changedFunction = true;
                }
            }

			MethodInfo methodInfo = moduleInfos[moduleIndex].MethodInfos[functionIndex];
			ParameterInfo[] parameters = methodInfo.GetParameters();

			if (changedFunction || parameters.Length != argsProp.arraySize + 1 || !setValuesOnce)
			{
				var existingArgs = (List<ArgumentBase>)EditorUtils.GetTargetObjectOfProperty(argsProp);

				List<Type> types = new List<Type>();
				List<ArgumentBase> arguments = new List<ArgumentBase>();

				var objectType = typeof(UnityEngine.Object);

				for (int i = 1; i < parameters.Length; i++) // skip first param because we know this will be effectinstance
				{
					var parameter = parameters[i];

					var argument = ArgumentHelper.GetArgumentOfType(parameter.ParameterType);
					argument.name = parameters[i].Name;

					if (parameter.HasDefaultValue && !objectType.IsAssignableFrom(parameter.ParameterType) &&
						argument.Value == Activator.CreateInstance(parameter.ParameterType))
					{
						// if argument value is the type's default, use the parameter's set default value
						argument.Value = parameter.DefaultValue;
					}

					types.Add(parameter.ParameterType);
					arguments.Add(argument);
				}

				// Retain existing arguments if new arguments are same type
				for (int i = 0; i < arguments.Count && i < existingArgs.Count; i++)
				{
					if (existingArgs[i] != null && arguments[i] != null &&
						existingArgs[i].type == arguments[i].type)
					{
						arguments[i].Value = existingArgs[i].Value;
						arguments[i].enumValue = existingArgs[i].enumValue;
						arguments[i].source = existingArgs[i].source;
						arguments[i].templateIndex = existingArgs[i].templateIndex;
					}
				}

				argsProp.ClearArray();
				while (argsProp.arraySize < arguments.Count)
					argsProp.InsertArrayElementAtIndex(argsProp.arraySize);

				for (int i = 0; i < argsProp.arraySize; i++)
				{
					var elementProp = argsProp.GetArrayElementAtIndex(i);
					elementProp.managedReferenceValue = arguments[i];
				}

				argsProp.serializedObject.ApplyModifiedProperties();
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
							position.height = EditorGUI.GetPropertyHeight(elementProp, true);
							EditorGUI.PropertyField(position, elementProp, true);
							position.y += position.height;
						}

						EditorGUI.indentLevel--;
					}

					if (GUI.changed)
						property.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.indentLevel--;
		}

		if(GUI.changed)
			EditorUtility.SetDirty(property.serializedObject.targetObject); // usually graph

		setValuesOnce = true;

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (property.isExpanded)
		{
			float height = EditorGUIUtility.singleLineHeight * 4f;

			var argsProp = property.FindPropertyRelative("arguments");
			if (argsProp.arraySize > 0)
			{
				height += EditorGUIUtility.singleLineHeight;

				if (argsProp.isExpanded)
				{
					for (int i = 0; i < argsProp.arraySize; i++)
					{
						var argProp = argsProp.GetArrayElementAtIndex(i);
						height += EditorGUI.GetPropertyHeight(argProp, argProp.isExpanded);
					}
				}
			}

			return height;
		}
		else
			return EditorGUIUtility.singleLineHeight;
	}
}
