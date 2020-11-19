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

	private SerializedObject so = null;
	private PropertyField propertyField = null;

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

		VisualElement inspector = inspectorRoot.Query<VisualElement>(className:"flow-inspector-content");

		propertyField = new PropertyField();
		propertyField.StretchToParentSize();

		so = new SerializedObject(flowGraph);
		propertyField.Bind(so);

		inspector.Add(propertyField);

		rootVisualElement.MarkDirtyRepaint();
	}

	private void GraphView_OnEffectSelected(FlowEffectElement effectRot)
	{
		if(selectedEffectRow != null)
			selectedEffectRow.styleSheets.Remove(selectedStyle);

		selectedEffectRow = effectRot;

		var effectProp = FindEffectProperty();
		if (effectProp != null)
		{
			effectProp.isExpanded = true;

			var iter = effectProp.Copy();
			while (iter.Next(true))
				iter.isExpanded = true;

			propertyField.BindProperty(effectProp);

			var effect = selectedEffectRow.effect;
			string label = string.Format("{0} - {1}", effect.function.module.Replace("FlowModule_", ""), effect.function.function);
			propertyField.label = label;
		}

		if (selectedEffectRow != null)
			selectedEffectRow.styleSheets.Add(selectedStyle);
	}

	private SerializedProperty FindEffectProperty()
	{
		if (selectedEffectRow == null || selectedEffectRow.effect == null)
			return null;

		var sp = so.GetIterator();
		while (sp.Next(true))
		{
			if (sp.type == "FlowEffect")
			{
				FlowEffect effect = EditorUtils.GetTargetObjectOfProperty(sp) as FlowEffect;
				if (effect != null && effect == selectedEffectRow.effect)
				{
					return sp;
				}
			}
		}

		return null;
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
