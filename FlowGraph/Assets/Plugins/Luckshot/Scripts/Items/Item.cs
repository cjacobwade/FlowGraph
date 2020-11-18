
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

[System.Serializable]
public struct RigidbodySettings
{
	public float mass;
	public float drag;
	public float angularDrag;
	public bool useGravity;
	public RigidbodyConstraints constraints;
	public CollisionDetectionMode collisionDetectionMode;

	public RigidbodySettings(Rigidbody rigidbody)
	{
		mass = rigidbody.mass;
		drag = rigidbody.drag;
		angularDrag = rigidbody.angularDrag;
		useGravity = rigidbody.useGravity;
		constraints = rigidbody.constraints;
		collisionDetectionMode = rigidbody.collisionDetectionMode;
	}

	public void Apply(Rigidbody rigidbody)
	{
		rigidbody.mass = mass;
		rigidbody.drag = drag;
		rigidbody.angularDrag = angularDrag;
		rigidbody.useGravity = useGravity;
		rigidbody.constraints = constraints;
		rigidbody.collisionDetectionMode = collisionDetectionMode;
	}
}

[DefaultExecutionOrder(-3)]
[SelectionBase]
public class Item : MonoBehaviour
{
	public static bool FindNearestParentItem(Collider collider, out Item item)
	{
		item = null;

		if (collider != null)
			item = collider.GetComponentInParent<Item>();

		return item != null;
	}

	[SerializeField]
	private ItemData itemData = null;
	public ItemData Data
	{ get { return itemData; } }

#if UNITY_EDITOR
	[Button("Find Or Create Item Data")]
	public void FindOrCreateItemData()
	{ itemData = ItemData.CreateItemData("Assets/Resources/ItemDatas", gameObject.name); }
#endif

	#region Properties
	private Dictionary<Type, PropertyItem> typeToPropertyMap = new Dictionary<Type, PropertyItem>();

	public void RegisterProperty(PropertyItem propertyItem)
	{
		typeToPropertyMap.Add(propertyItem.GetType(), propertyItem);

		propertyItem.OnStateChanged += PropertyItem_OnStateChanged;

		OnPropertyItemAdded(propertyItem);
	}

	public void DeregisterProperty(PropertyItem propertyItem)
	{
		typeToPropertyMap.Remove(propertyItem.GetType());

		propertyItem.OnStateChanged -= PropertyItem_OnStateChanged;

		OnPropertyItemRemoved(propertyItem);
	}

	public T GetProperty<T>() where T : PropertyItem
	{
		if(!typeToPropertyMap.TryGetValue(typeof(T), out PropertyItem property))
		{
			foreach (var kvp in typeToPropertyMap)
			{
				if (kvp.Key.IsSubclassOf(typeof(T)))
					return kvp.Value as T;
			}
		}

		return property as T;
	}

	public T GetOrAddProperty<T>() where T : PropertyItem
	{
		T property = GetProperty<T>();
		if (property == null)
			property = gameObject.AddComponent<T>();

		return property;
	}

	public bool HasProperty<T>() where T : PropertyItem
	{ return GetProperty<T>() != null; }

	#endregion // Properties

	#region Physics
	[SerializeField, AutoCache]
	protected new Rigidbody rigidbody = null;
	public Rigidbody Rigidbody
	{ get { return rigidbody; } }

	public LensManagerBool UseGravityLens = null;
	public LensManagerFloat DragLens = null;
	public LensManagerFloat AngularDragLens = null;

	private List<Collider> allColliders = new List<Collider>();
	public List<Collider> AllColliders
	{ get { return allColliders; } }

	[SerializeField, ReadOnly]
	private Bounds localBounds = new Bounds();
	public Bounds LocalBounds
	{ get { return localBounds; } }

	public Vector3 Center
	{ get { return transform.TransformPoint(localBounds.center); } }

	[ShowNativeProperty()]
	public float Height
	{ get { return localBounds.size.y; } }

	[ShowNativeProperty()]
	public float Radius
	{ get { return Mathf.Max(localBounds.extents.x, localBounds.extents.z); } }

	[ShowNativeProperty()]
	public float Volume
	{ get { return localBounds.size.x * localBounds.size.y * localBounds.size.z; } }

	private Vector3[] worldCorners = new Vector3[8];
	public Vector3[] CalculateWorldCorners()
	{
		Vector3 center = transform.TransformPoint(localBounds.center);

		Vector3 right = transform.right * localBounds.size.x;
		Vector3 up = transform.up * localBounds.size.y;
		Vector3 forward = transform.forward * localBounds.size.z;

		worldCorners[0] = center - up - right - forward;
		worldCorners[1] = center - up - right + forward;
		worldCorners[2] = center - up + right - forward;
		worldCorners[3] = center - up + right + forward;

		worldCorners[4] = center + up - right - forward;
		worldCorners[5] = center + up - right + forward;
		worldCorners[6] = center + up + right - forward;
		worldCorners[7] = center + up + right + forward;

		return worldCorners;
	}

