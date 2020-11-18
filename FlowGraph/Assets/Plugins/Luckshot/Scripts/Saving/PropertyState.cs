using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Dynamic;

[System.Serializable]
public class PropertyState
{
	[System.Serializable]
	public struct Field
	{
		public string name;
		public string type;
		public string value;
	}

	public string type = string.Empty;
	public List<Field> properties = new List<Field>();
	public List<Field> fields = new List<Field>();
}
