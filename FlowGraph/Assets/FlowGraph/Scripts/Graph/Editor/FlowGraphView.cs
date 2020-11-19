using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

public class FlowGraphView : GraphView
{
	public FlowGraphWindow window = null;
	public FlowGraph flowGraph = null;

	protected SerializedObject SerializedObject => window.SerializedObject;

	public event System.Action<FlowEffectElement> OnEffectSelected = delegate {};

	public FlowGraphView(FlowGraphWindow window, FlowGraph flowGraph)
	{
		this.window = window;
		this.flowGraph = flowGraph;

		var gridBackground = new GridBackground();
		gridBackground.name = "flow-grid-background";
		gridBackground.StretchToParentSize();
		Insert(0, gridBackground);

		SetupManipulators();
		SetupContextMenu();

		if (flowGraph.nodes.Count == 0)
			AddNode(Vector2.one * 100, "Start");

		foreach (var node in flowGraph.nodes)
		{
			var nodeElement = new FlowNodeElement(this, node);
			nodeElement.OnEffectSelected += (e) => OnEffectSelected(e);
			AddElement(nodeElement);
		}

		// Wire up connections
		this.Query<FlowNodeElement>().ForEach((n) =>
		{
			n.Query<FlowEffectElement>().ForEach((e) =>
			{
				int nextNodeId = e.effect.nextNodeID;
				if(nextNodeId != -1)
				{
					FlowPort effectPort = e.Query<FlowPort>();
					FlowPort nextNodePort = this.Query<FlowNodeElement>()
						.Where((n2) => nextNodeId == n2.node.id)
						.First()
						.Query<FlowPort>();
					
					Add(effectPort.ConnectTo(nextNodePort));
				}
			});
		});

		deleteSelection = OnDeletedSelection;
		graphViewChanged = OnGraphViewChanged;
	}

	private void SetupManipulators()
	{
		SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

		this.AddManipulator(new ContentDragger() { clampToParentEdges = true });
		this.AddManipulator(new SelectionDragger() { clampToParentEdges = true });
		this.AddManipulator(new RectangleSelector());
		this.AddManipulator(new ClickSelector());
	}

	private void SetupContextMenu()
	{
		RegisterCallback((ContextualMenuPopulateEvent evt) =>
		{
			evt.menu.AppendAction("Add Node", ContextMenu_AddNode);
		});
	}

	private void AddNode(Vector2 mousePosition, string name = null)
	{
		FlowNode node = new FlowNode(mousePosition);
		node.name = string.IsNullOrEmpty(name) ? GetUnusedDefaultNodeName() : name;
		node.id = GetUnusedNodeID();

		flowGraph.nodes.Add(node);
	}

	private void ContextMenu_AddNode(DropdownMenuAction dropdownMenuAction)
	{
		AddNode(dropdownMenuAction.eventInfo.mousePosition);
	}

	private string GetUnusedDefaultNodeName()
	{
		int usedCount = 0;
		string name = "Node";

		while (true)
		{
			bool nameTaken = false;
			foreach (var node in flowGraph.nodes)
			{
				if (node.name == name)
				{
					nameTaken = true;
					break;
				}
			}

			if (nameTaken)
			{
				usedCount++;
				name = string.Format("Node{0}", usedCount);
			}
			else
			{
				break;
			}
		}

		return name;
	}

	private int GetUnusedNodeID()
	{
		int id = 0;

		while (true)
		{
			bool idTaken = false;
			foreach (var node in flowGraph.nodes)
			{
				if (node.id == id)
				{
					idTaken = true;
					break;
				}
			}

			if (idTaken) id++;
			else break;
		}

		return id;
	}

	private void OnDeletedSelection(string operationName, AskUser askUser)
	{
		List<FlowNodeElement> elementsToRemove = new List<FlowNodeElement>();

		foreach (var selectable in selection)
		{
			if(selectable is FlowNodeElement node)
				elementsToRemove.Add(node);
		}

		List<Edge> connectionsToRemove = new List<Edge>();

		ports.ForEach((p) =>
		{
			if (p.node is FlowNodeElement nodeElement &&
				elementsToRemove.Contains(nodeElement) && p.connected)
			{
				foreach (var connection in p.connections)
					connectionsToRemove.Add(connection);
			}
		});

		Undo.RegisterCompleteObjectUndo(flowGraph, "Deleted Graph Elements");

		for (int i = 0; i < connectionsToRemove.Count; i++)
		{
			var edge = connectionsToRemove[i];
			edge.output.Disconnect(edge);
			edge.input.Disconnect(edge);

			edge.RemoveFromHierarchy();
		}

		for (int i = 0; i < elementsToRemove.Count; i++)
		{
			var nodeElement = elementsToRemove[i];

			RemoveElement(nodeElement);
			flowGraph.nodes.Remove(nodeElement.node);
		}

		selection.Clear();
		MarkDirtyRepaint();
	}

	public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
	{
		List<Port> compatiblePorts = new List<Port>();

		ports.ForEach((port) =>
		{
			if (startPort != port && port.direction != startPort.direction)
			{
				compatiblePorts.Add(port);
			}
		});

		return compatiblePorts;
	}

	private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
	{
		// Serialize changes to graph

		return graphViewChange;
	}
}