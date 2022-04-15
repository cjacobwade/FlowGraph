
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

	public RigidbodySettings(Rigidbody rigidbody)
	{
		mass = rigidbody.mass;
		drag = rigidbody.drag;
		angularDrag = rigidbody.angularDrag;
	}

	public void Apply(Rigidbody rigidbody)
	{
		rigidbody.mass = mass;
		rigidbody.drag = drag;
		rigidbody.angularDrag = angularDrag;
	}
}

[DefaultExecutionOrder(-3)]
[SelectionBase]
public class Item : MonoBehaviour
{
	public static bool FindNearestParentItem(Collider collider, out Item item)
	{ return FindNearestParentItem(collider.transform, out item); }

	public static bool FindNearestParentItem(Transform transform, out Item item)
	{
		item = null;

		if (transform != null)
			item = transform.GetComponentInParent<Item>();

		return item != null;
	}

	[SerializeField]
	private ItemData itemData = null;
	public ItemData Data => itemData;

	public LensManagerColor ColorLens = new LensManagerColor(requests => LensUtils.Priority(requests));

	public Color CurrentColor
	{
		get
		{
			if (ColorLens.GetRequestCount > 0)
				return ColorLens;

			if (itemData != null)
				return itemData.color;

			return Color.white;
		}
	}

	public LensManagerString NameLens = new LensManagerString(requests => LensUtils.Priority(requests));

	public string CurrentName
	{
		get
		{
			if (NameLens.GetRequestCount > 0)
				return NameLens;

			if (itemData != null)
				return itemData.Name;

			return gameObject.name;
		}
	}

#if UNITY_EDITOR
	[Button("Find Or Create Item Data")]
	public void FindOrCreateItemData()
	{ itemData = ItemData.CreateItemData("Assets/Resources/ItemDatas", gameObject.name); }
#endif

	#region Properties
	private Dictionary<Type, PropertyItem> typeToPropertyMap = new Dictionary<Type, PropertyItem>();
	public Dictionary<Type, PropertyItem> TypeToPropertyMap => typeToPropertyMap;

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

