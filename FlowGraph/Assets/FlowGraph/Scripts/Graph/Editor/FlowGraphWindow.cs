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

	private FlowEffectElement selectedEffectRow = null;

	private StyleSheet selectedStyle = null;

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

		selectedStyle = UIElementUtils.GetStyleSheet("FlowGraph_Selected");

		// Frame View
		VisualElement frameView = new VisualElement();
		frameView.name = "flow";
		rootVisualElement.Add(frameView);

		// Graph View
		graphView = new FlowGraphView(this, flowGraph);
		graphView.OnEffectSelected += GraphView_OnEffectSelected;
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

		inspector = inspectorRoot.Query<IMGUIContainer>(className:"unity-imgui-container");
		inspector.onGUIHandler = Inspector_OnGUI;

		rootVisualElement.MarkDirtyRepaint();
	}

	private void GraphView_OnEffectSelected(FlowEffectElement effectRot)
	{
		if(selectedEffectRow != null)
			selectedEffectRow.styleSheets.Remove(selectedStyle);

		selectedEffectRow = effectRot;

		if (selectedEffectRow != null)
			selectedEffectRow.styleSheets.Add(selectedStyle);
	}

	private void Inspector_OnGUI()
	{
		SerializedObject so = new SerializedObject(flowGraph);

		var sp = so.GetIterator();
		while (sp.Next(true))
		{
			if(sp.type == "FlowEffect")
			{
				FlowEffect effect = EditorUtils.GetTargetObjectOfProperty(sp) as FlowEffect;
				if(	effect != null && selectedEffectRow != null &&
					effect == selectedEffectRow.effect)
				{
					var sp2 = sp.Copy();
					while (sp2.Next(true))
					{
						if(!sp2.isExpanded)
							sp2.isExpanded = true; 
					}

					sp.isExpanded = true;
					EditorGUILayout.PropertyField(sp, new GUIContent(string.Format("{0} - {1}", selectedEffectRow.nodeElement.title, sp.displayName)), true);
					return;
				}
			}
		}
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
