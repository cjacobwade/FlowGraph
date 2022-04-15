using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luckshot.Paths;
using NaughtyAttributes;

public class PathMover : MonoBehaviour
{
	[SerializeField]
	private PathBase path = null;

	[SerializeField]
	private new Rigidbody rigidbody = null;

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

	[SerializeField]
	private bool changeRotation = false;

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

		if (changeRotation)
		{
			Vector3 dir = path.GetDirection(lineAlpha);
			Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
			rigidbody.MoveRotation(rot);
		}
	}
}
