using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "FlowGraphSettings", menuName = "Luckshot/Flow/Flow Graph Settings")]
public class FlowGraphSettings : ScriptableObject
{
	[System.Serializable]
	public class ModuleDefault
	{
		[FlowModuleName]
		public string moduleName = string.Empty;
		public UniqueObjectData forceUOD = null;
	}

	[SerializeField]
	private ModuleDefault[] moduleDefaults = null;

	[System.Serializable]
	public class DragAndDropSettings
	{
		public UnityEngine.Object typeObject = null;
		[FlowModuleName]
		public string module = string.Empty;
		[FlowFunctionName("module")]
		public string function = string.Empty;
	}

	[SerializeField]
	private DragAndDropSettings[] dragAndDropSettings = null;

	private Dictionary<string, UniqueObjectData> moduleToUODMap = new Dictionary<string, UniqueObjectData>();

	private Dictionary<Type, DragAndDropSettings> typeToDragSettingsMap = new Dictionary<Type, DragAndDropSettings>();

	private bool initialized = false;

	private void OnValidate()
	{
		initialized = false;
	}

	private void InitializeIfNeeded()
	{
		if (!initialized)
		{
			Initialize();
			initialized = true;
		}
	}

	public void Initialize()
	{
		moduleToUODMap.Clear();

		foreach (var md in moduleDefaults)
		{
			if(md.forceUOD != null)
				moduleToUODMap.Add(md.moduleName, md.forceUOD);
		}

		typeToDragSettingsMap.Clear();

		foreach (var settings in dragAndDropSettings)
		{
			if(settings.typeObject != null)
				typeToDragSettingsMap.Add(settings.typeObject.GetType(), settings);
		}
	}

	public UniqueObjectData GetDefaultUOD(string module)
	{
		InitializeIfNeeded();
		moduleToUODMap.TryGetValue(module, out UniqueObjectData uod);
		return uod;
	}

	public DragAndDropSettings GetDragAndDropSettingsForType(Type type)
	{
		InitializeIfNeeded();

		DragAndDropSettings settings = null;
		if(type != null)
			typeToDragSettingsMap.TryGetValue(type, out settings);

		return settings;
	}
}
