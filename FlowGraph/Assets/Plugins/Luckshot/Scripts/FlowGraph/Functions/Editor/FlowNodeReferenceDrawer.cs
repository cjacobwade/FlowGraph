using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(FlowNodeReference))]
public class FlowNodeReferenceDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		property.serializedObject.Update();

		FlowGraph flowGraph = property.serializedObject.targetObject as FlowGraph;
		if (flowGraph == null)
		{
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth *= 0.5f;

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, label, new GUIContent("FlowNodeReference must be under FlowGraph"), EditorStyles.helpBox);

			EditorGUIUtility.labelWidth = prevLabelWidth;
			EditorGUI.EndProperty();
			return;
		}

		var nodeIDProp = property.FindPropertyRelative("nodeID");
		if (nodeIDProp == null)
		{
			EditorGUI.EndProperty();
			return;
		}

		position.height = EditorGUIUtility.singleLineHeight;

		List<FlowNode> nodes = new List<FlowNode>(flowGraph.nodes);
		nodes.Sort((x, y) => x.id.CompareTo(y.id));

		int selectedIndex = 0;
		for(int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].id == nodeIDProp.intValue)
				selectedIndex = i;
		}

		List<string> nodeNames = new List<string>() { "None" };
		foreach (var n in nodes)
			nodeNames.Add(n.name);

		selectedIndex = EditorGUI.Popup(position, label.text, nodeIDProp.intValue + 1, nodeNames.ToArray());
		selectedIndex -= 1; // offset because of none

		if(EditorGUI.EndChangeCheck())
		{
			nodeIDProp.intValue = selectedIndex;
			if(selectedIndex >= 0)
				nodeIDProp.intValue = nodes[selectedIndex].id;

			property.serializedObject.ApplyModifiedProperties();
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;

		FlowGraph flowGraph = property.serializedObject.targetObject as FlowGraph;
		if (flowGraph == null)
			height = EditorGUIUtility.singleLineHeight * 2f;

		return height;
	}
}
