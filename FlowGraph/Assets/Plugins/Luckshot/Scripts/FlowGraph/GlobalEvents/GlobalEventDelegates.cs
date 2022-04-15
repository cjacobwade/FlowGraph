using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ItemEvent(ItemData item);
public delegate void ItemUsedEvent(ItemData item, ItemData user);
public delegate void UniqueObjectEvent(UniqueObjectData uod);
public delegate void UniqueObjectUsedEvent(UniqueObjectData uod, ItemData user);
