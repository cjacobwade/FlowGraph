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
	public SerializedObject SerializedObject => window.SerializedObject;

	public event System.Action<FlowEffectElement> OnEffectSelected = delegate {};

	private StyleSheet startNodeStyle = null;

	public FlowGraphView(FlowGraphWindow window, FlowGraph flowGraph)
	{
		this.window = window;
		this.flowGraph = flowGraph;

		var gridBackground = new GridBackground();
		gridBackground.name = "flow-grid-background";
		gridBackground.StretchToParentSize();

		startNodeStyle = UIElementUtils.GetStyleSheet("FlowGraph_StartNode");

		Insert(0, gridBackground);

		SetupManipulators();
		SetupContextMenu();

		if (flowGraph.nodes.Count == 0)
		{
			AddNode(Vector2.one * 100, "Start");
			flowGraph.startNodeID = 0;
		}

		foreach (var node in flowGraph.nodes)
			AddNodeElement(node);

		SetStartNode(flowGraph.startNodeID);

		// Wire up connections
		this.Query<FlowNodeElement>().ForEach((n) => n.ConnectEffects());

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

	public void SetStartNode(int nodeID)
	{
		int prevNodeID = flowGraph.startNodeID;

		if(nodeID != prevNodeID)
		{
			Undo.RecordObject(flowGraph, "Change Start Node");
			flowGraph.startNodeID = nodeID;
		}

		this.Query<FlowNodeElement>().ForEach(n =>
		{
			if (n.node.id == prevNodeID)
				n.styleSheets.Remove(startNodeStyle);
			
			if(n.node.id == nodeID)
				n.styleSheets.Add(startNodeStyle);
		});
	}

	private FlowNode AddNode(Vector2 mousePosition, string name = null)
	{
		Vector2 worldPos = viewTransform.matrix.inverse.MultiplyPoint(mousePosition);

		FlowNode node = new FlowNode(worldPos);
		node.name = string.IsNullOrEmpty(name) ? GetUnusedDefaultNodeName() : name;
		node.id = GetUnusedNodeID();

		flowGraph.nodes.Add(node);

		return node;
	}

	private void ContextMenu_AddNode(DropdownMenuAction dropdownMenuAction)
	{
		Undo.RegisterCompleteObjectUndo(flowGraph, "Add Node");

		FlowNode node = AddNode(dropdownMenuAction.eventInfo.mousePosition);
		AddNodeElement(node);
	}

	private void AddNodeElement(FlowNode node)
	{
		var nodeElement = new FlowNodeElement(this, node);
		nodeElement.OnEffectSelected += (e) => OnEffectSelected(e);
		AddElement(nodeElement);
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

	private void DeleteNodes(ICollection<FlowNodeElement> nodeElements)
	{
		List<Edge> connectionsToRemove = new List<Edge>();

		ports.ForEach((p) =>
		{
			if (p.node is FlowNodeElement nodeElement &&
				nodeElements.Contains(nodeElement) && p.connected)
			{
				foreach (var connection in p.connections)
					connectionsToRemove.Add(connection);
			}
		});

		for (int i = 0; i < connectionsToRemove.Count; i++)
		{
			var edge = connectionsToRemove[i];
			edge.output.Disconnect(edge);
			edge.input.Disconnect(edge);

			RemoveElement(edge);
		}

		foreach(var nodeElement in nodeElements)
		{
			RemoveElement(nodeElement);
			flowGraph.nodes.Remove(nodeElement.node);
		}
	}

	private void OnDeletedSelection(string operationName, AskUser askUser)
	{
		Undo.RegisterCompleteObjectUndo(flowGraph, "Deleted Graph Elements");

		List<FlowNodeElement> elementsToRemove = new List<FlowNodeElement>();
		foreach (var selectable in selection)
		{
			if(selectable is FlowNodeElement node)
				elementsToRemove.Add(node);
		}

		DeleteNodes(elementsToRemove);

		selection.Clear();
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