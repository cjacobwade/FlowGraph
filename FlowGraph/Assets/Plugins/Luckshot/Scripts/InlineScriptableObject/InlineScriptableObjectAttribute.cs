using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InlineScriptableObjectAttribute : Attribute
{
	public string path;
	public bool inline;

	public InlineScriptableObjectAttribute(string path = null, bool inline = true)
	{
		this.path = path;
		this.inline = inline;
	}
}
