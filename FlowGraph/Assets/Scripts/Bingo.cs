using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bingo : MonoBehaviour
{
	[GlobalEvent]
	public static event System.Action OnBingo = delegate {};
	[GlobalEvent]
	public static event System.Action<string> OnBongo = delegate { };
	[GlobalEvent]
	public static event System.Action<bool> OnBango = delegate { };
}
