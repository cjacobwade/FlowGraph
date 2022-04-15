using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FlowTemplate))]
public class FlowTemplateDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		var graphProp = property.FindPropertyRelative("flowGraph");
		if (graphProp == null)
			return;

		position.height = EditorGUIUtility.singleLineHeight;
		EditorGUI.PropertyField(position, property, label, false);

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();

			position.y += EditorGUIUtility.singleLineHeight;

			EditorGUI.PropertyField(position, graphProp, new GUIContent(graphProp.displayName), true);

			if (EditorGUI.EndChangeCheck())
				property.serializedObject.ApplyModifiedProperties();

			FlowGraph flowGraph = graphProp.objectReferenceValue as FlowGraph;			
			if (flowGraph != null)
			{
				var graphArgs = flowGraph.templateArguments;
				bool changed = false;

				var argsProp = property.FindPropertyRelative("arguments");
				if (graphArgs.Count != argsProp.arraySize)
				{
					while (argsProp.arraySize > graphArgs.Count)
					{
						argsProp.DeleteArrayElementAtIndex(argsProp.arraySize - 1);
						changed = true;
					}

					while (argsProp.arraySize < graphArgs.Count)
					{
						argsProp.InsertArrayElementAtIndex(argsProp.arraySize);

						int index = argsProp.arraySize - 1;
						var argProp = argsProp.GetArrayElementAtIndex(index);

						ArgumentBase arg = ArgumentHelper.GetArgumentOfType(graphArgs[index].Value.GetType());
						arg.name = graphArgs[index].name;
						argProp.managedReferenceValue = arg;

						changed = true;
					}
				}

				if (changed)
					property.serializedObject.ApplyModifiedProperties();

				argsProp = property.FindPropertyRelative("arguments");
				bool anyChanges = false;
				for (int i = 0; i < argsProp.arraySize; i++)
				{
					var elementProp = argsProp.GetArrayElementAtIndex(i);
					var argument = (ArgumentBase)EditorUtils.GetTargetObjectOfProperty(elementProp);
					if (argument.GetType() != flowGraph.templateArguments[i].GetType())
					{
						ArgumentBase existingArg = flowGraph.templateArguments[i];
						ArgumentBase arg = ArgumentHelper.GetArgumentOfType(existingArg.Value.GetType());
						arg.name = existingArg.name;
						elementProp.managedReferenceValue = arg;

						anyChanges = true;
					}
				}

				if (anyChanges)
					argsProp.serializedObject.ApplyModifiedProperties();

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
			height += EditorGUIUtility.singleLineHeight;

			var graphProp = property.FindPropertyRelative("flowGraph");
			if (graphProp.objectReferenceValue != null)
			{
				var argsProp = property.FindPropertyRelative("arguments");
				if (argsProp.arraySize > 0)
				{
					height += EditorGUIUtility.singleLineHeight;

					if (argsProp.isExpanded)
					{
						for (int i = 0; i < argsProp.arraySize; i++)
						{
							var elementProp = argsProp.GetArrayElementAtIndex(i);
							height += EditorGUI.GetPropertyHeight(elementProp, true);
						}
					}
				}
			}
		}

		return height;
	}
}
