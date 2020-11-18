using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemManager : Singleton<ItemManager>
{
	private Dictionary<string, ItemData> nameToDataMap = new Dictionary<string, ItemData>();
	private Dictionary<ItemData, Item> dataToPrefabMap = new Dictionary<ItemData, Item>();

	private List<Item> allItems = new List<Item>();
	public List<Item> AllItems
	{ get { return allItems; } }

	[SerializeField]
	private float minItemCleanupHeight = -100f;

	[SerializeField]
	private int cleanupChecksPerFrame = 2;
	private int cleanupCheckIter = 0;

	public void RegisterItem(Item item)
	{
		if (!allItems.Contains(item))
			allItems.Add(item);
	}

	public void DeregisterItem(Item item)
	{ allItems.Remove(item); }

	public ItemData GetItemDataByName(string name)
	{
		nameToDataMap.TryGetValue(name, out ItemData itemData);
		return itemData;
	}

	public Item GetItemPrefab(ItemData itemData)
	{ return dataToPrefabMap[itemData]; }

	private Dictionary<string, Type> propertyNameToType = new Dictionary<string, Type>();

	public Type GetTypeFromPropertyName(string name)
	{
		Type type = null;
		propertyNameToType.TryGetValue(name, out type);
		return type;
	}

	protected override void Awake()
	{
		base.Awake();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (var type in assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(PropertyItem)))
					propertyNameToType.Add(type.Name, type);
			}
		}

		Item[] itemPrefabs = Resources.LoadAll<Item>("Prefabs/Pickupables");

		for(int i = 0; i < itemPrefabs.Length; i++)
		{
			nameToDataMap.Add(itemPrefabs[i].Data.name, itemPrefabs[i].Data);
			dataToPrefabMap.Add(itemPrefabs[i].Data, itemPrefabs[i]);
		}
	}

	private void Update()
	{
		int cleanupChecks = 0;
		while(cleanupChecks < cleanupChecksPerFrame && allItems.Count > 0)
		{
			cleanupCheckIter = (int)Mathf.Repeat(cleanupCheckIter, allItems.Count);

			Item item = allItems[cleanupCheckIter];
			if (item.transform.position.y < minItemCleanupHeight)
			{
				// TODO: Protect player and other important items??

				Destroy(item.gameObject);
				cleanupCheckIter--;
			}

			cleanupChecks++;
			cleanupCheckIter++;
		}
	}

	public Item InstantiateItemFromItemState(ItemState itemState)
	{
		ItemData itemData = GetItemDataByName(itemState.itemName);
		if (itemData != null)
		{
			Item itemPrefab = GetItemPrefab(itemData);

			Item item = Instantiate(itemPrefab);
			item.ApplyItemState(itemState);

			return item;
		}

		return null;
	}
}
