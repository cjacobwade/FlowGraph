using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luckshot.Paths
{

	public class PathDebug : MonoBehaviour
	{
		[SerializeField, AutoCache]
		private PathBase path = null;

		[SerializeField, Range(0f, 1f)]
		private float alpha = 0f;

		private void OnDrawGizmosSelected()
		{
			if (path == null)
				return;

			if (path.Loop)
				alpha = Mathf.Repeat(alpha, 1f);
			else
				alpha = Mathf.Clamp01(alpha);

			Vector3 pos = path.GetPoint(alpha);
			Vector3 normal = path.GetNormal(alpha);
			Vector3 forward = path.GetDirection(alpha);

			Gizmos.DrawSphere(pos, 0.1f);

			Debug.DrawLine(pos, pos + normal, Color.red);
			Debug.DrawLine(pos, pos + forward, Color.blue);
		}
	}
}