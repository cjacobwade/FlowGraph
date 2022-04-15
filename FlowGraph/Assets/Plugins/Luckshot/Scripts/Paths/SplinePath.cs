using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Collections;

namespace Luckshot.Paths
{
	public class SplinePath : PathBase
	{
		// Conceptually:

		// Spline is a series of bezier curves each being

		// CurveStart
		// CurveGuide1
		// CurveGuide2
		// CurveEnd

		// Additional each CurveStart/End has an associated normal
		// which helps in calculating better normals along spline
		
		// With multiple curves the end of a preceding curve is the start of the next

		// Implementation:

		// CurveStarts/Ends and Guides are contained in a collection of points
		// these points are exposed for runtime adjustment

		// Some properties can check for number of curves or curve end/startpoints
		// and those need to be calculated based on points collection
		
		// CurveStart/End associated normals are stored in a separate, shorter collection

		//[SerializeField, HideInInspector]
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

		//[SerializeField, HideInInspector]
		[SerializeField]
		private List<Vector3> normals = new List<Vector3>();
		public List<Vector3> Normals
		{
			get
			{
				if (normals.Count == 0 || normals.Count != ControlCount)
				{
					if(points.Count != 0)
					{
						// try to match with existing points
						normals.Clear();

						while (normals.Count < ControlCount)
						{
							int controlIndex = normals.Count * 3;
							Vector3 control = points[controlIndex];

							int guideIndex = controlIndex + 1;
							if (guideIndex >= PointCount)
								guideIndex = controlIndex - 1;

							Vector3 control2 = points[guideIndex];
							Vector3 toControl2 = (control2 - control).normalized;

							Vector3 right = Vector3.Cross(Vector3.up, toControl2).normalized;
							Vector3 normal = Vector3.Cross(toControl2, right).normalized;
							normals.Add(normal);
						}
					}
				}
				return normals;
			}
		}

		[SerializeField]
		private List<Vector2> scalars = new List<Vector2>();
		public List<Vector2> Scalars
		{
			get
			{
				if (scalars.Count == 0 || scalars.Count != ControlCount)
				{
					if (points.Count != 0)
					{
						// try to match with existing points
						scalars.Clear();

						while (scalars.Count < ControlCount)
							scalars.Add(Vector2.one);
					}
				}
				return scalars;
			}
		}

		public int PointCount => Points.Count;

		public int ControlCount => (Points.Count / 3) + 1;

		public int CurveCount => Points.Count / 3;

		public Vector3 GetControlPointLocal(int index) => points[index];

		public Vector3 GetControlPoint(int index) => transform.TransformPoint(points[index]);

		public void SetControlPoint(int index, Vector3 point, bool moveGuides = false)
		{ SetControlPointLocal(index, transform.InverseTransformPoint(point), moveGuides); }

		public void SetControlPointLocal(int index, Vector3 point, bool moveGuides = false)
		{
			if (index % 3 == 0 && moveGuides)
			{
				Vector3 delta = point - points[index];
				if (loop)
				{
					if (index == 0)
					{
						points[1] += delta;
						points[Points.Count - 2] += delta;
						points[Points.Count - 1] = point;
					}
					else if (index == Points.Count - 1)
					{
						points[0] = point;
						points[1] += delta;
						points[index - 1] += delta;
					}
					else
					{
						points[index - 1] += delta;
						points[index + 1] += delta;
					}
				}
				else
				{
					if (index > 0)
					{
						points[index - 1] += delta;
					}
					if (index + 1 < Points.Count)
					{
						points[index + 1] += delta;
					}
				}
			}

			points[index] = point;
			EnforceAlignment(index);

			NotifyChanged();
		}

		public void CalculateNormals()
        {
			for (int i = 0; i < 3; i++)
			{
				float alpha = i / 2f;

				Vector3 direction = GetDirection(alpha);
				Vector3 normal = Vector3.Cross(direction, transform.right).normalized;

				SetControlNormal(i, normal);
			}
		}

		public Vector3 GetControlNormal(int controlIndex) => transform.TransformDirection(normals[controlIndex]);

		public Vector3 GetControlNormalLocal(int controlIndex) => normals[controlIndex];

		public void SetControlNormal(int controlIndex, Vector3 normal)
		{ SetControlNormalLocal(controlIndex, transform.InverseTransformDirection(normal)); }

