using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Luckshot.Paths
{
	public class PathAnchor : MonoBehaviour
	{
		public class AnchorInfo
		{
			public Transform transform;
			public float alpha;
			public Vector2 posOffset;
			public Quaternion rotOffset;
		}

		[SerializeField]
		private List<AnchorInfo> anchorInfos = new List<AnchorInfo>();

		private PathBase path = null;

		private void CachePathIfNeeded()
		{
			if (path == null)
				path = GetComponentInParent<PathBase>();

			if (path != null)
			{
				path.PathChanged -= Path_PathChanged;
				path.PathChanged += Path_PathChanged;
			}
		}

		private void Path_PathChanged(PathBase path)
		{
			ApplyAnchors();
		}

		private void CacheAnchors()
		{
			anchorInfos.Clear();

			if (path == null)
				return;

			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);

				AnchorInfo anchorInfo = new AnchorInfo();
				
				float alpha = path.GetNearestAlpha(child.position);

				Vector3 pos = path.GetPoint(alpha);
				Vector3 direction = path.GetDirection(alpha);
				Vector3 normal = path.GetNormal(alpha);
				Vector3 right = Vector3.Cross(normal, direction).normalized;
				Vector2 scalar = path.GetScalar(alpha);
				Quaternion pathRot = Quaternion.LookRotation(direction, normal);

				Vector3 toChild = child.position - pos;

				float dotRight = Vector3.Dot(toChild, right);
				float dotUp = Vector3.Dot(toChild, normal);

				anchorInfo.transform = child;
				anchorInfo.alpha = alpha;
				anchorInfo.posOffset = new Vector2(dotRight / scalar.x, dotUp / scalar.y);
				anchorInfo.rotOffset = Quaternion.Inverse(pathRot) * child.rotation;

				anchorInfos.Add(anchorInfo);
			}
		}

		private void ApplyAnchors()
		{
			if (path == null)
				return;

			foreach(var anchorInfo in anchorInfos)
			{
				Transform child = anchorInfo.transform;
				if (child == null)
					continue;

				Vector3 pos = path.GetPoint(anchorInfo.alpha);
				Vector3 direction = path.GetDirection(anchorInfo.alpha);
				Vector3 normal = path.GetNormal(anchorInfo.alpha);
				Vector3 right = Vector3.Cross(normal, direction).normalized;
				Vector2 scalar = path.GetScalar(anchorInfo.alpha);
				Quaternion pathRot = Quaternion.LookRotation(direction, normal);

				Vector2 posOffset = anchorInfo.posOffset;
				child.position = pos + right * posOffset.x * scalar.x + normal * posOffset.y * scalar.y;
				child.rotation = pathRot * anchorInfo.rotOffset;
			}
		}

		private void OnDrawGizmos()
		{
			if (Application.IsPlaying(this))
				return;

			if (!gameObject.scene.IsValid())
				return;

			CachePathIfNeeded();

#if UNITY_EDITOR
			if (Selection.gameObjects != null)
			{
				for (int i = 0; i < Selection.gameObjects.Length; i++)
				{
					if (Selection.gameObjects[i].transform == path.transform ||
						Selection.gameObjects[i].transform.IsChildOf(path.transform))
					{
						CacheAnchors();
						break;
					}
				}
			}
#endif

		}
	}
}