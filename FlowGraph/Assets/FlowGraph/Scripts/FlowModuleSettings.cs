using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FlowModuleSettings", menuName = "Luckshot/FlowModuleSettings")]
public class FlowModuleSettings : ScriptableObject
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

	[SerializeField]
	[FlowModuleName]
	private string moduleName = string.Empty;

	[SerializeField]
	[FlowFunctionName("moduleName2")]
	private string functionName = string.Empty;

	private Dictionary<string, UniqueObjectData> moduleToUODMap = null;

	public void Initialize()
	{
		moduleToUODMap = new Dictionary<string, UniqueObjectData>(); 

		foreach (var md in moduleDefaults)
			moduleToUODMap.Add(md.moduleName, md.forceUOD);
	}

	public UniqueObjectData GetDefaultUOD(string module)
	{
		if (moduleToUODMap == null)
			Initialize();

		moduleToUODMap.TryGetValue(module, out UniqueObjectData uod);
		return uod;
	}
}