	private RigidbodySettings? backupSettings = null;

	public RigidbodySettings RigidbodySettings
	{
		get { return rigidbody != null ? new RigidbodySettings(rigidbody) : backupSettings.Value; }

		set
		{
			if (rigidbody != null)
				value.Apply(rigidbody);
			else
				backupSettings = value;
		}
	}

	public static void SetIgnoreCollisionBetweenItems(Item a, Item b, bool setIgnored)
	{
		if (a == null || b == null)
			return;

		for (int j = 0; j < a.AllColliders.Count; j++)
		{
			Collider aCollider = a.AllColliders[j];
			if (aCollider == null ||
				aCollider.isTrigger ||
				!aCollider.enabled ||
				!aCollider.gameObject.activeInHierarchy)
			{
				continue;
			}

			for (int k = 0; k < b.AllColliders.Count; k++)
			{
				Collider bCollider = b.AllColliders[k];
				if (bCollider == null ||
					bCollider.isTrigger ||
					!bCollider.enabled ||
					!bCollider.gameObject.activeInHierarchy)
				{
					continue;
				}

				Physics.IgnoreCollision(aCollider, bCollider, setIgnored);
			}
		}
	}
	#endregion // Physics

	#region IgnoredColliders
	private HashSet<Collider> ignoredColliders = new HashSet<Collider>();

	public bool IsColliderIgnored(Collider collider)
	{ return ignoredColliders.Contains(collider); }

	public void AddIgnoredCollider(Collider addCollider)
	{ ignoredColliders.Add(addCollider); }

	public bool RemoveIgnoredCollider(Collider removeCollider)
	{ return ignoredColliders.Remove(removeCollider); }

	public void AddIgnoredColliders(IEnumerable<Collider> addColliders)
	{
		foreach (var addCollider in addColliders)
			AddIgnoredCollider(addCollider);
	}

	public bool RemoveIgnoredColliders(IEnumerable<Collider> removeColliders)
	{
		bool removed = false;
		foreach (var removeCollider in removeColliders)
		{
			if (RemoveIgnoredCollider(removeCollider))
				removed = true;
		}
		return removed;
	}
	#endregion

	[SerializeField, AutoCache]
	private VisualsModel visualsModel = null;
	public VisualsModel VisualsModel
	{ get { return visualsModel; } }

	[SerializeField, AutoCache]
	private ColliderModel colliderModel = null;
	public ColliderModel ColliderModel
	{ get { return colliderModel; } }

	public event Action<Item, Bounds> OnBoundsChanged = delegate { };
	public event Action<Item> OnCollidersChanged = delegate { };

	public event Action<Item, Rigidbody> OnRigidbodyAdded = delegate { };
	public event Action<Item, Rigidbody> OnWillRemoveRigidbody = delegate { };
	public event Action<Item> OnRigidbodyRemoved = delegate { };

	public event Action<PropertyItem> OnPropertyItemAdded = delegate { };
	public event Action<PropertyItem> OnPropertyItemRemoved = delegate { };

	public event Action<Item, PropertyItem> OnStateChanged = delegate { };

	public event Action<Item> OnItemEnabled = delegate { };
	public event Action<Item> OnItemDisabled = delegate { };

	public event Action<Item> OnItemDestroyed = delegate { };

	private bool collidersDirty = false;

	public virtual void Awake()
	{
		RefreshColliders();

		if (visualsModel == null)
			visualsModel = gameObject.AddComponent<VisualsModel>();

		if (colliderModel == null)
			colliderModel = gameObject.AddComponent<ColliderModel>();

		bool defaultUseGravity = true;
		float defaultDrag = 0f;
		float defaultAngularDrag = 0.05f;

		if (rigidbody != null)
		{
			defaultUseGravity = rigidbody.useGravity;
			defaultDrag = rigidbody.drag;
			defaultAngularDrag = rigidbody.angularDrag;
		}

		UseGravityLens = new LensManagerBool(requests => LensUtils.AllTrue(requests, defaultUseGravity));
		UseGravityLens.OnValueChanged += UseGravityLens_OnValueChanged;

		DragLens = new LensManagerFloat(requests => LensUtils.Priority(requests, defaultDrag));
		DragLens.OnValueChanged += DragLens_OnValueChanged;

		AngularDragLens = new LensManagerFloat(requests => LensUtils.Priority(requests, defaultAngularDrag));
		AngularDragLens.OnValueChanged += AngularDragLens_OnValueChanged;
	}

