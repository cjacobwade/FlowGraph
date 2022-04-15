using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProcessor_ApplyData : MeshProcessor
{
	[SerializeField]
	private MeshProcessData meshProcessData = null;

	public override Mesh ProcessMesh(Mesh mesh)
	{ return meshProcessData.ProcessMesh(mesh); }
}
