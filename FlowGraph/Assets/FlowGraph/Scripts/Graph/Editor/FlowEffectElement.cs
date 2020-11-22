﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;

public class FlowEffectElement : VisualElement
{
	public FlowNodeElement nodeElement = null;
	private FlowGraphView graphView = null;

	public FlowEffect effect = null;

	private Vector2 moveOffset = Vector2.zero;

	private static FlowNodeElement prevNodeElement = null;

	private static VisualElement movingElement = null;
	private static FlowEffectElement movingEffectElement = null;

	public static void ResetMoveState()
	{ 
		movingElement = null;
		movingEffectElement = null;
	}

	private FlowPort outPort = null;

	protected SerializedObject SerializedObject => nodeElement.graphView.window.SerializedObject;

	public event System.Action<FlowEffectElement> OnEffectSelected = delegate {};

	private EnumField enumField = null;
	private FloatField timeField = null;
	private Button button = null;

	public FlowEffectElement(FlowNodeElement nodeElement, FlowEffect effect)
	{
		this.nodeElement = nodeElement;
		this.graphView = nodeElement.graphView;

		this.effect = effect;

		Clear();

		VisualElement rowRoot = UIElementUtils.CreateElementByName<VisualElement>("FlowNodeRowWithEffect");
		Add(rowRoot);

		VisualElement selector = rowRoot.Query<VisualElement>(className: "flow-node-effect-handle");
		selector.RegisterCallback<MouseDownEvent>(Selector_OnMouseDown);

		VisualElement afterParent = rowRoot.Query<VisualElement>(className: "flow-node-effect-after-parent");
		
		enumField = new EnumField(FlowEffect.SequenceMode.After);
		enumField.name = "flow-node-effect-after";
		afterParent.Add(enumField);

		VisualElement timeParent = rowRoot.Query<VisualElement>(className: "flow-node-effect-time-parent");

		timeField = new FloatField();
		timeField.name = "flow-node-effect-time";
		timeParent.Add(timeField);

		button = rowRoot.Query<Button>(className: "flow-node-row-effect-button");
		button.clicked += EffectButton_OnClicked;

		RenameButton();

		outPort = FlowPort.Create(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowNode), nodeElement.graphView);
		outPort.portName = string.Empty;
		outPort.OnPortConnected += Port_OnPortConnected;
		outPort.OnPortDisconnected += Port_OnPortDisconnected;

		VisualElement portContainer = rowRoot.Query<VisualElement>(className:"flow-node-row-effect-port-container");
		portContainer.Add(outPort);

