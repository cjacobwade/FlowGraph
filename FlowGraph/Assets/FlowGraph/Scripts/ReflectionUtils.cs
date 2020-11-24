using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ReflectionUtils
{
	private static List<string> projectAssemblies = new List<string>()
	{
		"Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
		"Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" ,
		"Assembly-CSharp-Editor-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" ,
		"Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
	};

	public static List<string> ProjectAssemblies => projectAssemblies;
}
