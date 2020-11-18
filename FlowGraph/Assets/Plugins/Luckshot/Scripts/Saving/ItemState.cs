using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemState
{
	public bool persistent = false;
	public string itemName = string.Empty;
	public List<PropertyState> propertyStates = new List<PropertyState>();
}
