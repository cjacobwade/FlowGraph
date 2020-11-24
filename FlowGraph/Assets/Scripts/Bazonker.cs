using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bazonker : MonoBehaviour
{
	[GlobalEvent]
	public static event System.Action OnBazonked = delegate {};
	[GlobalEvent]
	public static event System.Action<string> OnBazonked2 = delegate { };
	[GlobalEvent]
	public static event System.Action<bool> OnBazonked3 = delegate { };
}