		public void SetControlNormalLocal(int controlIndex, Vector3 normal)
		{
			normals[controlIndex] = normal;

			if(loop)
			{
				if(controlIndex == 0)
				{
					normals[normals.Count - 1] = normal;
				}
				else if(controlIndex == normals.Count - 1)
				{
					normals[0] = normal;
				}
			}

			NotifyChanged();
		}

		public void SetControlScalar(int controlIndex, Vector2 scalar)
        {
			scalars[controlIndex] = scalar;

			if (loop)
			{
				if (controlIndex == 0)
				{
					scalars[scalars.Count - 1] = scalar;
				}
				else if (controlIndex == scalars.Count - 1)
				{
					scalars[0] = scalar;
				}
			}

			NotifyChanged();
		}

		public void EnforceAlignment(int index)
		{
			int controlIndex = (index + 1) / 3;
			if (!loop && (controlIndex == 0 || controlIndex == ControlCount - 1))
				return;

			int middleIndex = controlIndex * 3;
			int fixedIndex, enforcedIndex;
			if (index <= middleIndex)
			{
				fixedIndex = middleIndex - 1;
				if (fixedIndex < 0)
					fixedIndex = Points.Count - 2;

				enforcedIndex = middleIndex + 1;
				if (enforcedIndex >= Points.Count)
					enforcedIndex = 1;
			}
			else
			{
				fixedIndex = middleIndex + 1;
				if (fixedIndex >= Points.Count)
					fixedIndex = 1;

				enforcedIndex = middleIndex - 1;
				if (enforcedIndex < 0)
					enforcedIndex = Points.Count - 2;
			}

			Vector3 middle = points[middleIndex];
			Vector3 enforcedTangent = middle - points[fixedIndex];

			Vector3 newPoint = middle + enforcedTangent;
			if (points[enforcedIndex] != newPoint)
			{
				points[enforcedIndex] = middle + enforcedTangent;

				float alpha = controlIndex / (float)(ControlCount - 1);
				if (controlIndex == 0 || float.IsNaN(alpha))
					alpha = 0f;

				Vector3 up = GetControlNormal(controlIndex);
				Vector3 forward = GetDirection(alpha);

				Vector3 right = Vector3.Cross(up, forward).normalized;
				up = Vector3.Cross(forward, right).normalized;

				SetControlNormal(controlIndex, up);

				NotifyChanged();
			}
		}

