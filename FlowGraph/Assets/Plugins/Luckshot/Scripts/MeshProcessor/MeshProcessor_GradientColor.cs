using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProcessor_GradientColor : MeshProcessor
{
	[SerializeField]
	private Color topColor = Color.white;

	[SerializeField]
	private Color bottomColor = Color.black;

	[SerializeField]
	private float gradientScale = 1f;

	[SerializeField]
	private float gradientOffset = 0f;

	public override Mesh ProcessMesh(Mesh mesh)
	{
		Mesh uvMesh = MonoBehaviour.Instantiate(mesh);

		List<Vector3> verts = new List<Vector3>();
		List<Color> colors = new List<Color>();

		uvMesh.GetVertices(verts);
		uvMesh.GetColors(colors);

		if (colors.Count == 0)
		{
			Color baseColor = Color.white;
			baseColor.a = 1f;

			while (colors.Count < verts.Count)
				colors.Add(baseColor);
		}

		float maxY = mesh.bounds.max.y;
		float minY = mesh.bounds.min.y;

		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vert = verts[i];
			float alpha = Mathf.InverseLerp(minY, maxY, vert.y);

			Color multiplyColor = Color.Lerp(bottomColor, topColor, alpha * gradientScale - gradientOffset);
			colors[i] *= multiplyColor;
		}

		uvMesh.SetVertices(verts);
		uvMesh.SetColors(colors);

		return uvMesh;
	}
}
