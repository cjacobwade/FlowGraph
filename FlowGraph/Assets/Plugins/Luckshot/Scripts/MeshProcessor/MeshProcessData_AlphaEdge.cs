using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProcessData_AlphaEdge : MeshProcessData
{
	public override Mesh ProcessMesh(Mesh mesh)
	{
		Mesh uvMesh = MonoBehaviour.Instantiate(mesh);

		List<Vector3> verts = new List<Vector3>();
		List<Color> colors = new List<Color>();

		uvMesh.GetVertices(verts);

		HashSet<int> matchedVerts = new HashSet<int>();

		for (int i = 0; i < verts.Count; i++)
		{
			if (matchedVerts.Contains(i))
				continue;

			int numMatches = 0;
			if (i == 0)
				numMatches--;

			for (int j = i + 1; j < verts.Count; j++)
			{
				if (matchedVerts.Contains(j))
					continue;

				if ((verts[i] - verts[j]).magnitude < Mathf.Epsilon)
				{
					numMatches++;
					matchedVerts.Add(j);
				}
			}

			if(numMatches > 0)
				matchedVerts.Add(i);
		}

		for(int i =0; i < verts.Count; i++)
		{
			bool contains = matchedVerts.Contains(i);
			colors.Add(contains ? Color.white : new Color(1f, 1f, 1f, 0f));
		}

		uvMesh.SetVertices(verts);
		uvMesh.SetColors(colors);

		return uvMesh;
	}
}