	private void OnEnable()
	{
		if (ItemManager.Instance != null)
			ItemManager.Instance.RegisterItem(this);

		OnItemEnabled(this);

		if (collidersDirty)
			NotifyCollidersChanged();
	}

	private void OnDisable()
	{
		if (ItemManager.Instance != null)
			ItemManager.Instance.DeregisterItem(this);

		OnItemDisabled(this);
	}

	public void OnLoaded()
	{
		// OnItemLoaded?
	}

	private void OnValidate()
	{
		if (!Application.IsPlaying(this) &&
			GetComponent<ItemCollisionSFX>() == null)
		{
			gameObject.AddComponent<ItemCollisionSFX>();
		}
	}

	public void NotifyCollidersChanged()
	{
		if (isActiveAndEnabled)
		{
			RefreshBounds();
			OnCollidersChanged(this);
			collidersDirty = false;
		}
		else
			collidersDirty = true;
	}

	public void NotifyStateChanged()
	{
		OnStateChanged(this, null);
	}

	private void PropertyItem_OnStateChanged(PropertyItem property)
	{
		OnStateChanged(this, property);
	}

	private void UseGravityLens_OnValueChanged(bool useGravity)
	{
		RigidbodySettings settings = RigidbodySettings;
		settings.useGravity = useGravity;
		RigidbodySettings = settings;
	}

	private void DragLens_OnValueChanged(float drag)
	{
		RigidbodySettings settings = RigidbodySettings;
		settings.drag = drag;
		RigidbodySettings = settings;
	}

	private void AngularDragLens_OnValueChanged(float angularDrag)
	{
		RigidbodySettings settings = RigidbodySettings;
		settings.angularDrag = angularDrag;
		RigidbodySettings = settings;
	}

	public void RefreshColliders()
	{
		allColliders.Clear();

		Collider[] colliders = GetComponentsInChildren<Collider>(true);
		allColliders.AddRange(colliders);

		NotifyCollidersChanged();
	}

	public void RemoveRigidbody()
	{
		OnWillRemoveRigidbody(this, rigidbody);

		backupSettings = RigidbodySettings;

		// Rigidbody only gets destroyed at end of frame
		// so zero velocity / angular velocity incase this happens before FixedUpdate
		// in which case rigidbody will keep on moving

		rigidbody.isKinematic = true;
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;

		Destroy(rigidbody);

		OnRigidbodyRemoved(this);
	}

	public void RestoreRigidbody()
	{
		rigidbody = gameObject.GetComponent<Rigidbody>();
		if(rigidbody == null)
			rigidbody = gameObject.AddComponent<Rigidbody>();

		if (backupSettings.HasValue)
		{
			RigidbodySettings = backupSettings.Value;
			backupSettings = null;
		}

		OnRigidbodyAdded(this, rigidbody);
	}

	[ContextMenu("Refresh Bounds")]
	public void RefreshBounds()
	{
		localBounds = PhysicsUtils.EncapsulateColliders(allColliders, transform);
		OnBoundsChanged(this, localBounds);
	}

	public void ApplyItemState(ItemState itemState)
	{
		for (int i = 0; i < itemState.propertyStates.Count; i++)
		{
			string typeString = itemState.propertyStates[i].type;

			PropertyItem property = GetComponent(typeString) as PropertyItem;
			if (property == null)
			{
				Type type = ItemManager.Instance.GetTypeFromPropertyName(typeString); 
				if(type.IsSubclassOf(typeof(PropertyItem)))
					property = gameObject.AddComponent(type) as PropertyItem;
			}

			if(property != null)
				property.ApplyPropertyState(itemState.propertyStates[i]);
		}

		OnLoaded();
	}

	public ItemState BuildItemState()
	{
		ItemState itemState = new ItemState();
		itemState.persistent = GetProperty<PersistentItem>();
		itemState.itemName = Data.name;

		foreach(var kvp in typeToPropertyMap)
			itemState.propertyStates.Add(kvp.Value.BuildPropertyState());

		return itemState;
	}

	public virtual void OnDestroy()
	{
		OnItemDestroyed(this);
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.IsPlaying(this) && 
			allColliders.Count == 0)
		{
			RefreshColliders();
			RefreshBounds();
		}

		if (Rigidbody != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + Rigidbody.velocity);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position + Rigidbody.angularVelocity);
		}

		Gizmos.DrawSphere(Center, 0.02f);

		Matrix4x4 prevMatrix = Gizmos.matrix;

		Gizmos.color = Color.cyan;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(localBounds.center, localBounds.size);

		Gizmos.matrix = prevMatrix;
	}
}
