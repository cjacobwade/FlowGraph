using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class FlowEffectElement : VisualElement
{
	public FlowNodeElement nodeElement = null;
	public FlowEffect effect = null;

	protected SerializedObject SerializedObject => nodeElement.graphView.window.SerializedObject;

	public event System.Action<FlowEffectElement> OnEffectSelected = delegate {};

	public FlowEffectElement(FlowNodeElement nodeElement, FlowEffect effect)
	{
		this.nodeElement = nodeElement;
		this.effect = effect;

		Clear();

		VisualElement rowRoot = UIElementUtils.CreateElementByName<VisualElement>("FlowNodeRowWithEffect");
		Add(rowRoot);

		Button button = rowRoot.Query<Button>(className:"flow-node-row-effect-button");
		button.clicked += EffectButton_OnClicked;

		var effectProperty = FindEffectProperty();

		VisualElement afterParent = rowRoot.Query<VisualElement>(className: "flow-node-effect-after-parent");
		
		EnumField enumField = new EnumField(FlowEffect.SequenceMode.After);
		enumField.name = "flow-node-effect-after";
		enumField.BindProperty(effectProperty.FindPropertyRelative("sequenceMode"));
		afterParent.Add(enumField);

		VisualElement timeParent = rowRoot.Query<VisualElement>(className: "flow-node-effect-time-parent");

		FloatField timeField = new FloatField();
		timeField.name = "flow-node-effect-time";
		timeField.BindProperty(effectProperty.FindPropertyRelative("wait"));
		timeParent.Add(timeField);

		string module = effect.function.module.Replace("FlowModule_", "");
		string function = effect.function.function;

		button.text = string.Format("{0}.{1}", module, function);

		FlowPort port = FlowPort.Create(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowNode));
		port.portName = string.Empty;
		port.OnPortConnected += Port_OnPortConnected;
		port.OnPortDisconnected += Port_OnPortDisconnected;

		VisualElement portContainer = rowRoot.Query<VisualElement>(className:"flow-node-row-effect-port-container");
		portContainer.Add(port);
	}

	private void Port_OnPortConnected(FlowPort port, Edge edge)
	{
		var nodeElement = edge.input.node as FlowNodeElement;
		effect.nextNodeID = nodeElement.node.id;

		SerializedObject.Update();
	}

	private void Port_OnPortDisconnected(FlowPort port, Edge edge)
	{
		effect.nextNodeID = -1;

		SerializedObject.Update();
	}

	public SerializedProperty FindEffectProperty()
	{
		var sp = SerializedObject.GetIterator();
		while (sp.Next(true))
		{
			if (sp.type == "FlowEffect")
			{
				FlowEffect effect = EditorUtils.GetTargetObjectOfProperty(sp) as FlowEffect;
				if (effect != null && effect == this.effect)
				{
					return sp;
				}
			}
		}

		return null;
	}

	private void EffectButton_OnClicked()
	{
		OnEffectSelected(this);
	}
}
