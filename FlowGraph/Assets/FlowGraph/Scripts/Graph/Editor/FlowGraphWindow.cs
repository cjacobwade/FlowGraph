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

	private SerializedObject serializedObject = null;
	public SerializedObject SerializedObject => serializedObject;

	private PropertyField propertyField = null;

	private FlowEffectElement selectedEffectElement = null;

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

		rootVisualElement.Clear();

		Undo.undoRedoPerformed -= Undo_OnUndoRedo;
		Undo.undoRedoPerformed += Undo_OnUndoRedo;

		this.flowGraph = flowGraph;

		serializedObject = new SerializedObject(flowGraph);

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
		propertyField.Bind(serializedObject);
		inspector.Add(propertyField);

		rootVisualElement.MarkDirtyRepaint();
	}

	private void Undo_OnUndoRedo()
	{
		SerializedObject.Update();

		Vector3 pos = graphView.contentViewContainer.transform.position;
		Vector3 scale = graphView.contentViewContainer.transform.scale;

		OnEnable();

		graphView.UpdateViewTransform(pos, scale);
	}

	private void GraphView_OnEffectSelected(FlowEffectElement effectRot)
	{
		if(selectedEffectElement != null)
			selectedEffectElement.styleSheets.Remove(selectedStyle);

		selectedEffectElement = effectRot;

		var effectProp = selectedEffectElement.FindEffectProperty();
		if (effectProp != null)
		{
			effectProp.isExpanded = true;

			var iter = effectProp.Copy();
			while (iter.Next(true))
				iter.isExpanded = true;

			propertyField.BindProperty(effectProp);

			var effect = selectedEffectElement.effect;
			string label = string.Format("{0} - {1}", effect.function.module.Replace("FlowModule_", ""), effect.function.function);
			propertyField.label = label;
		}

		if (selectedEffectElement != null)
			selectedEffectElement.styleSheets.Add(selectedStyle);
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