		Rebind();
	}

	public void RenameButton()
	{
		string module = effect.function.module.Replace("FlowModule_", "");
		string function = effect.function.function;

		button.text = string.Format("{0}.{1}", module, function);
	}

	public void Rebind()
	{
		var effectProperty = FindEffectProperty();
		if (effectProperty != null)
		{
			enumField.bindingPath = effectProperty.FindPropertyRelative("sequenceMode").propertyPath;
			timeField.bindingPath = effectProperty.FindPropertyRelative("wait").propertyPath;
		}
	}

	private void Selector_OnMouseDown(MouseDownEvent evt)
	{
		if (movingElement != null || movingEffectElement != null)
			return;

		evt.StopPropagation();

		VisualElement selector = evt.target as VisualElement;

		graphView.RegisterCallback<MouseMoveEvent>(Selector_OnMouseMove);
		graphView.RegisterCallback<MouseUpEvent>(Selector_OnMouseUp);

		movingEffectElement = this;

		movingElement = new VisualElement();
		movingElement.style.width = layout.width;
		movingElement.style.height = layout.height;

		moveOffset = VisualElementExtensions.WorldToLocal(this, evt.mousePosition);

		Vector2 pos = evt.mousePosition - Vector2.up * layout.height - moveOffset;
		movingElement.transform.position = pos;
		movingElement.transform.scale = worldTransform.lossyScale;

		graphView.Add(movingElement);

		OnEffectSelected(this);
	}

	private void Selector_OnMouseMove(MouseMoveEvent evt)
	{
		if(movingEffectElement != null)
		{
			FlowNodeElement prevParentNode = nodeElement;
			FlowNodeElement newParentNode = null;

			graphView.Query<FlowNodeElement>().ForEach(n =>
			{
				Vector2 localPoint = VisualElementExtensions.WorldToLocal(n, evt.mousePosition);
				if (n.ContainsPoint(localPoint) && (newParentNode == null || n.layer > newParentNode.layer))
					newParentNode = n;
			});

			if(newParentNode != null)
			{
				List<FlowEffectElement> otherEffects = new List<FlowEffectElement>();
				newParentNode.Query<FlowEffectElement>().ForEach(e =>
				{
					if (e == this) return;
					otherEffects.Add(e);
				});

				otherEffects.Sort((x, y) => x.layout.y.CompareTo(y.layout.y));

				Vector2 nodeMousePos = VisualElementExtensions.WorldToLocal(newParentNode, evt.mousePosition);

				int prevSiblingIndex = -1;
				if (prevParentNode != null)
					prevSiblingIndex = prevParentNode.Contents.IndexOf(this);

				int siblingIndex = 0;
				foreach(var e in otherEffects)
				{
					Rect nodeEffectRect = e.ChangeCoordinatesTo(newParentNode, new Rect(Vector2.zero, e.layout.size));

					Vector2 nodeEffectPos = new Vector2(nodeEffectRect.x, nodeEffectRect.y);
					if (prevSiblingIndex > siblingIndex)
						nodeEffectPos.y += nodeEffectRect.height;

					if (nodeMousePos.y > nodeEffectPos.y)
						siblingIndex++;
				}

				Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Effect Moved");	

				bool changingHierachy = prevParentNode != newParentNode || prevSiblingIndex != siblingIndex;

				if (prevParentNode != newParentNode)
				{
					if (prevParentNode != null)
					{
						prevParentNode.node.effects.Remove(effect);
						prevParentNode.Contents.Remove(this);
					}

					nodeElement = newParentNode;
				}
				else 
				{
					if (changingHierachy)
					{
						newParentNode.node.effects.Remove(effect);
						newParentNode.Contents.Remove(this);
					}
				}

				if (changingHierachy)
				{
					newParentNode.Contents.Insert(siblingIndex, this);
					newParentNode.node.effects.Insert(siblingIndex, effect);

					SerializedObject.Update();

					OnEffectSelected(this);
				}
			}
			else
			{
				if (prevParentNode != null)
				{
					int prevSiblingIndex = prevParentNode.Contents.IndexOf(this);
					if( prevSiblingIndex == prevParentNode.node.effects.Count - 1)
					{
						// hack to fix error when dragging last effect out of node
						var sp = graphView.SerializedObject.FindProperty("startNodeID");
						graphView.window.PropertyField.BindProperty(sp);
						graphView.window.PropertyField.label = string.Empty;
					}

					movingElement.Add(movingEffectElement);
					nodeElement = null;

					prevParentNode.node.effects.Remove(effect);

					prevParentNode.RebuildEffectElements();

					OnEffectSelected(this);
				}

				movingElement.transform.position = evt.mousePosition - Vector2.up * layout.height - moveOffset;
			}
		}
	}

	private void Selector_OnMouseUp(MouseUpEvent evt)
	{
		VisualElement selector = evt.target as VisualElement;
		graphView.UnregisterCallback<MouseMoveEvent>(Selector_OnMouseMove);
		graphView.UnregisterCallback<MouseUpEvent>(Selector_OnMouseUp);

		if(movingEffectElement != null)
		{
			if (nodeElement != null)
			{
				nodeElement.RebuildEffectElements();
			}
			else
			{
				Destroy();
			}

			movingElement.parent.Remove(movingElement);

			movingEffectElement = null;
			movingElement = null;
		}
	}

	public void Destroy()
	{
		Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Remove Effect");

		List<Edge> connectionsToRemove = new List<Edge>();
		connectionsToRemove.AddRange(outPort.connections);

		foreach (var connection in connectionsToRemove)
		{
			connection.input.Disconnect(connection);
			connection.output.Disconnect(connection);
			graphView.RemoveElement(connection);
		}

		parent.Remove(this);
	}

	private void Port_OnPortConnected(FlowPort port, Edge edge)
	{
		Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Connected Port");

		var nodeElement = edge.input.node as FlowNodeElement;
		effect.nextNodeID = nodeElement.node.id;
	}

	private void Port_OnPortDisconnected(FlowPort port, Edge edge)
	{
		Undo.RegisterCompleteObjectUndo(graphView.flowGraph, "Disconnect Port");
		effect.nextNodeID = -1;
	}

	public SerializedProperty FindEffectProperty()
	{
		graphView.SerializedObject.Update();

		var sp = graphView.SerializedObject.GetIterator();
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
		Rebind();
		OnEffectSelected(this);
	}
}
