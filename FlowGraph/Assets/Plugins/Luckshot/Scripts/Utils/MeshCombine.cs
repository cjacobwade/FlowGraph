using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCombine : MonoBehaviour
{
    [Button("Combine Children")]
    public void CombineChildren()
    {
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		if (meshFilter == null || meshRenderer == null)
			return;

		Vector3 prevPos = transform.position;
		Quaternion prevRot = transform.rotation;

		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		List<Material> materials = new List<Material>();
        List<List<CombineInstance>> combineInstanceLists = new List<List<CombineInstance>>();

        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in meshFilters)
        {
			if (mf.gameObject == gameObject)
				continue;

            MeshRenderer renderer = mf.GetComponent<MeshRenderer>();

            if (!renderer || !mf.sharedMesh ||
                renderer.sharedMaterials.Length != mf.sharedMesh.subMeshCount)
            {
                continue;
            }

            for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
            {
				List<CombineInstance> combineInstances = null;

				int materialArrayIndex = materials.IndexOf(renderer.sharedMaterials[i]);
				if (materialArrayIndex == -1)
				{
					materials.Add(renderer.sharedMaterials[i]);

					combineInstances = new List<CombineInstance>();
					combineInstanceLists.Add(combineInstances);
				}
				else
				{
					combineInstances = combineInstanceLists[materialArrayIndex];
				}

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = renderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = i;
                combineInstance.mesh = mf.sharedMesh;

                combineInstances.Add(combineInstance);
            }
        }

        CombineInstance[] perMaterialCombineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            List<CombineInstance> combineInstanceList = combineInstanceLists[m];

            Mesh perMaterialMesh = new Mesh();
            perMaterialMesh.CombineMeshes(combineInstanceList.ToArray(), true, true);

            CombineInstance combineInstance = new CombineInstance();
            combineInstance.mesh = perMaterialMesh;
            combineInstance.subMeshIndex = 0;

            perMaterialCombineInstances[m] = combineInstance;
        }

		foreach (MeshFilter mf in meshFilters)
			mf.gameObject.SetActive(false);

		gameObject.SetActive(true);

		Mesh mesh = new Mesh();
		mesh.name = gameObject.name + " Combined Mesh";

		mesh.CombineMeshes(perMaterialCombineInstances, false, false);
		meshFilter.sharedMesh = mesh;

		meshRenderer.sharedMaterials = materials.ToArray();

		MeshCollider meshCollider = GetComponent<MeshCollider>();
		if (meshCollider != null)
			meshCollider.sharedMesh = mesh;

		transform.position = prevPos;
		transform.rotation = prevRot;
	}

	[Button("Undo Combine")]
	private void UndoCombine()
	{
		MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>(true);
		foreach (var meshFilter in meshFilters)
			meshFilter.gameObject.SetActive(true);

		ClearCombined();
	}

	private void ClearCombined()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter != null && meshFilter.sharedMesh != null)
			DestroyImmediate(meshFilter.sharedMesh);
	}
}
