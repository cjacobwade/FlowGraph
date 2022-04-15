using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshProcessData_DisplacementTex : MeshProcessData
{
	[SerializeField]
	private Texture2D displaceTex = null;

	[SerializeField]
	private float strength = 1f;

	public override Mesh ProcessMesh(Mesh mesh)
	{
		// Reason to instantiate is because this might be processing a mesh asset 
		Mesh displacedMesh = MonoBehaviour.Instantiate(mesh);
		displacedMesh.name = displacedMesh.name.Replace("(Clone)", "");

		if (displaceTex != null)
		{
			List<Vector3> verts = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<Color> colors = new List<Color>();

			displacedMesh.GetVertices(verts);
			displacedMesh.GetNormals(normals);
			displacedMesh.GetUVs(0, uvs);
			displacedMesh.GetColors(colors);

			if(colors.Count == 0)
			{
				Color baseColor = Color.white;
				baseColor.a = 0f;

				while (colors.Count < verts.Count)
					colors.Add(baseColor);
			}	

#if UNITY_EDITOR
			if (!displaceTex.isReadable)
			{
				string path = AssetDatabase.GetAssetPath(displaceTex);

				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
				importer.isReadable = true;

				EditorUtility.SetDirty(importer);
				importer.SaveAndReimport();
			}
#endif

			for (int i = 0; i < verts.Count; i++)
			{
				Vector2 uv = uvs[i];
				Vector2Int pixel = new Vector2Int(
					(int)(Mathf.Repeat(uv.x, 1f) * displaceTex.width),
					(int)(Mathf.Repeat(uv.y, 1f) * displaceTex.height));

				Color color = displaceTex.GetPixel(pixel.x, pixel.y);
				verts[i] += normals[i] * color.r * colors[i].a * strength;
			}

			displacedMesh.SetVertices(verts);
 			displacedMesh.RecalculateNormals();
 			displacedMesh.RecalculateTangents();
		}

		return displacedMesh;
	}
}
