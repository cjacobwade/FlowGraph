using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataCollection", menuName = "Luckshot/Item Data Collection")]
public class ItemDataCollection : ScriptableObject
{
    public List<ItemData> itemDatas = new List<ItemData>();

    public bool Contains(ItemData itemData)
	{ return itemDatas.Contains(itemData); }

    public bool AnyHaveSameRoot(ItemData itemData)
	{
		foreach(var data in itemDatas)
		{
			if (ItemData.HaveSameRoot(data, itemData))
				return true;
		}

		return false;
	}
}
