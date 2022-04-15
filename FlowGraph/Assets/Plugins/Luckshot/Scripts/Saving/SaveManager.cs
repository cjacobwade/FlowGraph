
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Luckshot.Platform;

public class SaveManager : Singleton<SaveManager>
{
	private List<Item> items = new List<Item>();

	private WorldState worldState = null;

	public event System.Action OnWillLoad = delegate {};
	public event System.Action OnLoadFinished = delegate {};

	private void Start()
	{
#if !UNITY_EDITOR && UNITY_STANDALONE
		BigHopsPrefs.Instance.LoadConfig();
#endif
	}

	public void RegisterItem(Item item)
	{
		items.Add(item);

		if(worldState != null)
		{
			for (int i = 0; i < worldState.itemStates.Count; i++)
			{
				ItemState itemState = worldState.itemStates[i];
				if (items[i].Data.name == itemState.itemName)
				{
					itemState.ApplyStateToItem(item);
				}
			}
		}
	}

	public void DeregisterItem(Item item)
	{ items.Remove(item); }

	public void CollectWorldState()
	{
		// TODO: Rather than restarting from scratch maybe can update things based on dirty state?

		worldState = new WorldState();
		for (int i = 0; i < items.Count; i++)
			worldState.itemStates.Add(items[i].BuildItemState());
	}

	[ContextMenu("Save World State")]
	public void SaveToDisk()
	{
		CollectWorldState();

		string saveOutput = JsonUtility.ToJson(worldState);
		PlatformServices.CurrentPlatform.SaveToDisk(saveOutput);
	}

	[ContextMenu("Load World State")]
	public bool LoadFromDisk()
	{
		OnWillLoad();

		string loadText = PlatformServices.CurrentPlatform.LoadFromDisk();
		if (string.IsNullOrEmpty(loadText))
			return false;

		worldState = JsonUtility.FromJson<WorldState>(loadText);

		for(int i = 0; i < worldState.itemStates.Count; i++)
		{
			ItemState itemState = worldState.itemStates[i];
			if (itemState != null)
			{
				if (itemState.persistent)
				{
					for (int j = 0; j < items.Count; j++)
					{
						if (items[j].Data.name == itemState.itemName)
						{
							itemState.ApplyStateToItem(items[j]);
							break;
						}
					}
				}
				else
				{
					ItemManager.Instance.InstantiateItemFromItemState(itemState);
				}
			}
		}

		OnLoadFinished();

		return true;
	}

	private void OnApplicationQuit()
	{
		if (BigHopsPrefs.Instance.AutoSaveOnQuit) 
		{
			SaveToDisk();
		}
	}
}
