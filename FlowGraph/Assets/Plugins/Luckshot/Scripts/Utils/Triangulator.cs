using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Triangulator
{
	private List<Vector3> points = new List<Vector3>();

	public Triangulator(Vector3[] inPoints)
	{ points = new List<Vector3>(inPoints); }

	public int[] Triangulate()
	{
		List<int> indices = new List<int>();

		int n = points.Count;
		if (n < 3)
		{
			return indices.ToArray();
		}

		int[] V = new int[n];
		for (int v = 0; v < n; v++)
		{
			V[v] = v;
		}

		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2;)
		{
			if ((count--) <= 0)
			{
				return indices.ToArray();
			}

			int u = v;
			if (nv <= u)
			{
				u = 0;
			}

			v = u + 1;
			if (nv <= v)
			{
				v = 0;
			}

			int w = v + 1;
			if (nv <= w)
			{
				w = 0;
			}

			if (Snip(u, v, w, nv, V))
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];

				indices.Add(a);
				indices.Add(b);
				indices.Add(c);

				m++;

				for (s = v, t = v + 1; t < nv; s++, t++)
				{
					V[s] = V[t];
				}

				nv--;
				count = 2 * nv;
			}
		}

		indices.Reverse();
		return indices.ToArray();
	}

	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector3 a = points[V[u]];
		Vector3 b = points[V[v]];
		Vector3 c = points[V[w]];

		Vector3 aToB = b - a;
		Vector3 aToC = c - a;

		if (Mathf.Epsilon > Vector3.Dot(aToB, aToC))
		{
			return false;
		}

		for (int i = 0; i < n; i++)
		{
			if ((i == u) || (i == v) || (i == w))
			{
				continue;
			}

			Vector3 p = points[V[i]];

			if (InsideTriangle(a, b, c, p))
			{
				return false;
			}
		}

		return true;
	}

	private bool InsideTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector3 aToB = b - a;
		Vector3 bToC = c - b;
		Vector3 cToA = a - c;

		Vector3 aToP = p - a;
		Vector3 bToP = p - b;
		Vector3 cToP = p - c;

		return	Vector3.Dot(bToC, bToP) >= 0f &&
				Vector3.Dot(aToB, aToP) >= 0f &&
				Vector3.Dot(cToA, cToP) >= 0f;
	}
}