	public PropertyItem GetProperty(Type type)
    {
		if(!typeToPropertyMap.TryGetValue(type, out PropertyItem property))
        {
			foreach(var kvp in typeToPropertyMap)
            {
				if (kvp.Key.IsSubclassOf(type))
					return kvp.Value;
            }
        }

		return property;
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
	public Rigidbody Rigidbody => rigidbody;

	public LensManagerBool UseGravityLens = null;
	public LensManagerBool KinematicLens = null;
	public LensManager<CollisionDetectionMode> CollisionModeLens = null;
	public LensManager<RigidbodyConstraints> ConstraintsLens = null;

	public LensManagerFloat DragLens = null;
	public LensManagerFloat AngularDragLens = null;

	private List<Collider> allColliders = new List<Collider>();
	public List<Collider> AllColliders => allColliders;

	[SerializeField, ReadOnly]
	private Bounds localBounds = new Bounds();
	public Bounds LocalBounds => localBounds;

	public Bounds CalculateWorldBounds()
	{ return PhysicsUtils.CalculateCollidersBounds(AllColliders); }

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

	#endregion // Physics

	#region IgnoredColliders
#if UNITY_EDITOR
	[SerializeField, ReadOnly]
	private List<Collider> ignoredColliders = new List<Collider>();
#else
	private HashSet<Collider> ignoredColliders = new HashSet<Collider>();
#endif

	public bool IsColliderIgnored(Collider collider)
	{ return ignoredColliders.Contains(collider); }

	public void SetIgnoreCollider(Collider otherCollider, bool setIgnore = true)
    {
		foreach(var collider in allColliders)
			Physics.IgnoreCollision(collider, otherCollider, setIgnore);

		if (setIgnore)
			_AddIgnoredCollider(otherCollider);
		else
			_RemoveIgnoredCollider(otherCollider);
    }

	public void SetIgnoreColliders(IEnumerable<Collider> otherColliders, bool setIgnore = true)
	{
		foreach(var collider in allColliders)
		{
			foreach (var otherCollider in otherColliders)
			{
				_IgnoreCollision(collider, otherCollider, setIgnore);
			}
		}

		if (setIgnore)
			_AddIgnoredColliders(otherColliders);
		else
			_RemoveIgnoredColliders(otherColliders);
	}

	private void _IgnoreCollision(Collider a, Collider b, bool setIgnore = true)
	{
		if (a != null && b != null)
			Physics.IgnoreCollision(a, b, setIgnore);
	}

	private void _AddIgnoredCollider(Collider addCollider)
	{ ignoredColliders.Add(addCollider); }

	private bool _RemoveIgnoredCollider(Collider removeCollider)
	{ return ignoredColliders.Remove(removeCollider); }

	private void _AddIgnoredColliders(IEnumerable<Collider> addColliders)
	{
		foreach (var addCollider in addColliders)
			_AddIgnoredCollider(addCollider);
	}

	private bool _RemoveIgnoredColliders(IEnumerable<Collider> removeColliders)
	{
		bool removed = false;
		foreach (var removeCollider in removeColliders)
		{
			if (_RemoveIgnoredCollider(removeCollider))
				removed = true;
		}
		return removed;
	}

	private static List<Collider> ignoreAColliders = new List<Collider>();
	private static List<Collider> ignoreBColliders = new List<Collider>();

	public static void SetIgnoreCollisionBetweenItems(Item aItem, Item bItem, bool setIgnored = true)
	{
		if (aItem == null || bItem == null)
			return;

		ignoreAColliders.Clear();
		ignoreBColliders.Clear();

		for (int j = 0; j < aItem.AllColliders.Count; j++)
		{
			Collider aCollider = aItem.AllColliders[j];
			if (aCollider == null || aCollider.isTrigger)
				continue;

			ignoreAColliders.Add(aCollider);
		}

		for (int k = 0; k < bItem.AllColliders.Count; k++)
		{
			Collider bCollider = bItem.AllColliders[k];
			if (bCollider == null || bCollider.isTrigger)
				continue;

			ignoreBColliders.Add(bCollider);
		}

		for (int i = 0; i < ignoreAColliders.Count; i++)
		{
			Collider aCollider = ignoreAColliders[i];

			for (int j = 0; j < ignoreBColliders.Count; j++)
			{
				Collider bCollider = ignoreBColliders[j];
				Physics.IgnoreCollision(aCollider, bCollider, setIgnored);
			}
		}

		if (setIgnored)
		{
			aItem._AddIgnoredColliders(ignoreBColliders);
			bItem._AddIgnoredColliders(ignoreAColliders);
		}
		else
		{
			aItem._RemoveIgnoredColliders(ignoreBColliders);
			bItem._RemoveIgnoredColliders(ignoreAColliders);
		}
	}
	#endregion

	[SerializeField, AutoCache]
	private VisualsModel visualsModel = null;
	public VisualsModel VisualsModel => visualsModel;

	[SerializeField, AutoCache]
	private ColliderModel colliderModel = null;
	public ColliderModel ColliderModel => colliderModel;

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

	private bool beingDestroyed = false;
	public bool BeingDestroyed => beingDestroyed;

	public event Action<Item> OnItemDestroyed = delegate { };

	private bool collidersDirty = false;

	public virtual void Awake()
	{
		RefreshColliders();

		if (visualsModel == null)
		{
			visualsModel = GetComponent<VisualsModel>();
			if(visualsModel == null)
				visualsModel = gameObject.AddComponent<VisualsModel>();
		}

		if (colliderModel == null)
		{
			colliderModel = GetComponent<ColliderModel>();
			if(colliderModel == null)
				colliderModel = gameObject.AddComponent<ColliderModel>();
		}
		
		float defaultDrag = 0f;
		float defaultAngularDrag = 0.05f;

		bool defaultKinematic = false;
		bool defaultUseGravity = true;

		CollisionDetectionMode defaultCollisionMode = CollisionDetectionMode.Discrete;
		RigidbodyConstraints defaultConstraints = RigidbodyConstraints.None;

		if (rigidbody != null)
		{
			defaultDrag = rigidbody.drag;
			defaultAngularDrag = rigidbody.angularDrag;

			defaultKinematic = rigidbody.isKinematic;
			defaultUseGravity = rigidbody.useGravity;

			defaultCollisionMode = rigidbody.collisionDetectionMode;
			defaultConstraints = rigidbody.constraints;
		}

		UseGravityLens = new LensManagerBool(requests => LensUtils.AllTrue(requests, defaultUseGravity));
		UseGravityLens.OnValueChanged += UseGravityLens_OnValueChanged;

		KinematicLens = new LensManagerBool(requests => LensUtils.AnyTrue(requests, defaultKinematic));
		KinematicLens.OnValueChanged += KinematicLens_OnValueChanged;

		CollisionModeLens = new LensManager<CollisionDetectionMode>(requests => LensUtils.Priority(requests, defaultCollisionMode));
		CollisionModeLens.OnValueChanged += CollisionModeLens_OnValueChanged;

		ConstraintsLens = new LensManager<RigidbodyConstraints>(requests => LensUtils.Priority(requests, defaultConstraints));
		ConstraintsLens.OnValueChanged += ConstraintsLens_OnValueChanged;

		DragLens = new LensManagerFloat(requests => LensUtils.Priority(requests, defaultDrag));
		DragLens.OnValueChanged += DragLens_OnValueChanged;

		AngularDragLens = new LensManagerFloat(requests => LensUtils.Priority(requests, defaultAngularDrag));
		AngularDragLens.OnValueChanged += AngularDragLens_OnValueChanged;
	}

	private void Start()
	{ 
		// Make sure any disabled components still get initialized to have Item ref
		PropertyItem[] propertyItems = GetComponents<PropertyItem>();
		for (int i = 0; i < propertyItems.Length; i++)
			propertyItems[i].Awake();
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


	[ContextMenu("Refresh Colliders")]
	public void RefreshColliders()
	{
		allColliders.Clear();

		Collider[] colliders = GetComponentsInChildren<Collider>(true);
		allColliders.AddRange(colliders);

		NotifyCollidersChanged();
	}

	private void UseGravityLens_OnValueChanged(bool useGravity)
	{
		if(Rigidbody != null)
			Rigidbody.useGravity = useGravity;
	}

	private void KinematicLens_OnValueChanged(bool isKinematic)
	{
		if (Rigidbody != null)
		{
			Rigidbody.collisionDetectionMode = isKinematic ? CollisionDetectionMode.Discrete : CollisionModeLens;
			Rigidbody.isKinematic = isKinematic;
		}
	}

	private void CollisionModeLens_OnValueChanged(CollisionDetectionMode collisionMode)
	{
		if (Rigidbody != null)
			Rigidbody.collisionDetectionMode = KinematicLens ? CollisionDetectionMode.Discrete : collisionMode;
	}

	private void ConstraintsLens_OnValueChanged(RigidbodyConstraints constraints)
	{
		if (Rigidbody != null)
			Rigidbody.constraints = constraints;
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

	public void RemoveRigidbody()
	{
		if (rigidbody == null)
			return;

		OnWillRemoveRigidbody(this, rigidbody);

		backupSettings = RigidbodySettings;

		// Rigidbody only gets destroyed at end of frame
		// so zero velocity / angular velocity incase this happens before FixedUpdate
		// in which case rigidbody will keep on moving

		rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // only here to avoid warning
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

		rigidbody.useGravity = UseGravityLens;
		rigidbody.isKinematic = KinematicLens;
		rigidbody.constraints = ConstraintsLens;
		rigidbody.collisionDetectionMode = rigidbody.isKinematic ? CollisionDetectionMode.Discrete : CollisionModeLens;

		if (backupSettings.HasValue)
		{
			RigidbodySettings = backupSettings.Value;
			backupSettings = null;
		}

		OnRigidbodyAdded(this, rigidbody);
	}

	public void RefreshBounds()
	{
		localBounds = PhysicsUtils.CalculateCollidersBounds(allColliders, transform);
		OnBoundsChanged(this, localBounds);
	}

	public virtual void OnDestroy()
	{
		if (!Singleton.IsQuitting)
		{
			beingDestroyed = true;
			OnItemDestroyed(this);
		}
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
