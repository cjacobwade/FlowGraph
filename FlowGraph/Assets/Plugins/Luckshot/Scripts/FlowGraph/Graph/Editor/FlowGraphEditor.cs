using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(FlowGraph))]
public class FlowGraphEditor : Editor
{
	private SerializedObject so = null;

	private string newArgumentName = "Argument0";

	private Type[] argumentTypes = null;
	private Type[] argumentSourceTypes = null;

	public override void OnInspectorGUI()
	{
		if (GUILayout.Button(new GUIContent("Open Flow Graph")))
		{
			FlowGraphWindow.OpenWindow();
		}

		EditorGUILayout.Space(15f);

		EditorGUILayout.LabelField("Template Arguments", EditorStyles.boldLabel);

		if (so == null)
			so = new SerializedObject(target);
		else
			so.Update();

		var typeNamesProp = so.FindProperty("argumentTypeNames");
		var argumentsProp = so.FindProperty("templateArguments");

		if (argumentsProp != null)
		{
			EditorGUI.indentLevel++;

			bool removed = false;
			List<ArgumentBase> args = (List<ArgumentBase>)EditorUtils.GetTargetObjectOfProperty(argumentsProp);
			for (int i = 0; i < args.Count; i++)
			{
				if (args[i] == null)
				{
					args.RemoveAt(i--);
					removed = true;
				}
			}

			if (removed)
				so.Update();

			if (argumentSourceTypes == null)
			{
				argumentTypes = ArgumentHelper.GetArgumentTypes();
				argumentSourceTypes = ArgumentHelper.GetSourceTypes();
			}

			for (int i = 0; i < argumentsProp.arraySize; i++)
			{
				EditorGUILayout.BeginHorizontal();

				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var argProp = argumentsProp.GetArrayElementAtIndex(i);
					var typeNameProp = typeNamesProp.GetArrayElementAtIndex(i);

					EditorGUILayout.PropertyField(argProp, new GUIContent(argProp.displayName), true, GUILayout.ExpandWidth(true));

					string[] typeNames = new string[argumentTypes.Length];
					string[] displayNames = new string[argumentTypes.Length];
					for (int j = 0; j < typeNames.Length; j++)
					{
						typeNames[j] = argumentTypes[j].Name;
						displayNames[j] = typeNames[j].Replace("Argument_", "");
					}

					int selectedIndex = Mathf.Max(0, Array.IndexOf(typeNames, typeNameProp.stringValue));
					selectedIndex = EditorGUILayout.Popup(selectedIndex, displayNames, GUILayout.Width(90f));

					string typeName = typeNames[selectedIndex];
					if (typeNameProp.stringValue != typeName)
					{
						Undo.RegisterCompleteObjectUndo(so.targetObject, "TypeChange");
						typeNameProp.stringValue = typeName;

						var argument = ArgumentHelper.GetArgumentOfType(argumentSourceTypes[selectedIndex]);
						argument.name = args[i].name;
						argProp.managedReferenceValue = argument;
						Debug.Log("Assign Managed");
						so.ApplyModifiedPropertiesWithoutUndo();
					}

					if (GUILayout.Button("X", GUILayout.Width(15f)))
					{
						argumentsProp.DeleteArrayElementAtIndex(i);
						typeNamesProp.DeleteArrayElementAtIndex(i);
						so.ApplyModifiedProperties();
					}
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.Space(15);

			EditorGUILayout.BeginHorizontal();

			EnsureNewArgumentNameUnique();

			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 65f;

			newArgumentName = EditorGUILayout.TextField("Add Name", newArgumentName, GUILayout.Width(150f));

			EditorGUIUtility.labelWidth = prevLabelWidth;

			if (GUILayout.Button("Add Template Argument"))
			{
				argumentsProp.InsertArrayElementAtIndex(argumentsProp.arraySize);
				typeNamesProp.InsertArrayElementAtIndex(typeNamesProp.arraySize);
				int index = argumentsProp.arraySize - 1;

				var elementProp = argumentsProp.GetArrayElementAtIndex(index);
				var argument = ArgumentHelper.GetArgumentOfType(typeof(UnityEngine.Object));
				argument.name = newArgumentName;
				elementProp.managedReferenceValue = argument;

				Type[] argumentTypes = ArgumentHelper.GetArgumentTypes();
				int typeIndex = Mathf.Max(0, Array.IndexOf(argumentTypes, typeof(Argument_Object)));

				var typeNameProp = typeNamesProp.GetArrayElementAtIndex(index);
				typeNameProp.stringValue = argumentTypes[typeIndex].Name;

				so.ApplyModifiedProperties();
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	private void EnsureNewArgumentNameUnique()
	{
		List<string> names = new List<string>();

		var argumentsProp = so.FindProperty("templateArguments");
		if (argumentsProp != null)
		{
			for (int i = 0; i < argumentsProp.arraySize; i++)
			{
				var argProp = argumentsProp.GetArrayElementAtIndex(i);
				var nameProp = argProp.FindPropertyRelative("name");
				names.Add(nameProp.stringValue);
			}
		}

		if (names.Contains(newArgumentName))
		{
			int index = 1;
			newArgumentName = "Argument0";
			while (names.Contains(newArgumentName))
				newArgumentName = string.Format("Argument{0}", index++);
		}
	}
}
