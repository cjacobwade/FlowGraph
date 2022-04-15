using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CollisionSFXPair
{
	public PhysicMaterial matA;
	public PhysicMaterial matB;
	//[FMODUnity.EventRef]
	public string audioEvent;
}

[CreateAssetMenu(fileName = "CollisionSFXMatrix", menuName = "Luckshot/CollisionSFXMatrix")]
public class CollisionSFXMatrix : ScriptableObject
{
	[SerializeField]
	private CollisionSFXPair fallback = new CollisionSFXPair();

	[SerializeField]
	private CollisionSFXPair[] collisionSFXPairs = null;

	public CollisionSFXPair FindMatchingCollisionPair(PhysicMaterial a, PhysicMaterial b)
	{
		if (a == null && b == null)
			return fallback;

		CollisionSFXPair? pairing = default;

		for(int i =0; i < collisionSFXPairs.Length; i++)
		{
			CollisionSFXPair pair = collisionSFXPairs[i];

			if ((a == pair.matA && b == pair.matB) ||
				(b == pair.matA && a == pair.matB))
			{
				return collisionSFXPairs[i];
			}

			if(	(a != null && (a == pair.matA || a == pair.matB)) ||
				(b != null && (b == pair.matA || b == pair.matB)))
			{
				pairing = collisionSFXPairs[i];
			}
		}

		return pairing.HasValue ? pairing.Value : fallback;
	}
}
