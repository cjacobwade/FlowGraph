using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GETest : MonoBehaviour
{
	[SerializeField]
	private GlobalEventDefinition eventDefinition = null;

	[SerializeField]
	private GlobalEventDefinition[] eventDefinitions = null;

	[SerializeReference]
	public List<EventParameterBase> parameters = new List<EventParameterBase>();

	[SerializeField]
	private EventParameter_Bool boolParam = new EventParameter_Bool();

	[SerializeField]
	private EventParameter_Color colorParam = new EventParameter_Color();

	[SerializeField]
	private EventParameter_Object objectParam = new EventParameter_Object();

	[Button("Fill Parameters")]
	public void FillParameters()
	{
		parameters.Clear();

		parameters.Add(new EventParameter_Color() { name = "fadeColor"});
		parameters.Add(new EventParameter_Object());
		parameters.Add(new EventParameter_Bool());
		parameters.Add(new EventParameter_Int());
		parameters.Add(new EventParameter_Vector3());
	}
}
