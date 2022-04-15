using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace Luckshot.Paths
{
	public class LinePath : PathBase
	{
		[SerializeField]
		private List<Vector3> points = new List<Vector3>();
		public List<Vector3> Points
		{
			get
			{
				if (points.Count == 0) Reset();
				return points;
			}
		}

		public int PointCount => Points.Count;

		public int LineCount => loop ? PointCount : (PointCount - 1);

		private bool clockwise = false;
		public bool Clockwise => clockwise;

		public Vector3 GetControlPointLocal(int index) => points[index];

		public Vector3 GetControlPoint(int index) => transform.TransformPoint(points[index]);

		public void SetControlPoint(int index, Vector3 point, bool moveGuides = false)
		{ SetControlPointLocal(index, transform.InverseTransformPoint(point), moveGuides); }

		public void SetControlPointLocal(int index, Vector3 point, bool moveGuides = false)
		{
			points[index] = point;
			NotifyChanged();
		}

		private void Awake()
		{
			RecalculateClockwise();
		}

		private void RecalculateClockwise()
		{
			float sum = 0f;
			for (int i = 0; i < PointCount; i++)
			{
				Vector3 a = points[i];
				Vector3 b = points[(int)Mathf.Repeat(i - 1, PointCount)];

				sum += (b.x - a.x) * (b.z + a.z);
			}

			clockwise = sum > 0;
		}

		public override Vector3 GetNormal(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerControl = 1f / (float)LineCount;

			int index = Mathf.FloorToInt(t / alphaPerControl);
			index = SafePointIndex(index);

			int nextIndex = SafePointIndex(index + 1);

			Vector3 controlPos = GetControlPoint(index);
			Vector3 nextControlPos = GetControlPoint(nextIndex);

			Vector3 forward = nextControlPos - controlPos;
			Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
			Vector3 normal = Vector3.Cross(forward, right).normalized;

			return normal;
		}

		public Vector3 GetPoint(int index) => transform.TransformPoint(Points[index]);

		public override Vector3 GetPoint(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerPoint = 1f / (float)LineCount;

			int index = Mathf.FloorToInt(t / alphaPerPoint);
			index = SafePointIndex(index);

			int nextIndex = index + 1;
			nextIndex = SafePointIndex(nextIndex);

			float remainder = t % alphaPerPoint;

			Vector3 localPos = Vector3.Lerp(points[index], points[nextIndex], remainder / alphaPerPoint);
			return transform.TransformPoint(localPos);
		}

		public int SafePointIndex(int index)
		{
			if (loop)
				index = (int)Mathf.Repeat(index, PointCount);
			else
				index = Mathf.Clamp(index, 0, PointCount - 1);

			return index;
		}

		public override float GetNearestAlpha(Vector3 point, int iterations = 10)
		{
			int nearestIter = 0;
			float nearestAlpha = 0f;
			float nearestDistance = float.MaxValue;

			// Get a general spot along the spline that our point is near
			// This is more accurate then immediately halfing
			int totalIterations = iterations * PointCount;
			for (int i = 0; i < totalIterations; i++)
			{
				float iterAlpha = i / (float)totalIterations;

				Vector3 iterPos = GetPoint(iterAlpha);
				float iterDistance = Vector3.Distance(point, iterPos);

				if (iterDistance < nearestDistance)
				{
					nearestIter = i;
					nearestAlpha = iterAlpha;
					nearestDistance = iterDistance;
				}
			}

			// Within a range around closest large iteration,
			// keep halving range till we have a good approximation
			float minIterAlpha = Mathf.Max(0, nearestIter - 1) / (float)totalIterations;
			float maxIterAlpha = Mathf.Min(totalIterations, nearestIter + 1) / (float)totalIterations;
			for (int i = 0; i < totalIterations; i++)
			{
				float iterAlpha = Mathf.Lerp(minIterAlpha, maxIterAlpha, i / (float)totalIterations);

				Vector3 iterPos = GetPoint(iterAlpha);
				float iterDistance = Vector3.Distance(point, iterPos);

				if (iterDistance < nearestDistance)
				{
					nearestAlpha = iterAlpha;
					nearestDistance = iterDistance;
				}
			}

			return nearestAlpha;
		}

		public Vector3 GetNearestPathPoint(Vector3 position, int numIterations = 10)
		{ return GetPoint(GetNearestAlpha(position, numIterations)); }

		public override Vector3 GetVelocity(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerPoint = 1f / (float)LineCount;

			int index = Mathf.FloorToInt(t / alphaPerPoint);
			index = Mathf.Clamp(index, 0, PointCount - 1);

			int nextIndex = SafePointIndex(index + 1);
			if (index == nextIndex)
				index -= 1;

			return transform.TransformVector(points[nextIndex] - points[index]);
		}

		public override Vector3 GetDirection(float alpha)
		{ return GetVelocity(alpha).normalized; }

		public override float GetLength()
		{
			float dist = 0f;

			Vector3 prevPos = transform.TransformPoint(Points[0]);
			for (int i = 1; i < PointCount; i++)
			{
				Vector3 pos = transform.TransformPoint(Points[i]);
				dist += (pos - prevPos).magnitude;
				prevPos = pos;
			}

			return dist;
		}

		public void AddControl()
		{
			Vector3 forward = GetDirection(1f);
			Vector3 end = GetPoint(1f);
			
			points.Add(transform.InverseTransformPoint(end + forward * 3f));

			if (loop)
				Points[PointCount - 1] = Points[0];

			NotifyChanged();
		}

		public void RemoveControl()
		{
			points.RemoveAt(PointCount - 1);

			if (loop)
				Points[PointCount - 1] = Points[0];

			NotifyChanged();
		}

		public void InsertControl(int index, Vector3 pos)
		{
			points.Insert(index, transform.InverseTransformPoint(pos));

			if (loop)
				points[PointCount - 1] = points[0];

			NotifyChanged();
		}

		public void RemoveControl(int index)
		{
			points.RemoveAt(index);
			if (loop)
				Points[PointCount - 1] = Points[0];

			NotifyChanged();
		}

		public void Reset()
		{
			points.Clear();
			points.Add(new Vector3(1f, 0f, 0f));
			points.Add(new Vector3(2f, 0f, 0f));

			NotifyChanged();
		}
	}
}