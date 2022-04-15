using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Paths;
using NaughtyAttributes;

public class PathRoller : MonoBehaviour
{
	[SerializeField]
	private PathBase path = null;

	[SerializeField]
	private new Rigidbody rigidbody = null;

	[SerializeField]
	private Collider radiusCollider = null;

	[SerializeField]
	private float speed = 1f;
	public void SetSpeed(float inSpeed)
	{ 
		speed = inSpeed;
		reversed = false;
	}

	private float lineAlpha = 0f;
	public float LineAlpha => lineAlpha;

	public void SetLineAlpha(float lineAlpha)
	{
		this.lineAlpha = lineAlpha;
		ApplyLineAlpha(lineAlpha);
	}

	private bool reversed = false;

	[SerializeField]
	private bool pauseAtEnds = false;

	private void Awake()
	{
		lineAlpha = path.GetNearestAlpha(rigidbody.position);
		ApplyLineAlpha(lineAlpha);
	}

	private void FixedUpdate()
	{
		if (rigidbody == null || path == null)
			return;

		if (speed != 0f)
		{
			bool wasReversed = reversed;
			lineAlpha = path.MoveAtFixedSpeed(lineAlpha, speed * Time.deltaTime, ref reversed);

			if (pauseAtEnds && wasReversed != reversed)
				speed = 0f;

			ApplyLineAlpha(lineAlpha);
		}
	}

	private void ApplyLineAlpha(float lineAlpha)
    {
		this.lineAlpha = lineAlpha;

		Vector3 pos = path.GetPoint(lineAlpha);
		rigidbody.MovePosition(pos);


		Vector3 dir = path.GetDirection(lineAlpha);
		Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

		float radius = 0f;
		SphereCollider sphereCol = radiusCollider as SphereCollider;
		if (sphereCol != null)
		{ 
			radius = sphereCol.radius;
		}
		else
		{
			CapsuleCollider capsuleCol = radiusCollider as CapsuleCollider;
			if (capsuleCol != null)
				radius = capsuleCol.radius;
		}


		float circumference = 2f * Mathf.PI * radius;
		float rotationsPerLoop = Mathf.Round(path.GetLength()/circumference);

		Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
		rot = Quaternion.AngleAxis(rotationsPerLoop * lineAlpha * 360f, right) * rot;
		rigidbody.MoveRotation(rot);
	}
}
