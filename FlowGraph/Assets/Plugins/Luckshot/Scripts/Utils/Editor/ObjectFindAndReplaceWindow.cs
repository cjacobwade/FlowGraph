﻿using UnityEngine;
using System.Collections;
using UnityEditor;

public class ObjectFindAndReplaceWindow : EditorWindow
{
	public Object[] _objectArr = new Object[0];
	public string _findText = "";
	public string _replaceText = "";

	[MenuItem("Utilities/Object Name Find And Replace")]
	static void Init()
	{
		ObjectFindAndReplaceWindow window = (ObjectFindAndReplaceWindow)EditorWindow.GetWindow(typeof(ObjectFindAndReplaceWindow));
		window.autoRepaintOnSceneChange = true;
		window.Show();
	}

	void OnGUI()
	{
		EditorGUI.BeginChangeCheck();

		ScriptableObject target = this;
		SerializedObject so = new SerializedObject(target);

		// Object array
		SerializedProperty objectArrProp = so.FindProperty("_objectArr");
		EditorGUILayout.PropertyField(objectArrProp, true);

		_objectArr = new Object[objectArrProp.arraySize];
		for (int i = 0; i < objectArrProp.arraySize; i++)
			_objectArr[i] = objectArrProp.GetArrayElementAtIndex(i).objectReferenceValue as Object;

		// Find string field
		_findText = EditorGUILayout.TextField("Find Text", _findText);

		// Replace string field
		_replaceText = EditorGUILayout.TextField("Replace Text", _replaceText);

		// Apply button if fields are filled
		if (!string.IsNullOrEmpty(_findText))
		{
			if (GUILayout.Button("Replace"))
			{
				Undo.RecordObjects(_objectArr, "Find and Replace");

				for (int i = 0; i < _objectArr.Length; i++)
				{
					string assetPath = AssetDatabase.GetAssetPath(_objectArr[i]);
					_objectArr[i].name = _objectArr[i].name.Replace(_findText, _replaceText);
					AssetDatabase.RenameAsset(assetPath, _objectArr[i].name);
				}

				AssetDatabase.Refresh();
			}
		}

		EditorGUI.EndChangeCheck();

		if (GUI.changed)
			so.ApplyModifiedProperties();
	}
}
