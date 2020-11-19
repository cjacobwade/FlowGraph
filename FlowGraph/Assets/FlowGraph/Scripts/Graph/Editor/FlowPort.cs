using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class FlowPort : Port
{
	// Copied from decompile of regular port
	private class FlowPortEdgeConnectorListener : IEdgeConnectorListener
	{
		private GraphView graphView = null;
		private GraphViewChange graphViewChange = default;

		private List<Edge> edgesToCreate = new List<Edge>();
		private List<GraphElement> edgesToDelete = new List<GraphElement>();

		public FlowPortEdgeConnectorListener(GraphView graphView)
		{
			this.graphView = graphView;
			graphViewChange.edgesToCreate = edgesToCreate;
		}

		public void OnDropOutsidePort(Edge edge, Vector2 position)
		{
			FlowPort fromPort = edge.input as FlowPort;
			if (fromPort == null)
				fromPort = edge.output as FlowPort;

			FlowPort toPort = null;
			graphView.nodes.ForEach((n) =>
			{
				Vector2 localPos = VisualElementExtensions.WorldToLocal(n, position);
				if (n.ContainsPoint(localPos) && (toPort == null || n.layer < toPort.node.layer))
				{
					toPort = n.Query<FlowPort>().Where((p) => p.direction != fromPort.direction);			
				}
			});

			if(toPort != null)
			{
				if (edge.input == null)
					edge.input = toPort;
				else
					edge.output = toPort;

				OnDrop(graphView, edge);
			}
		}

		public void OnDrop(GraphView graphView, Edge edge)
		{
			edgesToCreate.Clear();
			edgesToCreate.Add(edge);

			edgesToDelete.Clear();

			if (edge.input.capacity == Capacity.Single)
			{
				foreach (Edge connection in edge.input.connections)
				{
					if (connection != edge)
						edgesToDelete.Add(connection);
				}
			}

			if (edge.output.capacity == Capacity.Single)
			{
				foreach (Edge connection2 in edge.output.connections)
				{
					if (connection2 != edge)
						edgesToDelete.Add(connection2);
				}
			}

			if (edgesToDelete.Count > 0)
				graphView.DeleteElements(edgesToDelete);

			List<Edge> tempEdgesToCreate = edgesToCreate;
			if (graphView.graphViewChanged != null)
			{
				var graphViewChange = graphView.graphViewChanged(this.graphViewChange);
				tempEdgesToCreate = graphViewChange.edgesToCreate;
			}

			foreach (Edge item in tempEdgesToCreate)
			{
				graphView.AddElement(item);
				edge.input.Connect(item);
				edge.output.Connect(item);
			}
		}
	}

	public class FlowEdgeDragHelper : EdgeDragHelper<Edge>
	{
		public FlowEdgeDragHelper(IEdgeConnectorListener listener) : base(listener)
		{
			var listenerField = GetType().BaseType.GetField("m_Listener", BindingFlags.Instance | BindingFlags.NonPublic);
			listenerField.SetValue(this, listener);
			resetPositionOnPan = true;
			Reset(false);
		}

		public override void HandleMouseUp(MouseUpEvent evt)
		{
			bool didConnect = false;
			Vector2 mousePosition = evt.mousePosition;
			m_GraphView.ports.ForEach(delegate (Port p)
			{
				p.OnStopEdgeDragging();
			});

			FieldInfo ghostEdgeField = GetType().BaseType.GetField("m_GhostEdge", BindingFlags.Instance | BindingFlags.NonPublic);
			Edge ghostEdge = ghostEdgeField.GetValue(this) as Edge;
			if (ghostEdge != null)
			{
				if (ghostEdge.input != null)
				{
					ghostEdge.input.portCapLit = false;
				}
				if (ghostEdge.output != null)
				{
					ghostEdge.output.portCapLit = false;
				}
				m_GraphView.RemoveElement(ghostEdge);
				ghostEdge.input = null;
				ghostEdge.output = null;
				ghostEdge = null;
			}

			MethodInfo getEndPortMethod = GetType().BaseType.GetMethod("GetEndPort", BindingFlags.Instance | BindingFlags.NonPublic);

			Port endPort = getEndPortMethod.Invoke(this, new object[] { mousePosition }) as Port;
			if (endPort == null && m_Listener != null)
			{
				m_Listener.OnDropOutsidePort(edgeCandidate, mousePosition);
			}
			edgeCandidate.SetEnabled(true);
			if (edgeCandidate.input != null)
			{
				edgeCandidate.input.portCapLit = false;
			}
			if (edgeCandidate.output != null)
			{
				edgeCandidate.output.portCapLit = false;
			}
			if (edgeCandidate.input != null && edgeCandidate.output != null)
			{
				Port input = edgeCandidate.input;
				Port output = edgeCandidate.output;
				m_GraphView.DeleteElements(new Edge[1]
				{
					edgeCandidate
				});
				edgeCandidate.input = input;
				edgeCandidate.output = output;
			}
			else
			{
				m_GraphView.RemoveElement(edgeCandidate);
			}
			if (endPort != null)
			{
				if (endPort.direction == Direction.Output)
				{
					edgeCandidate.output = endPort;
				}
				else
				{
					edgeCandidate.input = endPort;
				}
				m_Listener.OnDrop(m_GraphView, edgeCandidate);
				didConnect = true;
			}
			else
			{
				edgeCandidate.output = null;
				edgeCandidate.input = null;
			}

			edgeCandidate.ResetLayer();
			edgeCandidate = null;

			m_CompatiblePorts = null;
			Reset(didConnect);
		}
	}

	public class FlowEdgeConnector : EdgeConnector<Edge>
	{
		public FlowEdgeConnector(IEdgeConnectorListener listener) : base(listener)
		{
			var edgeDragHelper = GetType().BaseType.GetField("m_EdgeDragHelper", BindingFlags.Instance | BindingFlags.NonPublic);
			edgeDragHelper.SetValue(this, new FlowEdgeDragHelper(listener));
		}
	}

	public event Action<FlowPort, Edge> OnPortConnected = delegate {};
	public event Action<FlowPort, Edge> OnPortDisconnected = delegate { };

	private FlowPort(Orientation orientation, Direction direction, Capacity capacity, Type type) : 
		base(orientation, direction, capacity, type)
	{
		
	}

	public static FlowPort Create(Orientation orientation, Direction direction, Capacity capacity, Type type, GraphView graphView)
	{
		FlowPort port = new FlowPort(orientation, direction, capacity, type)
		{
			m_EdgeConnector = new FlowEdgeConnector(new FlowPortEdgeConnectorListener(graphView))
		};

		port.AddManipulator(port.m_EdgeConnector);
		return port;
	}

	public override void Connect(Edge edge)
	{
		base.Connect(edge);
		OnPortConnected(this, edge);
	}

	public override void Disconnect(Edge edge)
	{
		base.Disconnect(edge);
		OnPortDisconnected(this, edge);
	}
}
