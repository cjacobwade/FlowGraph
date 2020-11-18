using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCombine : MonoBehaviour
{
	List<MeshFilter> filtersToCopy = new List<MeshFilter>();

	[Button("Combine Children")]
	public void CombineChildren()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter != null)
		{
			meshFilter.mesh = null;

			Vector3 prevPos = transform.position;
			Quaternion prevRot = transform.rotation;

			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;

			filtersToCopy.Clear();
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			for(int i= 0; i < renderers.Length; i++)
			{
				MeshFilter childFilter = renderers[i].GetComponent<MeshFilter>();
				if(childFilter && childFilter.sharedMesh != null)
				{
					filtersToCopy.Add(childFilter);
				}
			}

			CombineInstance[] combine = new CombineInstance[filtersToCopy.Count];
			for (int i = 0; i < combine.Length; i++)
			{
				combine[i].mesh = Instantiate(filtersToCopy[i].sharedMesh);
				combine[i].transform = filtersToCopy[i].transform.localToWorldMatrix;
				filtersToCopy[i].gameObject.SetActive(false);
			}

			Mesh mesh = new Mesh();
			mesh.name = gameObject.name + " Combined Mesh";

			mesh.CombineMeshes(combine);
			meshFilter.sharedMesh = mesh;

			transform.position = prevPos;
			transform.rotation = prevRot;
		}
	}

	[Button("Clear")]
	void ClearCombined()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter != null && meshFilter.sharedMesh != null)
			DestroyImmediate(meshFilter.sharedMesh);
	}
}
