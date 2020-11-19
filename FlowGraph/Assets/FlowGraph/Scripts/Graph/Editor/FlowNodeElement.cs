using System.Collections;
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

	private VisualElement contents = null;

	protected SerializedObject SerializedObject => graphView.window.SerializedObject;

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

		FlowPort port = FlowPort.Create(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowNode));
		port.portName = string.Empty;

		VisualElement portRoot = this.Query<VisualElement>("title");
		portRoot.Insert(0, port);

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
		contents.Remove(contents.Query<VisualElement>("top"));
		contents.Remove(contents.Query<VisualElement>("divider"));

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
		addEffectButton.text = "+";

		Add(contents);

		MarkDirtyRepaint();
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(newPos);
		node.position = new Vector2(newPos.x, newPos.y);
	}

	private void AddEffectButton_Clicked()
	{
		AddEffect();
	}

	private void AddEffect()
	{
		var effect = new FlowEffect();
		effect.sequenceMode = FlowEffect.SequenceMode.AfterPrev;
		node.effects.Add(effect);

		SerializedObject.Update();

		var effectElement = new FlowEffectElement(this, effect);
		effectElement.OnEffectSelected += (e) => OnEffectSelected(e);
		contents.Insert(contents.childCount - 1, effectElement);
	}
}
