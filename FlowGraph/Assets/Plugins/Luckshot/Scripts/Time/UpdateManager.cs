using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : Singleton<UpdateManager>
{
	private List<System.Action> registeredUpdates = new List<System.Action>();
	private List<System.Action> registeredFixedUpdates = new List<System.Action>();
	private List<System.Action> registeredLateUpdates = new List<System.Action>();

	protected override void Awake()
	{
		base.Awake();
		enabled = false;
	}

	private void CheckEnableState()
	{
		bool anyRegistered = registeredUpdates.Count > 0 ||
			registeredLateUpdates.Count > 0 ||
			registeredFixedUpdates.Count > 0;

		enabled = anyRegistered;
	}

	private void Update()
	{
		for(int i = 0; i < registeredUpdates.Count; i++)
			registeredUpdates[i]();
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < registeredFixedUpdates.Count; i++)
			registeredFixedUpdates[i]();
	}

	private void LateUpdate()
	{
		for (int i = 0; i < registeredLateUpdates.Count; i++)
			registeredLateUpdates[i]();
	}

	public void RegisterUpdate(System.Action action)
	{
		registeredUpdates.Add(action);
		CheckEnableState();
	}

	public void UnregisterUpdate(System.Action action)
	{
		registeredUpdates.Remove(action);
		CheckEnableState();
	}

	public void RegisterFixedUpdate(System.Action action)
	{
		registeredFixedUpdates.Add(action);
		CheckEnableState();
	}

	public void UnregisterFixedUpdate(System.Action action)
	{
		registeredFixedUpdates.Remove(action);
		CheckEnableState();
	}

	public void RegisterLateUpdate(System.Action action)
	{
		registeredLateUpdates.Add(action);
		CheckEnableState();
	}

	public void UnregisterLateUpdate(System.Action action)
	{
		registeredLateUpdates.Remove(action);
		CheckEnableState();
	}
}
