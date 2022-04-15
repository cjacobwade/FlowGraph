// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

// Developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

// Must be placed within a folder named "Editor"
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Extends how ScriptableObject object references are displayed in the inspector
/// Shows you all values under the object reference
/// Also provides a button to create a new ScriptableObject if property is null.
/// </summary>
[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class InlineScriptableObjectDrawer : PropertyDrawer
{
	bool open = false;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float totalHeight = EditorGUIUtility.singleLineHeight;
		if (open)
		{
			var data = (ScriptableObject)property.objectReferenceValue;
			SerializedObject serializedObject = new SerializedObject(data);
			SerializedProperty prop = serializedObject.GetIterator();
			if (prop.NextVisible(true))
			{
				do
				{
					if (prop.name == "m_Script") continue;
					var subProp = serializedObject.FindProperty(prop.name);
					float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
					totalHeight += height;
				}
				while (prop.NextVisible(false));
			}
			// Add a tiny bit of height if open for the background
			totalHeight += EditorGUIUtility.standardVerticalSpacing;
		}
		return totalHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		InlineScriptableObjectAttribute inlineAttr = GetAttribute(fieldInfo.FieldType);

		if (property.objectReferenceValue != null)
		{
			if (inlineAttr == null || !inlineAttr.inline)
			{
				var rect2 = new Rect(
					position.x,
					position.y,
					position.width,
					EditorGUIUtility.singleLineHeight);

				EditorGUI.PropertyField(rect2, property, label, true);
				EditorGUI.EndProperty();
				return;
			}

			open = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), open, property.displayName, true);

			var rect = new Rect(
					EditorGUIUtility.labelWidth + 14,
					position.y,
					position.width - EditorGUIUtility.labelWidth,
					EditorGUIUtility.singleLineHeight);

			EditorGUI.PropertyField(rect, property, GUIContent.none, true);
				
			if (GUI.changed)
				property.serializedObject.ApplyModifiedProperties();

			if (property.objectReferenceValue == null)
			{
			}
			else
			{
				if (open)
				{
					// Draw a background that shows us clearly which fields are part of the ScriptableObject
					GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, Screen.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

					EditorGUI.indentLevel++;
					var data = (ScriptableObject)property.objectReferenceValue;
					SerializedObject serializedObject = new SerializedObject(data);

					// Iterate over all the values and draw them
					SerializedProperty prop = serializedObject.GetIterator();
					float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					if (prop.NextVisible(true))
					{
						do
						{
							// Don't bother drawing the class file
							if (prop.name == "m_Script") continue;
							float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
							EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
							y += height + EditorGUIUtility.standardVerticalSpacing;
						}
						while (prop.NextVisible(false));
					}
					if (GUI.changed)
						serializedObject.ApplyModifiedProperties();

					EditorGUI.indentLevel--;
				}
			}
		}
		else
		{
			EditorGUI.ObjectField(new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight), property, new GUIContent(property.displayName));
			if (GUI.Button(new Rect(position.x + position.width - 58, position.y, 58, EditorGUIUtility.singleLineHeight), "Create"))
			{
				string selectedAssetPath = "Assets";
				if (inlineAttr != null && !string.IsNullOrEmpty(inlineAttr.path))
					selectedAssetPath = inlineAttr.path;

				property.objectReferenceValue = CreateAssetWithSavePrompt(fieldInfo.FieldType, selectedAssetPath);
			}
		}

		if(GUI.changed)
			property.serializedObject.ApplyModifiedProperties();

		EditorGUI.EndProperty();
	}

	// Creates a new ScriptableObject via the default Save File panel
	ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
	{
		path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "New " + type.Name + ".asset", "asset", "Enter a file name for the ScriptableObject.", path);
		if (path == "")
			return null;

		ScriptableObject asset = ScriptableObject.CreateInstance(type);
		AssetDatabase.CreateAsset(asset, path);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		EditorGUIUtility.PingObject(asset);
		return asset;
	}

	private static InlineScriptableObjectAttribute GetAttribute(Type t)
	{
		Attribute[] attrs = Attribute.GetCustomAttributes(t);
		foreach (Attribute attr in attrs)
		{
			if (attr is InlineScriptableObjectAttribute)
				return attr as InlineScriptableObjectAttribute;
		}

		return null;
	}
}