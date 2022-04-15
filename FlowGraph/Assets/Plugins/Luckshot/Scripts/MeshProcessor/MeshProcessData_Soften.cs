using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshProcessData_Soften : MeshProcessData
{
	public override Mesh ProcessMesh(Mesh mesh)
	{
		Mesh softenedMesh = MonoBehaviour.Instantiate(mesh);

		List<Vector3> verts = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();

		softenedMesh.GetVertices(verts);
		softenedMesh.GetNormals(normals);

		Dictionary<Vector3, List<int>> posToIndicesMap = new Dictionary<Vector3, List<int>>();
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 ds = verts[i];

			ds.x = (float)Math.Round(ds.x, 2);
			ds.y = (float)Math.Round(ds.y, 2);
			ds.z = (float)Math.Round(ds.z, 2);

			if (!posToIndicesMap.TryGetValue(ds, out List<int> indices))
			{
				indices = new List<int>();
				posToIndicesMap.Add(ds, indices);
			}

			indices.Add(i);
		}

		foreach (var kvp in posToIndicesMap)
		{
			Vector3 averageNormal = Vector3.zero;
			for (int i = 0; i < kvp.Value.Count; i++)
				averageNormal += normals[kvp.Value[i]];

			averageNormal /= kvp.Value.Count;
			for (int i = 0; i < kvp.Value.Count; i++)
				normals[kvp.Value[i]] = averageNormal;
		}

		softenedMesh.SetVertices(verts);
		softenedMesh.SetNormals(normals);

		return softenedMesh;
	}
}
