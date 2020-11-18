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

		string module = effect.function.module.Replace("FlowModule_", "");
		string function = effect.function.function;

		button.text = string.Format("{0}.{1}", module, function);

		FlowPort port = FlowPort.Create(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowNode));
		port.portName = string.Empty;

		VisualElement portContainer = rowRoot.Query<VisualElement>(className:"flow-node-row-effect-port-container");
		portContainer.Add(port);
	}

	private void EffectButton_OnClicked()
	{
		OnEffectSelected(this);
	}
}
