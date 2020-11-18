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

	public override void OnStartEdgeDragging()
	{
		base.OnStartEdgeDragging();

		if (m_EdgeConnector.edgeDragHelper.draggedPort == this)
		{
			Debug.Log("start");
		}
	}

	public override void OnStopEdgeDragging()
	{
		base.OnStopEdgeDragging();

		if (m_EdgeConnector.edgeDragHelper.draggedPort == this)
		{
			if(m_EdgeConnector.edgeDragHelper.edgeCandidate == null)
			{
				Debug.Log("no edge candidate");
			}
			else if (m_EdgeConnector.edgeDragHelper.edgeCandidate.output != null)
			{
				Debug.Log("has output");
			}
			
			Debug.Log("stop");
		}
	}
}
