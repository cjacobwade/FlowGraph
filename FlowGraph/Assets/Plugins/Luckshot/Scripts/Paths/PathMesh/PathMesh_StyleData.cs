using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Luckshot/Path Mesh/Style Data")]
public class PathMesh_StyleData : ScriptableObject
{
	[Header("Texture")]

	public Vector2 texelsPerUnit = new Vector2(2f, 0.07f);

	[Header("Materials")]

	public Material baseMaterial = null;

	public PhysicMaterial physicMaterial = null;

	[Header("Processors")]

	public List<MeshProcessData> processors = new List<MeshProcessData>();
}
