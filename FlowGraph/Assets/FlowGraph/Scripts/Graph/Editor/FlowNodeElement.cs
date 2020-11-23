﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class FlowNodeElement : Node
{
	public FlowGraphView graphView = null;
	public FlowNode node = null;

	private FlowPort inputPort = null;
	public FlowPort InputPort => inputPort;

	private VisualElement contents = null;
	public VisualElement Contents => contents;

	public SerializedObject SerializedObject => graphView.window.SerializedObject;

	public event System.Action<FlowEffectElement> OnEffectSelected = delegate {};

	public FlowNodeElement(FlowGraphView graphView, FlowNode node)
	{
		this.graphView = graphView;
		this.node = node;

		title = node.name;
		
		style.borderBottomLeftRadius = 8;
		style.borderBottomRightRadius = 8;
		style.borderTopRightRadius = 8;
		style.borderTopLeftRadius = 8;

		UseDefaultStyling();

		SetPosition(new Rect(node.position.x, node.position.y, 0, 0));

		inputPort = FlowPort.Create(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowNode), graphView);
		inputPort.portName = string.Empty;

		VisualElement portRoot = this.Query<VisualElement>("title");
		portRoot.Insert(0, inputPort);

		VisualElement titleLabel = this.Query<VisualElement>("title-label");
		titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
		titleLabel.style.flexGrow = 1f;

		if (node.effects.Count == 0)
		{
			var effect = new FlowEffect();
			node.effects.Add(effect);
		}

		VisualElement titleButton = this.Query<VisualElement>("title-button-container");
		titleButton.parent.Remove(titleButton);

		contents = this.Query<VisualElement>("contents");
		
		BuildEffects();
		BindEffects();

		Add(contents);
	}

	public void BuildEffects()
	{
		this.Query<FlowEffectElement>().ForEach(e => e.ClearConnections());
		contents.Clear();

		foreach (var effect in node.effects)
		{
			var effectElement = new FlowEffectElement(this, effect);
			effectElement.OnEffectSelected += (e) => OnEffectSelected(e);
			contents.Add(effectElement);
		}

		VisualElement addEffectRow = UIElementUtils.CreateElementByName<VisualElement>("FlowNodeRowWithAddEffect");
		contents.Add(addEffectRow);

		Button addEffectButton = addEffectRow.Query<Button>();
		addEffectButton.clicked += AddEffectButton_Clicked;
		addEffectButton.text = "Add Effect";
	}

	public void ConnectEffects()
	{
		this.Query<FlowEffectElement>().ForEach((e) => e.ConnectToNext());
	}

	public void BindEffects()
	{
		SerializedObject.Update();

		this.Query<FlowEffectElement>().ForEach((e) => { e.Unbind(); e.Rebind(); });
		this.Bind(SerializedObject);
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(newPos);

		Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Move Node");
		node.position = new Vector2(newPos.x, newPos.y);
	}

	private void AddEffectButton_Clicked()
	{
		AddEffect();
	}

	private void AddEffect()
	{
		Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Add Effect");

		var effect = new FlowEffect();
		effect.sequenceMode = FlowEffect.SequenceMode.AfterPrev;
		node.effects.Add(effect);

		BuildEffects();
		BindEffects();
		ConnectEffects();
	}
}
