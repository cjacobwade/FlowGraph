using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Paths;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PathMesh : MonoBehaviour, IMeshProcessListener
{
	[System.Serializable]
	public class OffsetStrip
	{
		public Vector2[] offsets = new Vector2[2] { new Vector2(0, 0f), new Vector2(1f, 0f)};
		public Material material = null;
	}

	[Header("Appearance")]

	[SerializeField]
	private PathMesh_StyleData style = null;
	public PathMesh_StyleData Style => style;

	[SerializeField]
	private PathMesh_ContourData contourData = null;
	public PathMesh_ContourData ContourData => contourData;

	[SerializeField]
	private PathMesh_ContourData collisionContourData = null;
	public PathMesh_ContourData CollisionContourData => collisionContourData;

	[Header("Settings")]

	[SerializeField, AutoCache]
	private PathBase path = null;
	public PathBase Path => path;

	[SerializeField]
	private float sidesPerUnit = 0.5f;

	[SerializeField, Range(0f, 1f)]
	private float fillAmount = 1f;

	[SerializeField]
	private bool closeFrontEnd = true;

	[SerializeField]
	private bool closeBackEnd = true;

	[SerializeField]
	private bool flipFaces = false;

	private static int CAPACITY = 512;
	private static int TRI_CAPACITY = 1024;

	private List<Vector3> verts = new List<Vector3>(CAPACITY);
	private List<Vector3> normals = new List<Vector3>(CAPACITY);
	private List<Vector2> uvs = new List<Vector2>(CAPACITY);
	
	protected static List<List<int>> triLists = new List<List<int>>
	{
		new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY),
		new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY), new List<int>(TRI_CAPACITY)
	};

	[Header("Mesh")]

	[SerializeField]
	private Mesh serializedMesh = null;

	private MeshRenderer meshRenderer = null;
	public MeshRenderer MeshRenderer
	{
		get
		{
			if (meshRenderer == null)
				meshRenderer = GetComponent<MeshRenderer>();

			return meshRenderer;
		}
	}

	private MeshFilter meshFilter = null;
	public MeshFilter MeshFilter
	{
		get
		{
			if (meshFilter == null)
				meshFilter = GetComponent<MeshFilter>();

			return meshFilter;
		}
	}

	private MeshCollider meshCollider = null;
	public MeshCollider MeshCollider
	{
		get
		{
			if (meshCollider == null)
				meshCollider = GetComponent<MeshCollider>();

			return meshCollider;
		}
	}

	private static HashSet<Material> allMaterials = new HashSet<Material>();
	private static List<Material> materials = new List<Material>();

	public void RegenerateIfNeeded()
	{
		if (serializedMesh == null || MeshFilter.sharedMesh != serializedMesh)
		{
			Regenerate();
		}
	}

	public event System.Action<PathMesh> PathMeshChanged = delegate {};

	public void NotifyMeshProcessorChanged(MeshProcessor meshProcessor)
	{ PathMeshChanged(this); }

	public void Regenerate()
	{
		if (!gameObject.scene.IsValid())
			return;

		if (MeshRenderer.isPartOfStaticBatch)
			return;

		if (style == null || contourData == null)
			return;

		serializedMesh = GenerateMesh(contourData);

		MeshFilter.sharedMesh = serializedMesh;
		MeshRenderer.sharedMaterials = GetMaterials().ToArray();

		if (MeshCollider != null)
		{
			Mesh collisionMesh = serializedMesh;
			if (collisionContourData != null &&
				collisionContourData != contourData)
			{
				collisionMesh = GenerateMesh(collisionContourData);
			}

			MeshCollider.sharedMesh = collisionMesh;
			MeshCollider.sharedMaterial = style.physicMaterial;
		}
	}

	public Mesh GenerateMesh(PathMesh_ContourData contour)
	{
		verts.Clear();
		normals.Clear();
		uvs.Clear();

		foreach (var triList in triLists)
			triList.Clear();

		float length = path.GetLength() * fillAmount;
		int sides = Mathf.CeilToInt(length * sidesPerUnit);
		if (sides <= 1)
			return null;

		List<Vector3> poses = new List<Vector3>();
		List<Vector3> norms = new List<Vector3>();
		List<Vector3> rights = new List<Vector3>();
		List<Vector3> scalars = new List<Vector3>();

		float alpha = 0f;
		float prevAlpha = 0f;

		bool reversed = false;

		while(true)
		{
			bool reachedEnd = alpha > fillAmount || reversed;
			bool passedLoop = path.Loop && prevAlpha > 0.5f && alpha < 0.5f;

			if (reachedEnd || passedLoop)
				alpha = fillAmount;

			Vector3 pos = path.GetPoint(alpha);
			Vector3 normal = path.GetNormal(alpha);
			Vector3 direction = path.GetDirection(alpha);
			Vector3 right = Vector3.Cross(normal, direction).normalized;
			Vector2 scalar = path.GetScalar(alpha);

			poses.Add(pos);
			norms.Add(normal);
			rights.Add(right);
			scalars.Add(scalar);

			if (alpha >= fillAmount)
				break;

			prevAlpha = alpha;
			alpha = PathUtils.MoveAtFixedSpeed(path, alpha, length / sides, ref reversed);
		}

		sides = poses.Count;

		OffsetStrip[] strips = contour.offsetStrips;

		int numOffsets = 0;
		for (int i = 0; i < strips.Length; i++)
			numOffsets += strips[i].offsets.Length;

		int offsetNum = 0;
		Vector2 prevOffset = Vector2.zero;

		float totalUDist = 0f;
		for (int i = 0; i < strips.Length; i++)
		{
			for (int j = 0; j < strips[i].offsets.Length; j++)
			{
				Vector2 offset = strips[i].offsets[j];

				if (offsetNum > 0)
				{
					float dist = (offset - prevOffset).magnitude;
					totalUDist += dist;
				}

				prevOffset = offset;
				offsetNum++;
			}
		}

		offsetNum = 0;
		float uDist = 0f;

		for (int i = 0; i < strips.Length; i++)
		{
			int matIndex = GetMaterials().IndexOf(strips[i].material);
			if (matIndex < 0)
				matIndex = 0;

			List<int> triList = triLists[matIndex];

			for (int j = 0; j < strips[i].offsets.Length; j++)
			{
				Vector2[] offsets = strips[i].offsets;

				float vDist = 0f;
				float startUDist = uDist;

				for (int k = 0; k < sides; k++)
				{
					if (j > 0)
					{
						prevOffset = offsets[j - 1];
						Vector2 offset = offsets[j];

						Vector3 prevVert = transform.InverseTransformPoint(poses[k] +
							norms[k] * prevOffset.y * path.Radius * scalars[k].y +
							rights[k] * prevOffset.x * path.Radius * scalars[k].x);

						Vector3 vert = transform.InverseTransformPoint(poses[k] +
							norms[k] * offset.y * path.Radius * scalars[k].y +
							rights[k] * offset.x * path.Radius * scalars[k].x);

						if (k > 0)
							vDist += (poses[k] - poses[k - 1]).magnitude;

						// Vert 1
						verts.Add(prevVert);

						Vector2 uv = new Vector2(startUDist / totalUDist, vDist);
						uv.Scale(style.texelsPerUnit);
						uvs.Add(uv);

						uDist = startUDist + (offset - prevOffset).magnitude;

						// Vert 2
						verts.Add(vert);

						uv = new Vector2(uDist / totalUDist, vDist);
						uv.Scale(style.texelsPerUnit);
						uvs.Add(uv);

						if (k > 0)
						{
							int startIndex = verts.Count - 1;

							triList.Add(startIndex - 2);
							triList.Add(startIndex - 1);
							triList.Add(startIndex);

							triList.Add(startIndex - 2);
							triList.Add(startIndex - 3);
							triList.Add(startIndex - 1);
						}
					}
				}

				offsetNum++;
			}
		}

		if (!path.Loop)
		{
			if (closeFrontEnd)
				AddEndCover(false, poses[0], norms[0], rights[0], scalars[0]);

			if (closeBackEnd)
				AddEndCover(true, poses[sides - 1], norms[sides - 1], rights[sides - 1], scalars[sides - 1]);
		}

		for (int i = 0; i < verts.Count; i++)
			normals.Add(Vector3.zero);

		for (int i = 0; i < triLists.Count; i++)
		{
			List<int> triList = triLists[i];
			for (int j = 0; j < triList.Count; j += 3)
			{
				Vector3 a = verts[triList[j]];
				Vector3 b = verts[triList[j + 1]];
				Vector3 c = verts[triList[j + 2]];

				Vector3 normal = Vector3.Cross(b - a, c - b).normalized;
				normals[triList[j]] = normal;
				normals[triList[j + 1]] = normal;
				normals[triList[j + 2]] = normal;
			}
		}

		CalculateNormals();

		Mesh mesh = new Mesh();
		mesh.name = "Generated Path Mesh";

		mesh.SetVertices(verts);
		mesh.SetNormals(normals);
		mesh.SetUVs(0, uvs);

		int submeshIndex = 0;
		for (int i = 0; i < triLists.Count; i++)
		{
			List<int> triList = triLists[i];
			if (triList.Count > 0)
			{
				if (flipFaces)
				{
					for (int j = 0; j < triList.Count; j += 3)
					{
						int temp = triList[j];
						triList[j] = triList[j + 2];
						triList[j + 2] = temp;
					}
				}

				mesh.subMeshCount = i + 1;
				mesh.SetIndices(triLists[i], MeshTopology.Triangles, submeshIndex++);
			}
		}

		foreach (var processor in style.processors)
			mesh = MeshProcessManager.ProcessMesh(processor, mesh);

		return mesh;
	}

	private List<Material> GetMaterials()
	{
		allMaterials.Clear();
		allMaterials.Add(style.baseMaterial);

		foreach (var strip in contourData.offsetStrips)
			allMaterials.Add(strip.material);

		materials.Clear();

		foreach(var mat in allMaterials)
		{
			if (mat != null)
				materials.Add(mat);
		}

		return materials;
	}

	private void AddEndCover(bool flip, Vector3 pos, Vector3 normal, Vector3 right, Vector2 scalar)
	{
		int startIndex = verts.Count;

		verts.Add(transform.InverseTransformPoint(pos));
		uvs.Add(new Vector2(0f, 0f));

		OffsetStrip[] strips = contourData.offsetStrips;
		for (int i = 0; i < strips.Length; i++)
		{
			Vector2[] offsets = strips[i].offsets;
			for (int j = 0; j < offsets.Length; j++)
			{
				Vector2 offset = offsets[j] * path.Radius;
				Vector3 vert = transform.InverseTransformPoint(pos + normal * offset.y * scalar.y + right * offset.x * scalar.x);
				verts.Add(vert);
				uvs.Add(new Vector2(offset.x, offset.y) / path.Radius / style.texelsPerUnit.x);
			}
		}

		List<int> triList = triLists[0]; // use base material

		int numVerts = verts.Count - startIndex;
		for (int i = 1; i < numVerts; i++)
		{
			triList.Add(startIndex + i);
			triList.Add(startIndex);
			triList.Add(startIndex + i - 1);

			if (flip)
			{
				int temp = triList[triList.Count - 1];
				triList[triList.Count - 1] = triList[triList.Count - 3];
				triList[triList.Count - 3] = temp;
			}
		}
	}

	private void CalculateNormals()
	{
		Dictionary<Vector3, List<int>> posToIndicesMap = new Dictionary<Vector3, List<int>>();
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 ds = LuckshotMath.Round(verts[i], 2);
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

// 			Vector3 normalX = normals[kvp.Value[0]];
// 			for (int i = 1; i < kvp.Value.Count; i++)
// 			{
// 				float angleDiff = Vector3.Angle(normalX, normals[kvp.Value[i]]);
// 				if (angleDiff > 70f)
// 					return;
// 			}

			averageNormal /= kvp.Value.Count;
			for (int i = 0; i < kvp.Value.Count; i++)
				normals[kvp.Value[i]] = averageNormal;
		}
	}

	public void ClearSerializedMesh()
	{
		serializedMesh = null;
	}
}
