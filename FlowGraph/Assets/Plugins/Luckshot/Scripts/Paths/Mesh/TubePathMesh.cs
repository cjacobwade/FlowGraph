using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Paths;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TubePathMesh : MonoBehaviour, ICustomMesh
{
	public event System.Action<ICustomMesh> OnMeshChanged = delegate { };

	[SerializeField, AutoCache]
	protected PathBase path = null;
	public PathBase Path => path;

	[SerializeField]
	protected Mesh serializedMesh = null;

	protected MeshFilter meshFilter = null;
	public MeshFilter MeshFilter
	{
		get
		{
			if (meshFilter == null)
				meshFilter = GetComponent<MeshFilter>();

			return meshFilter;
		}
	}

	protected MeshCollider meshCollider = null;
	public MeshCollider MeshCollider
	{
		get
		{
			if (meshCollider == null)
				meshCollider = GetComponent<MeshCollider>();

			return meshCollider;
		}
	}

	[SerializeField, Range(0.1f, 3f)]
	protected float spacing = 0.3f;

	[SerializeField, Range(0f, 1f)]
	protected float fillAmount = 1f;
	public float FillAmount => fillAmount;

	public void SetFillAmount(float inFillAmount)
	{
		if (fillAmount != inFillAmount)
		{
			fillAmount = inFillAmount;
			Regenerate();
		}
	}

	[SerializeField, Range(3, 30), OnValueChanged("BuildMesh")]
	protected int subdivisions = 8;

	[SerializeField]
	protected bool variableWidth = false;

	[SerializeField, ShowIf("variableWidth")]
	protected AnimationCurve widthCurve = new AnimationCurve();

	[SerializeField, OnValueChanged("BuildMesh")]
	protected bool colorVerts = false;

	[SerializeField, ShowIf("colorVerts")]
	protected Gradient colorGradient = new Gradient();

	protected float alphaIter = 0.01f;

	protected List<Vector3> verts = new List<Vector3>();
	protected List<int> indices = new List<int>();
	protected List<Vector3> normals = new List<Vector3>();
	protected List<Vector2> uvs = new List<Vector2>();
	protected List<Color> colors = new List<Color>();

	protected float pathLength = 0f;

	private bool HasLinePath()
	{ return path != null && path is LinePath; }

	[SerializeField, ShowIf("HasLinePath")]
	private float lineCornerFalloff = 0.05f;

	protected List<Vector3> pointDirs = new List<Vector3>();
	protected List<float> pointIters = new List<float>();

	protected List<float> dists = new List<float>();
	protected List<float> iters = new List<float>();

	private void Awake()
	{
		if (path == null)
			path = GetComponent<PathBase>();
	}

	public void ClearMesh()
	{
		if(serializedMesh != null)
		{
			if (Application.IsPlaying(this))
				Destroy(serializedMesh);
			else
				DestroyImmediate(serializedMesh);
		}
	}

	[Button("Regenerate")]
	public void Regenerate()
	{
		if (this == null)
			return;

		if (MeshFilter != null && path != null)
		{
			if (serializedMesh == null)
			{
				serializedMesh = new Mesh();
				serializedMesh.name = "PathMesh";
				serializedMesh.hideFlags = HideFlags.None;
				serializedMesh.MarkDynamic();
			}
			else
				serializedMesh.Clear();

			verts.Clear();
			indices.Clear();
			normals.Clear();
			uvs.Clear();
			colors.Clear();

			Vector3 lastPos = path.GetPoint(0f);

			Vector3 normal = -path.GetDirection(0f);
			verts.Add(transform.InverseTransformPoint(lastPos + normal * path.Radius / 2f));
			normals.Add(transform.InverseTransformVector(normal));
			uvs.Add(new Vector2(0f, 0f));

			if (colorVerts)
				colors.Add(colorGradient.Evaluate(0f));

			pathLength = path.GetLength() * fillAmount;

			dists.Clear();
			iters.Clear();

			iters.Add(0f);
			dists.Add(0f);

			float totalDist = 0f;

			float iter = 0f;
			while (iter < fillAmount)
			{
				float moveDist = spacing;
				while (moveDist > 0f && iter < fillAmount)
				{
					float prevIter = iter;
					iter += alphaIter;
					iter = Mathf.Clamp(iter, 0f, fillAmount);

					Vector3 pos = path.GetPoint(iter);
					Vector3 toPos = pos - lastPos;
					float toPosDist = toPos.magnitude;

					if (toPosDist < moveDist)
					{
						moveDist -= toPosDist;
						totalDist += toPosDist;

						lastPos = pos;
					}
					else
					{

						float alpha = moveDist / toPosDist;
						iter = Mathf.Lerp(prevIter, iter, alpha);
						lastPos = Vector3.Lerp(lastPos, pos, alpha);

						iters.Add(iter);

						totalDist += moveDist;
						dists.Add(totalDist);
						 
						moveDist = 0f;
					}
				}
			}

			iters.Add(fillAmount);
			dists.Add(pathLength);

			LinePath linePath = path as LinePath;
			if (linePath != null)
			{
				// THIS IS A BAD ATTEMPT AT MAKING LINEPATHS WORK. NOT OFFICIALLY SUPPORTED
				float alphaPerLine = fillAmount / (float)linePath.LineCount;

				pointDirs.Clear();
				pointIters.Clear();
				for (int i = 1; i < linePath.LineCount; i++)
				{
					float alpha = alphaPerLine * i;
					Vector3 preDir = linePath.GetDirection(alpha - PathUtils.DefaultTraverseAlphaSpeed);
					Vector3 postDir = linePath.GetDirection(alpha + PathUtils.DefaultTraverseAlphaSpeed);

					Vector3 dir = (preDir + postDir) / 2f;
					pointDirs.Add(dir);
					pointIters.Add(alpha);

					for (int j = 0; j < iters.Count; j++)
					{
						if (iters[j] > alpha)
						{
							iters.Insert(j, alpha);
							dists.Insert(j, alpha); // Cursed
							break;
						}
					}
				}

				for (int i = 0; i < iters.Count; i++)
				{
					Vector3 direction = path.GetDirection(iters[i]);

					float nearestDist = Mathf.Infinity;
					Vector3 nearestDir = direction;
					for (int j = 0; j < pointIters.Count; j++)
					{
						float iterDist = Mathf.Abs(pointIters[j] - iters[i]);
						if (iterDist < nearestDist && iterDist < lineCornerFalloff)
						{
							float alpha = Mathf.InverseLerp(lineCornerFalloff, 0f, iterDist);
							nearestDir = Vector3.Lerp(direction, pointDirs[j], alpha);
							nearestDist = iterDist;
						}
					}

					AddEdgeLoop(iters[i], i, nearestDir);
				}
			}
			else
			{
				for (int i = 0; i < iters.Count; i++)
					AddEdgeLoop(iters[i], i);


			}

			normal = path.GetDirection(fillAmount);
			verts.Add(transform.InverseTransformPoint(lastPos + normal * path.Radius / 2f));
			uvs.Add(new Vector2(1, pathLength));
			normals.Add(transform.InverseTransformVector(normal));

			if (colorVerts)
				colors.Add(colorGradient.Evaluate(1f));

			serializedMesh.SetVertices(verts);
			serializedMesh.SetTriangles(indices, 0);
			serializedMesh.SetNormals(normals);
			serializedMesh.SetUVs(0, uvs);

			if (colorVerts)
				serializedMesh.SetColors(colors);

			MeshFilter.sharedMesh = serializedMesh;
			
			if(meshCollider != null)
				meshCollider.sharedMesh = serializedMesh;

			OnMeshChanged(this);
		}
	}

	protected void AddEdgeLoop(float iter, int numIter, Vector3? forwardOverride = null)
	{
		Vector3 point = path.GetPoint(iter);

		Vector3 forward = path.GetDirection(iter);
		if (forwardOverride.HasValue)
			forward = forwardOverride.Value;

		Vector3 up = path.GetNormal(iter);
		Vector3 right = Vector3.Cross(forward, up).normalized;
		up = Vector3.Cross(right, forward).normalized;

		float radius = path.Radius;
		float dist = dists[numIter];

		float yAlpha = dist / pathLength;
		if (float.IsNaN(yAlpha))
			yAlpha = 0f;

		Color color = Color.black;
		if (colorVerts)
			color = colorGradient.Evaluate(yAlpha);

		float widthMod = 1f;
		if (variableWidth)
			widthMod = widthCurve.Evaluate(yAlpha);

		int startNum = (subdivisions + 1) * numIter;
		for (int i = startNum; i <= startNum + subdivisions; i++)
		{
			float xAlpha = (i - startNum) / (float)subdivisions;

			Vector3 offset = right * Mathf.Cos(xAlpha * LuckshotMath.TAU) + up * Mathf.Sin(xAlpha * LuckshotMath.TAU);
			Vector3 vert = point + offset * radius * widthMod;

			verts.Add(transform.InverseTransformPoint(vert));
			normals.Add(transform.InverseTransformVector(offset));
			uvs.Add(new Vector2(xAlpha, dist));

			if (colorVerts)
				colors.Add(color);

			if (numIter == 0)
			{
				if (i > startNum)
				{
					indices.Add(i);
					indices.Add(i + 1);
					indices.Add(0);
				}
			}
			else
			{
				if (iter == fillAmount)
				{
					if (i > startNum)
					{
						indices.Add(startNum + (subdivisions + 1) + 1);
						indices.Add(i + 1);
						indices.Add(i);
					}
				}

				if (i > startNum)
				{
					indices.Add(i - (subdivisions + 1));
					indices.Add(i);
					indices.Add(i + 1);

					indices.Add(i - (subdivisions + 1));
					indices.Add(i + 1);
					indices.Add(i + 1 - (subdivisions + 1));
				}
			}
		}
	}
}
