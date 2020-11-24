using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClass : MonoBehaviour
{
	[GlobalEvent]
	public static event System.Action OnTested = delegate {};
	[GlobalEvent]
	public static event System.Action<TestClass> OnTested2 = delegate { };
	[GlobalEvent]
	public static event System.Action<bool, string, Color> OnTested3 = delegate { };
}
