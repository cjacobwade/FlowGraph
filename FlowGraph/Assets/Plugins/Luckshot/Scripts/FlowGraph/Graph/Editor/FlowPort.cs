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
	public class FlowEdgeDragHelper : EdgeDragHelper<Edge>
	{
		public FlowEdgeDragHelper(IEdgeConnectorListener listener) : base(listener)
		{
			var listenerField = GetType().BaseType.GetField("m_Listener", BindingFlags.Instance | BindingFlags.NonPublic);
			listenerField.SetValue(this, listener);
			resetPositionOnPan = true;
			Reset(false);
		}
		
		public Port GetEndPoint_NodesAccepted(Vector2 mousePosition)
		{
			Port myPort = draggedPort;
			Port otherPort = null;

			if (myPort.direction == Direction.Input)
			{
				MethodInfo getEndPortMethod = GetType().BaseType.GetMethod("GetEndPort", BindingFlags.Instance | BindingFlags.NonPublic);
				otherPort = getEndPortMethod.Invoke(this, new object[] { mousePosition }) as Port;

				return otherPort;
			}
			else if (myPort.direction == Direction.Output)
			{
				m_GraphView.ports.ForEach(p =>
				{
					if (p.direction != myPort.direction && 
						p.orientation == myPort.orientation)
					{
						Vector2 localPos = VisualElementExtensions.WorldToLocal(p.node, mousePosition);
						if(p.node.ContainsPoint(localPos) && (otherPort == null || otherPort.node.layer > p.node.layer))
						{
							otherPort = p;
						}
					}
				});

				return otherPort;
			}

			return null;
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

			Port endPort = GetEndPoint_NodesAccepted(mousePosition);
			if (endPort == null && m_Listener != null)
			{
				m_Listener.OnDropOutsidePort(edgeCandidate, mousePosition);

				if (draggedPort.direction == Direction.Output)
				{
					Edge edgeToDelete = null;
					m_GraphView.edges.ForEach((e) =>
					{
						if (e.output == draggedPort && e.input != null)
							edgeToDelete = e;
					});
					if (edgeToDelete != null)
					{
						draggedPort.Disconnect(edgeToDelete);
						draggedPort.portCapLit = false;

						m_GraphView.RemoveElement(edgeToDelete);
					}
				}
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
		var listenerType = typeof(Port).GetNestedType("DefaultEdgeConnectorListener", BindingFlags.NonPublic | BindingFlags.Instance);
		object listener = Activator.CreateInstance(listenerType);

		FlowPort port = new FlowPort(orientation, direction, capacity, type)
		{
			m_EdgeConnector = new FlowEdgeConnector(listener as IEdgeConnectorListener)
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
