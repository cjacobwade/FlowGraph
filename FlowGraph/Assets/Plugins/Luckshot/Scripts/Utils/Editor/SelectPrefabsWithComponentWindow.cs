using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SelectPrefabsWithComponentWindow : EditorWindow
{
	public TextAsset componentAsset = null;

	[MenuItem("Utilities/Select All GameObjects With Component")]
	static void Init()
	{
		SelectPrefabsWithComponentWindow window = (SelectPrefabsWithComponentWindow)EditorWindow.GetWindow(typeof(SelectPrefabsWithComponentWindow));
		window.autoRepaintOnSceneChange = true;
		window.Show();
	}

	void OnGUI()
	{
		EditorGUI.BeginChangeCheck();

		ScriptableObject target = this;
		SerializedObject so = new SerializedObject(target);

		// Object array
		SerializedProperty componentAssetProp = so.FindProperty("componentAsset");
		EditorGUILayout.PropertyField(componentAssetProp, true);

		if(componentAssetProp.objectReferenceValue != null)
		{
			string componentType = componentAssetProp.objectReferenceValue.name;

			if (GUILayout.Button("Select All Prefabs With Component"))
			{
				string[] allPrefabGUIDs = AssetDatabase.FindAssets("t:prefab");
				List<GameObject> prefabsWithComponent = new List<GameObject>();

				for (int i = 0; i < allPrefabGUIDs.Length; i++)
				{
					string path = AssetDatabase.GUIDToAssetPath(allPrefabGUIDs[i]);
					GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
					if (go.GetComponent(componentType))
						prefabsWithComponent.Add(go);
				}

				Selection.objects = prefabsWithComponent.ToArray();
			}
		}

		EditorGUI.EndChangeCheck();

		if (GUI.changed)
			so.ApplyModifiedProperties();
	}
}
