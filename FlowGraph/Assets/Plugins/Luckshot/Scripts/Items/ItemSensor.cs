using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ItemSensor : MonoBehaviour
{
	[System.Serializable]
	public class ItemColliderCollection
	{
		public Item propertyItem;
		public List<Collider> colliders = new List<Collider>();
	}

	[SerializeField, ReadOnly]
	private List<Item> collection = new List<Item>();
	public List<Item> Collection
	{
		get
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i] == null)
				{
					Item unsensed = collection[i];

					collection.RemoveAt(i--);
					OnItemExited(unsensed);
				}
			}

			return collection;
		}
	}

#if UNITY_EDITOR
	[SerializeField, ReadOnly]
	private List<ItemColliderCollection> colliderCollections = new List<ItemColliderCollection>();
#endif

	private Dictionary<Item, ItemColliderCollection> itemToColliderCollection = new Dictionary<Item, ItemColliderCollection>();

	public ItemColliderCollection GetItemColliderCollection(Item item)
	{
		if (itemToColliderCollection.TryGetValue(item, out ItemColliderCollection itemColliderCollection))
			return itemColliderCollection;

		return null;
	}

	private List<Collider> collidersToRemove = new List<Collider>();

	public event System.Action<Item> OnItemEntered = delegate {};
	public event System.Action<Item> OnItemExited = delegate {};

	[SerializeField]
	private bool detectTriggers = true;

	private List<Collider> triggerColliders = new List<Collider>();
	public List<Collider> TriggerColliders => triggerColliders;

	private void Awake()
	{
		CacheTriggers();
	}

	public void CacheTriggers()
	{
		triggerColliders.Clear();

		Collider[] colliders = transform.GetComponentsInChildren<Collider>(true);
		for (int i = 0; i < colliders.Length; i++)
		{
			Collider collider = colliders[i];

			if (!collider.isTrigger)
				continue;

			if (collider.attachedRigidbody != null &&
				collider.attachedRigidbody.transform != transform)
				continue;

			triggerColliders.Add(colliders[i]);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < collection.Count; i++)
			OnItemExited(collection[i]);

		collection.Clear();
		itemToColliderCollection.Clear();

#if UNITY_EDITOR
		colliderCollections.Clear();
#endif
	}

	private void SensedItem_OnCollidersChanged(Item item)
	{
		CheckCollidersValid(item);
	}

	private void SensedItem_OnRigidbodyRemoved(Item item)
	{
		CheckCollidersValid(item);
	}

	private void CheckCollidersValid(Item item)
	{
		if(itemToColliderCollection.TryGetValue(item, out ItemColliderCollection colliderCollection))
		{
			collidersToRemove.Clear();

			foreach(var collider in colliderCollection.colliders)
			{
				if (collider == null || !collider.enabled ||
					!collider.gameObject.activeInHierarchy)
				{
					collidersToRemove.Add(collider);
					continue;
				}

				if(!detectTriggers && collider.isTrigger)
				{
					collidersToRemove.Add(collider);
					continue;
				}

				if(!item.AllColliders.Contains(collider) ||
					!Item.FindNearestParentItem(collider, out Item parentItem) || 
					parentItem != item)
				{
					collidersToRemove.Add(collider);
					continue;
				}
			}

			for (int i = 0; i < collidersToRemove.Count; i++)
				colliderCollection.colliders.Remove(collidersToRemove[i]);

			if(colliderCollection.colliders.Count == 0)
			{
				// Manually check if there's any other overlaps since this check might be happening as colliders are swapped
				// as is a very common case on the player
				for(int i = 0; i < item.AllColliders.Count; i++)
				{
					Collider itemCol = item.AllColliders[i];
					if (!colliderCollection.colliders.Contains(itemCol) &&
						IsColliderValidAndOverlappingSensor(itemCol))
					{
						colliderCollection.colliders.Add(itemCol);
					}
				}
			}

			if (colliderCollection.colliders.Count > 0)
				return;
		}

		RemoveItem(item);
	}

	private bool IsColliderValidAndOverlappingSensor(Collider itemCol)
	{
		if (itemCol.enabled && itemCol.gameObject.activeInHierarchy &&
			(detectTriggers || !itemCol.isTrigger))
		{
			for (int j = 0; j < triggerColliders.Count; j++)
			{
				Collider triggerCol = triggerColliders[j];
				if (PhysicsUtils.CheckColliderOverlap(itemCol, triggerCol))
					return true;
			}
		}

		return false;
	}

	private void SensedItem_OnItemDisabled(Item item)
	{
		CheckCollidersValid(item);
	}

	private void SensedItem_OnItemDestroyed(Item item)
	{
		if(itemToColliderCollection.TryGetValue(item, out ItemColliderCollection collection))
		{
			collection.colliders.Clear();
			RemoveItem(item);
		}
	}

	private void AddItem(Item item)
	{
		// TODO: Subscribing/unsubscribing like this is going to generate lots of garbage
		// should make a wrapper for delegates so we can avoid the list rebuild C#
		// uses in the delegate += operator

		item.OnItemDisabled += SensedItem_OnItemDisabled; 
		item.OnItemDestroyed += SensedItem_OnItemDestroyed;

		item.OnCollidersChanged += SensedItem_OnCollidersChanged;
		item.OnRigidbodyRemoved += SensedItem_OnRigidbodyRemoved;

		collection.Add(item);
		OnItemEntered(item);
	}

	private void RemoveItem(Item item)
	{
		item.OnItemDisabled -= SensedItem_OnItemDisabled;
		item.OnItemDestroyed -= SensedItem_OnItemDestroyed;

		item.OnCollidersChanged -= SensedItem_OnCollidersChanged;
		item.OnRigidbodyRemoved -= SensedItem_OnRigidbodyRemoved;

		collection.Remove(item);
		OnItemExited(item);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!detectTriggers && other.isTrigger)
			return;

		if (Item.FindNearestParentItem(other, out Item item))
		{
			if (itemToColliderCollection.TryGetValue(item, out ItemColliderCollection colliderCollection))
			{
				if(!colliderCollection.colliders.Contains(other))
					colliderCollection.colliders.Add(other);
			}
			else
			{
				colliderCollection = new ItemColliderCollection();
				colliderCollection.propertyItem = item;
				colliderCollection.colliders.Add(other);

				itemToColliderCollection.Add(item, colliderCollection);

#if UNITY_EDITOR
				colliderCollections.Add(colliderCollection);
#endif
			}

			if (!collection.Contains(item))
			{
				AddItem(item);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (Item.FindNearestParentItem(other, out Item item))
		{
			if (itemToColliderCollection.TryGetValue(item, out ItemColliderCollection itemInfo))
			{
				if(!IsColliderValidAndOverlappingSensor(other) &&
					itemInfo.colliders.Remove(other) &&
					itemInfo.colliders.Count == 0)
				{
					RemoveItem(item);
				}
			}
		}
	}
}
