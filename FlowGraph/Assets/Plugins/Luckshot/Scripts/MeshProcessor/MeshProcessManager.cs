using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MeshProcessManager
{
	private static Dictionary<MeshProcessData, Dictionary<int, Mesh>> processorToMeshMap = new Dictionary<MeshProcessData, Dictionary<int, Mesh>>();
	
	public static Mesh ProcessMesh(this MeshProcessData processor, Mesh mesh)
	{
		if (processor == null)
		{
			Debug.LogWarning("Processor is null");
			return mesh;
		}

		if(processorToMeshMap.TryGetValue(processor, out Dictionary<int, Mesh> meshMap))
		{
			if (meshMap.TryGetValue(mesh.GetInstanceID(), out Mesh existingMesh))
			{
				return existingMesh;
			}
		}
		else
		{
			meshMap = new Dictionary<int, Mesh>();
			processorToMeshMap[processor] = meshMap;
		}

		Mesh processedMesh = processor.ProcessMesh(mesh);
		meshMap[mesh.GetInstanceID()] = processedMesh;

		return processedMesh;
	}

	
}
