using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView; 
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class FlowGraphWindow : EditorWindow
{
	public FlowGraph flowGraph = null;

	public FlowGraphView graphView = null;
	public IMGUIContainer inspector = null;
	  
	public static FlowGraphWindow OpenWindow(FlowGraph flowGraph)
	{
		FlowGraphWindow flowGraphWindow = GetWindow<FlowGraphWindow>();
		flowGraphWindow.titleContent = new GUIContent(flowGraph.name);

		return flowGraphWindow;
	}

	private void OnEnable()
	{
		FlowGraph flowGraph = Selection.activeObject as FlowGraph;
		if (flowGraph == null)
			return;

		this.flowGraph = flowGraph;

		rootVisualElement.AddStyleSheet("FlowGraph");

		// Frame View
		VisualElement frameView = new VisualElement();
		frameView.name = "flow";
		rootVisualElement.Add(frameView);

		// Graph View
		graphView = new FlowGraphView(this, flowGraph);
		graphView.name = "flow-graph";
		graphView.StretchToParentSize();
		frameView.Add(graphView);

		// Inspector
		VisualElement inspectorRoot = UIElementUtils.CreateElementByName<VisualElement>("FlowInspector");
		inspectorRoot.name = "flow-inspector-root";
		frameView.Add(inspectorRoot);

		VisualElement buttonRoot = UIElementUtils.CreateElementByName<VisualElement>("FlowInspectorHeaderButton");
		VisualElement header = inspectorRoot.Query<VisualElement>(className: "flow-inspector-header-row");
		header.Add(buttonRoot);

		Button button = buttonRoot.Query<Button>(className:"unity-button");
		button.text = "Effect";
		button.clicked += EffectButton_OnClicked;

		inspector = inspectorRoot.Query<IMGUIContainer>("unity-imgui-container");
		inspectorRoot.Add(inspector);

		rootVisualElement.MarkDirtyRepaint();
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
