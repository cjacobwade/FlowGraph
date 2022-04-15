using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemData", menuName = "Luckshot/Item Data")]
[InlineScriptableObject(path = "Assets/Resources/ItemDatas", inline = false)]
[SaveLoadAsset(resourcePath = "ItemDatas/")]
public class ItemData : ScriptableObject
{
	public static bool HaveSameRoot(ItemData a, ItemData b)
	{
		while (a.parentItemData != null)
			a = a.parentItemData;

		while (b.parentItemData != null)
			b = b.parentItemData;

		return a.Equals(b);
	}

	public string nameOverride = string.Empty;
	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(nameOverride))
				return name.Replace("_", "").SplitIntoWordsByCase();

			return nameOverride;
		}
	}

	[TextArea(3, 5)]
	public string description = string.Empty;

	public Color color = Color.white;
	public bool interesting = true;
	public ItemData parentItemData = null;

	public ItemData GetRootItemData()
	{
		ItemData root = this;
		while (root.parentItemData != null)
			root = root.parentItemData;

		return root;
	}

#if UNITY_EDITOR
	public static ItemData CreateItemData(string path, string name)
	{
		Object prevSelection = Selection.activeObject;

		ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path + "/" + name + ".asset");
		if(itemData == null)
		{
			itemData = ScriptableObjectUtils.CreateAsset<ItemData>(path, name);
			itemData.name = name;
		}

		Selection.activeObject = prevSelection;
		return itemData;
	}
#endif
}
