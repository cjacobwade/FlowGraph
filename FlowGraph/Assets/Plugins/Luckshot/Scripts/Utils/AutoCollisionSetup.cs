using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class AutoCollisionSetup : MonoBehaviour
{
#if UNITY_EDITOR
	void OnEnable()
	{
		EditorApplication.hierarchyChanged -= SetupCollision;
		EditorApplication.hierarchyChanged += SetupCollision;
	}

	void OnDisable()
	{
		EditorApplication.hierarchyChanged -= SetupCollision;
	}

	void SetupCollision()
	{
		if (!Application.isPlaying)
		{
			MeshRenderer[] meshChildren = transform.GetComponentsInChildren<MeshRenderer>();
			IEnumerable<GameObject> childMeshColliderGos = transform.GetComponentsInChildren<MeshCollider>().Select(mc => mc.gameObject);
			for (int i = 0; i < meshChildren.Length; i++)
			{
				if (!childMeshColliderGos.Contains(meshChildren[i].gameObject))
					meshChildren[i].gameObject.AddComponent<MeshCollider>();
			}
		}
	}
#endif
}