using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollisionSFX : MonoBehaviour
{
	private static CollisionSFXMatrix collisionSFXMatrix = null;
	public static CollisionSFXMatrix CollisionSFXMatrix
	{
		get
		{
			if (collisionSFXMatrix == null)
			{
				collisionSFXMatrix = Resources.Load<CollisionSFXMatrix>("Data/CollisionSFXMatrix");

				if (collisionSFXMatrix == null)
					Debug.LogError("No Collision SFX Matrix Found");
			}

			return collisionSFXMatrix;
		}
	}

	private float cooldown = 0.2f;
	private float lastHitTime = -Mathf.Infinity;

	private float volume = 0.5f;
	private FloatRange pitchRange = new FloatRange(0.9f, 1.1f);

	private float maxHitForce = 20f;
	private float minHitVolume = 0.05f;

	private float maxCameraDist = 50f;

	[SerializeField]
	private bool disableCollisionSFX = false;

	private void Awake()
	{
		if (disableCollisionSFX)
			Destroy(this);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Time.time < lastHitTime + cooldown)
			return;

		float hitForce = collision.relativeVelocity.magnitude;
		float appliedVolume = volume * Mathf.Clamp01(hitForce / maxHitForce);
		if (appliedVolume < minHitVolume)
			return;

		ContactPoint contact = collision.GetContact(0);

		float cameraSqrDist = (Camera.main.transform.position - contact.point).sqrMagnitude;
		if (cameraSqrDist > maxCameraDist * maxCameraDist)
			return;

		PhysicMaterial myMat = contact.thisCollider.sharedMaterial;
		PhysicMaterial otherMat = contact.otherCollider.sharedMaterial;

		CollisionSFXPair collisionSFXPair = CollisionSFXMatrix.FindMatchingCollisionPair(myMat, otherMat);
		if (collisionSFXPair.clips.Length > 0)
		{
			AudioClip clip = collisionSFXPair.clips.Random();
			if (clip != null)
			{
				AudioManager.Instance.PlaySound(clip, contact.point, appliedVolume, pitchRange.Random);
			}
		}

		lastHitTime = Time.time;
	}
}
