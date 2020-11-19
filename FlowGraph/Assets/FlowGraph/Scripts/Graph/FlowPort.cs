using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using UnityEditor;
using System.Reflection;

public class FlowPort : Port
{
	public event Action<FlowPort, Edge> OnPortConnected = delegate {};
	public event Action<FlowPort, Edge> OnPortDisconnected = delegate { };

	private FlowPort(Orientation orientation, Direction direction, Capacity capacity, Type type) : 
		base(orientation, direction, capacity, type)
	{
	}

	public static FlowPort Create(Orientation orientation, Direction direction, Capacity capacity, Type type)
	{
		// This stupid hack is because Unity is a bad engine for bad people
		Type connectorListenerType = typeof(Port).GetNestedType("DefaultEdgeConnectorListener", BindingFlags.NonPublic);
		object connectorListener = Activator.CreateInstance(connectorListenerType);

		FlowPort port = new FlowPort(orientation, direction, capacity, type)
		{
			m_EdgeConnector = new EdgeConnector<Edge>((IEdgeConnectorListener)connectorListener)
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
