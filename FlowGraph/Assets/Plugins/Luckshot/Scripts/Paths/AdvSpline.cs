using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Luckshot.Paths
{
	public abstract class AdvSpline<T> : SplinePath where T : SplineNodeData, new()
	{
		[SerializeField]
		private List<T> nodes = new List<T>();
		protected List<T> Nodes
		{
			get
			{
				if (nodes.Count == 0) Reset();
				return nodes;
			}
		}

		public int NodeCount => nodes.Count;

		public T GetNodeData(int index)
		{ return nodes[index]; }

		public void SetNodeData(int index, T nodeData)
		{ nodes[index] = nodeData; }

		public void InterpNodeData(T inNodeData, float a)
		{
			a = Mathf.Clamp01(a);

			a *= (NodeCount - 1);
			int prev = Mathf.FloorToInt(a);
			int next = Mathf.CeilToInt(a);

			if (prev < NodeCount - 1)
				a -= (float)prev;

			inNodeData.Lerp(Nodes[prev], Nodes[next], Mathf.Clamp01(a));
		}

		public override void InsertControl(int curveIndex, Vector3 point)
		{
			base.InsertControl(curveIndex, point);

			nodes.Insert(curveIndex, new T());

			// NOTE: This is going to cause weirdness
			// need to come up with some deep copy solution instead
			if (loop)
				nodes[NodeCount - 1] = nodes[0];
		}

		public override void RemoveControl(int curveIndex)
		{
			if (CurveCount > 1)
			{
				nodes.RemoveAt(curveIndex);

				// NOTE: This is going to cause weirdness
				// need to come up with some deep copy solution instead
				if (loop)
					nodes[NodeCount - 1] = nodes[0];
			}
		}

		public override void Reset()
		{
			base.Reset();

			nodes.Clear();
			nodes.Add(new T());
			nodes.Add(new T());
		}
	}
}