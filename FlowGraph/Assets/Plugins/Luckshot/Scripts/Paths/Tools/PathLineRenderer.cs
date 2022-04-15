using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Paths;
using NaughtyAttributes;

[RequireComponent(typeof(LineRenderer))]
public class PathLineRenderer : MonoBehaviour
{
	[SerializeField, AutoCache]
	private PathBase path = null;

	[SerializeField, AutoCache]
	private LineRenderer line = null;

	[SerializeField]
	private float pointsPerUnit = 5f;

	[SerializeField]
	private Vector2 texScale = Vector2.one;

	[SerializeField]
	private float texScrollSpeed = 3f;

	private MaterialPropertyBlock propertyBlock = null;
	private readonly int texParamsID = Shader.PropertyToID("_MainTex_ST");

	private void Awake()
	{
		if (path == null)
		{
			Debug.LogError("No PathBase found. Trying to find one now.", this);
			path = GetComponent<PathBase>();
		}

		path.PathChanged -= Path_OnPathChanged;
	}

	private void LateUpdate()
	{
		UpdateTexture();
	}

	public void Regenerate()
	{
		UpdateLine();
		UpdateTexture();
	}

	private void Path_OnPathChanged(PathBase inPath)
	{
		if (path == inPath)
			UpdateLine();
		else
			inPath.PathChanged -= Path_OnPathChanged;
	}

	private void UpdateLine()
	{
		if (path != null && line != null)
		{
			int numPoints = Mathf.CeilToInt(path.GetLength() * pointsPerUnit);

			Vector3[] points = new Vector3[numPoints];
			for (int i = 0; i < numPoints; i++)
				points[i] = path.GetPoint(i/(float)(numPoints - 1));

			line.positionCount = numPoints;
			line.SetPositions(points);
		}
	}

	private void UpdateTexture()
	{
		if (propertyBlock == null)
			propertyBlock = new MaterialPropertyBlock();

		line.GetPropertyBlock(propertyBlock);
		Vector4 texParams = propertyBlock.GetVector(texParamsID);
		texParams.x = texScale.x * path.GetLength();
		texParams.y = texScale.y;
		texParams.z += texScrollSpeed * Time.deltaTime;
		propertyBlock.SetVector(texParamsID, texParams);
		line.SetPropertyBlock(propertyBlock);
	}

	private void OnDrawGizmos()
	{
		if (Application.IsPlaying(this))
			return;

		if (path != null)
		{
			path.PathChanged -= Path_OnPathChanged;
			path.PathChanged += Path_OnPathChanged;
		}

		UpdateLine();
		UpdateTexture();
	}

}
