using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(MeshProcessor), true)]
public class MeshProcessorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if(GUI.changed)
		{
			foreach (var t in targets)
			{
				MeshProcessor meshProcessor = t as MeshProcessor;
				if (meshProcessor == null)
					continue;

				List<IMeshProcessListener> listeners = new List<IMeshProcessListener>();

				Component[] components = meshProcessor.GetComponents<Component>();
				foreach (var component in components)
				{
					IMeshProcessListener listener = component as IMeshProcessListener;
					if (listener != null)
						listeners.Add(listener);
				}

				foreach (var listener in listeners)
					listener.NotifyMeshProcessorChanged(meshProcessor);
			}
		}
	}
}
