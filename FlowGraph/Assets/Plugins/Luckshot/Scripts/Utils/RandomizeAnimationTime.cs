using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAnimationTime : MonoBehaviour
{
	[SerializeField, AutoCache]
	private new Animation animation = null; 

	private void Awake()
	{
		animation.SetNormalizedTime(Random.value);
	}
}
