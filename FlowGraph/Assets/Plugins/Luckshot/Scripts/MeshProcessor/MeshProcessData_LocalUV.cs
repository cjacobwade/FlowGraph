using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProcessData_LocalUV : MeshProcessData
{
	public override Mesh ProcessMesh(Mesh mesh)
	{
		Mesh uvMesh = MonoBehaviour.Instantiate(mesh);

		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		uvMesh.GetVertices(verts);
		uvMesh.GetUVs(0, uvs);
		
		for(int i =0; i < uvs.Count; i++)
		{
			Vector3 localPos = verts[i];
			uvs[i] = new Vector2(localPos.x, localPos.z);
		}

		uvMesh.SetVertices(verts);
		uvMesh.SetUVs(0, uvs);

		return uvMesh;
	}
}
