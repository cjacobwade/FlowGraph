using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luckshot.Paths
{
	public abstract class PathBase : MonoBehaviour
	{
		public abstract Vector3 GetPoint(float alpha);
		public abstract float GetNearestAlpha(Vector3 position, int iterations = 10);
		public abstract Vector3 GetDirection(float alpha);
		
		public abstract float GetLength();

		public abstract Vector3 GetVelocity(float alpha);
		public abstract Vector3 GetNormal(float t);

		public virtual Vector2 GetScalar(float t)
		{ return Vector2.one; }

		[SerializeField, HideInInspector]
		protected bool loop = false;
		public virtual bool Loop => loop;

		public void SetLoop(bool loop)
		{ 
			this.loop = loop;
			NotifyChanged();
		}

		[SerializeField]
		private float radius = 1f;
		public float Radius => radius * transform.lossyScale.x;

		public void SetRadius(float radius)
		{ 
			this.radius = radius;
			NotifyChanged();
		}

		public void NotifyChanged()
		{ PathChanged(this); }

		public event System.Action<PathBase> PathChanged = delegate {};
	}
}
