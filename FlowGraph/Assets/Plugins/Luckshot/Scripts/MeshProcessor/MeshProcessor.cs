using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeshProcessor : MonoBehaviour
{
	public abstract Mesh ProcessMesh(Mesh mesh);
}
