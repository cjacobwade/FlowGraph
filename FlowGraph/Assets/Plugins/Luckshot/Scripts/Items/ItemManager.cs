using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemManager : Singleton<ItemManager>
{
	private Dictionary<string, ItemData> nameToDataMap = new Dictionary<string, ItemData>();
	private Dictionary<ItemData, Item> dataToPrefabMap = new Dictionary<ItemData, Item>();

	private List<Item> allItems = new List<Item>();
	public List<Item> AllItems => allItems;

	[SerializeField]
	private float minItemCleanupHeight = -100f;

	[SerializeField]
	private int cleanupChecksPerFrame = 2;
	private int cleanupCheckIter = 0;

	private Collider[] searchColliders = new Collider[100];
	private HashSet<Item> searchHitItems = new HashSet<Item>();

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
				if (item.GetProperty<SafeItem>())
					return;

				Destroy(item.gameObject);
				cleanupCheckIter--;
			}

			cleanupChecks++;
			cleanupCheckIter++;
		}
	}

	public Item TryFindItemMatchingItemStateDefinition(ItemStateDefinition itemStateDefinition, 
		Vector3 startPos, float searchRange = 100f, Func<Item, bool> validateFunc = null)
    { return TryFindItemMatchingStateDefinition(itemStateDefinition, null, startPos, searchRange, validateFunc); }

	public Item TryFindItemMatchingPropertyStateDefinition(PropertyItemStateDefinition propertyStateDefinition,
		Vector3 startPos, float searchRange = 100f, Func<Item, bool> validateFunc = null)
	{ return TryFindItemMatchingStateDefinition(null, propertyStateDefinition, startPos, searchRange, validateFunc); }

	private Item TryFindItemMatchingStateDefinition(ItemStateDefinition itemStateDefinition,
		PropertyItemStateDefinition propertyStateDefinition,
		Vector3 startPos, float searchRange = 100f,
		Func<Item, bool> validateFunc = null)
	{
		int numHits = Physics.OverlapSphereNonAlloc(startPos, searchRange, searchColliders,
			1 << Layers.GRABBABLE_INT | 1 << Layers.PLAYER_INT,
			QueryTriggerInteraction.Collide);

		searchHitItems.Clear();

		for (int i = 0; i < numHits; i++)
		{
			if (Item.FindNearestParentItem(searchColliders[i], out Item item))
				searchHitItems.Add(item);
		}

		Item nearestItem = null;
		float nearestDist = Mathf.Infinity;

		foreach (var hitItem in searchHitItems)
		{
			float dist = Vector3.Distance(hitItem.Center, startPos);
			if (dist > nearestDist)
				continue;

			if (validateFunc != null && !validateFunc(hitItem))
				continue;

			if ((itemStateDefinition == null || itemStateDefinition.CheckState(hitItem)) &&
				(propertyStateDefinition == null || propertyStateDefinition.CheckState(hitItem)))
			{
				nearestItem = hitItem;
				nearestDist = dist;
			}
		}

		return nearestItem;
	}

	public Item InstantiateItemFromItemState(ItemState itemState)
	{
		ItemData itemData = GetItemDataByName(itemState.itemName);
		if (itemData != null)
		{
			Item itemPrefab = GetItemPrefab(itemData);

			Item item = Instantiate(itemPrefab);
			itemState.ApplyStateToItem(item);

			return item;
		}

		return null;
	}
}
