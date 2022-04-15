using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class VisibilityListener : MonoBehaviour
{
	[SerializeField]
	[ReadOnly]
	private bool isVisible = false;
	public bool IsVisible
	{ get { return isVisible; } }

	public event System.Action<bool> OnVisibilityChanged = delegate { };

	private void Awake()
	{
		Renderer renderer = GetComponent<Renderer>();
		if (renderer != null)
			isVisible = renderer.isVisible;
	}

	void OnBecameVisible()
	{
		isVisible = true;
		OnVisibilityChanged(true);
	}

	void OnBecameInvisible()
	{
		isVisible = false;
		OnVisibilityChanged(false);
	}
}
