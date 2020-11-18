using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public static class UIElementUtils
{
	public static void AddStyleSheet(this VisualElement element, string name)
	{
		string[] guids = AssetDatabase.FindAssets(string.Format("t:StyleSheet {0}", name));
		for (int i = 0; i < guids.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
			if (styleSheet != null && styleSheet.name == name)
			{
				element.styleSheets.Add(styleSheet);
				return;
			}
		}
	}

	public static T CreateElementByName<T>(string name) where T : VisualElement
	{
		string[] guids = AssetDatabase.FindAssets(string.Format("t:VisualTreeAsset {0}", name));
		for (int i = 0; i < guids.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
			if (treeAsset != null && treeAsset.name == name)
			{
				TemplateContainer container = treeAsset.CloneTree();
				if (container.GetType() != typeof(T) &&
					!container.GetType().IsSubclassOf(typeof(T)))
				{
					return container.Query<T>();
				}

				return container as T;
			}
		}

		return null;
	}
}
