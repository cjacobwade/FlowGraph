using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArgumentContainer : MonoBehaviour
{
	[System.Serializable]
	public class TestType
	{
		public float f;
		public int i;
		public string s;
	}

	[SerializeField]
	private FlowModuleFunction function = new FlowModuleFunction();

	[SerializeField]
	private Argument_Float floatArgument = new Argument_Float();

	[SerializeField]
	private Argument_Object objectArgument = new Argument_Object();

	[SerializeField]
	private Argument_ObjectArray objectArrArgument = new Argument_ObjectArray();

	[SerializeField]
	private Argument_Vector2 vec2Argument = new Argument_Vector2();

	[SerializeReference]
    private List<ArgumentBase> arguments = new List<ArgumentBase>();

	[ContextMenu("Add Test Arguments")]
    private void AddTestArguments()
    {
		arguments.Clear();

        arguments.Add(new Argument_Float());
        arguments.Add(new Argument_Color());
        arguments.Add(new Argument_Int());
        arguments.Add(new Argument_Object());
        arguments.Add(new Argument_Vector2());
	}
}
