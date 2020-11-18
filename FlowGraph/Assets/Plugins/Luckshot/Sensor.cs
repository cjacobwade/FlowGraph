using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor<T> : MonoBehaviour where T : Component
{
	protected List<T> collection = new List<T>();
	public List<T> Collection
	{
		get
		{
			for(int i =0;i < collection.Count; i++)
			{
				if (collection[i] == null)
				{
					T unsensed = collection[i];
				
					collection.RemoveAt(i--);
					OnItemExited(unsensed);
				}
			}

			return collection;
		}
	}

	public System.Action<T> OnItemEntered = delegate {};
	public System.Action<T> OnItemExited = delegate {};

	private void OnDisable()
	{
		for(int i = 0; i < collection.Count; i++)
		{
			T sensed = collection[i];

			collection.RemoveAt(i--);
			OnItemExited(sensed);
		}
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		T sensed = null;
		if(other.attachedRigidbody)
			sensed = other.attachedRigidbody.GetComponent<T>();
		else
			sensed = other.GetComponent<T>();

		if(sensed && !collection.Contains(sensed))
		{
			collection.Add(sensed);
			OnItemEntered(sensed);
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		T sensed = null;
		if (other.attachedRigidbody)
			sensed = other.attachedRigidbody.GetComponent<T>();
		else
			sensed = other.GetComponent<T>();

		if (sensed && collection.Contains(sensed))
		{
			collection.Remove(sensed);
			OnItemExited(sensed);
		}
	}
}
