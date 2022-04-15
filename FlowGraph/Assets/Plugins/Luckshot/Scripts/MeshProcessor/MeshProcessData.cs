using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshProcessData", menuName = "Luckshot/Mesh Process Data")]
public abstract class MeshProcessData : ScriptableObject
{
	public abstract Mesh ProcessMesh(Mesh mesh);
}