		public override Vector3 GetPoint(float t)
		{
			if (Points.Count < 4)
				return Vector3.zero;

			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = PointCount - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			return transform.TransformPoint(BezierUtils.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
		}

		public override float GetNearestAlpha(Vector3 point, int iterations = 10)
		{
			int nearestIter = 0;
			float nearestAlpha = 0f;
			float nearestDistance = float.MaxValue;

			// Get a general spot along the spline that our point is near
			// This is more accurate then immediately halfing
			int totalIterations = iterations * Points.Count;
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

		public Vector3 GetNearestSplinePoint(Vector3 position, int numIterations = 10)
		{ return GetPoint(GetNearestAlpha(position, numIterations)); }

		public override Vector3 GetVelocity(float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = PointCount - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return transform.TransformPoint(BezierUtils.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
		}

		public override Vector3 GetDirection(float t)
		{ return GetVelocity(t).normalized; }

		public override Vector3 GetNormal(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerCurve = 1f / (float)CurveCount;

			int nodeA = SafeNodeIndex(Mathf.FloorToInt(t / alphaPerCurve));
			int nodeB = SafeNodeIndex(nodeA + 1);

			if (!loop && nodeA == CurveCount && nodeB == 0)
			{
				nodeA = CurveCount - 1;
				nodeB = CurveCount;
			}

			float remainder = t % alphaPerCurve;
			float alpha = remainder / alphaPerCurve;
			if (t == 1f)
				alpha = 1f;
			
			Vector3 normalA = transform.TransformDirection(Normals[nodeA]);
			Vector3 normalB = transform.TransformDirection(Normals[nodeB]);

			// This normal is always perpendicular but can twist
			Vector3 forward = GetDirection(t);
			Vector3 normal = Vector3.Slerp(normalA, normalB, alpha).normalized;

			if (alpha != 0f && alpha != 1f)
			{
				Vector3 right = Vector3.Cross(normal, forward).normalized;
				normal = Vector3.Cross(-right, forward).normalized;
			}

			return normal;
		}

        public override Vector2 GetScalar(float t)
        {
			t = Mathf.Clamp01(t);

			float alphaPerCurve = 1f / (float)CurveCount;

			int nodeA = SafeNodeIndex(Mathf.FloorToInt(t / alphaPerCurve));
			int nodeB = SafeNodeIndex(nodeA + 1);

			if(!loop && nodeA == CurveCount && nodeB == 0)
			{
				nodeA = CurveCount - 1;
				nodeB = CurveCount;
			}

			float remainder = t % alphaPerCurve;
			float alpha = remainder / alphaPerCurve;
			if (t == 1f)
				alpha = 1f;

			Vector2 thicknessA = Scalars[nodeA];
			Vector2 thicknessB = Scalars[nodeB];

			Vector2 thickness = Vector2.Lerp(thicknessA, thicknessB, alpha);
			return thickness;
		}

        private int SafeNodeIndex(int index)
		{
			index = (int)Mathf.Repeat(index, ControlCount);
			return index;
		}

		public override float GetLength()
		{
			float dist = 0f;
			float alphaIter = 0.01f;

			Vector3 prevPos = GetPoint(0f);

			float alpha = 0;
			while (alpha < 1f)
			{
				alpha += alphaIter;
				alpha = Mathf.Clamp01(alpha);

				Vector3 pos = GetPoint(alpha);
				dist += (pos - prevPos).magnitude;
				prevPos = pos;
			}

			return dist;
		}

		public void AddControl()
		{
			Vector3 forward = GetDirection(1f);
			Vector3 normal = GetNormal(1f);
			Vector3 end = GetPoint(1f);

			Vector3 guide = end + forward * 0.5f;
			Vector3 guide2 = end + forward * 1.5f;
			Vector3 end2 = end + forward * 2f;

			points.Add(transform.InverseTransformPoint(guide));
			points.Add(transform.InverseTransformPoint(guide2));
			points.Add(transform.InverseTransformPoint(end2));

			normals.Add(normal);
			scalars.Add(Vector2.one);

			EnforceAlignment(PointCount - 1);
			EnforceAlignment(PointCount - 3);

			if (loop)
			{
				points[PointCount - 1] = points[0];
				EnforceAlignment(0);
			}

			NotifyChanged();
		}

		public void RemoveControl()
		{ RemoveControl(ControlCount - 1); }

		public virtual void InsertControl(int controlIndex, Vector3 point)
		{
			float alpha = controlIndex / (float)(ControlCount - 1);
			if (controlIndex == 0 || float.IsNaN(alpha))
				alpha = 1f / ControlCount / 2f;

			float alphaOffset = 1f / ControlCount * 0.3f;

			Vector3 prev = GetPoint(alpha - alphaOffset);
			Vector3 next = GetPoint(alpha + alphaOffset);
			Vector3 normal = GetNormal(alpha);

			int insertIndex = controlIndex * 3 + 2;

			points.Insert(insertIndex, transform.InverseTransformPoint(prev));
			points.Insert(insertIndex + 1, transform.InverseTransformPoint(point));
			points.Insert(insertIndex + 2, transform.InverseTransformPoint(next));

			normals.Insert(controlIndex, normal);
			scalars.Insert(controlIndex, Vector2.one);

			EnforceAlignment(insertIndex);
			EnforceAlignment(insertIndex + 2);

			if (loop)
			{
				points[PointCount - 1] = points[0];
				EnforceAlignment(0);
			}

			NotifyChanged();
		}

		public virtual void RemoveControl(int controlIndex)
		{
			if (CurveCount > 1)
			{
				int startIndex = controlIndex * 3 - 1;

				if (controlIndex == 0)
					startIndex = 0;
				else if (controlIndex == ControlCount - 1)
					startIndex -= 1;

				points.RemoveRange(startIndex, 3);
				normals.RemoveAt(controlIndex);
				scalars.RemoveAt(controlIndex);

				if(startIndex >= 0)
					EnforceAlignment(startIndex - 1);

				if (loop)
				{
					points[PointCount - 1] = points[0];
					EnforceAlignment(0);
				}
			}

			NotifyChanged();
		}

		public virtual void Reset()
		{
			points.Clear();
			points.Add(new Vector3(1f, 0f, 0f));
			points.Add(new Vector3(2f, 0f, 0f));
			points.Add(new Vector3(3f, 0f, 0f));
			points.Add(new Vector3(4f, 0f, 0f));

			normals.Clear();
			normals.Add(Vector3.up);
			normals.Add(Vector3.up);

			scalars.Clear();
			scalars.Add(Vector2.one);
			scalars.Add(Vector2.one);

            NotifyChanged();
		}
	}
}