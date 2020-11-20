using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView; 
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using System;

public class FlowGraphWindow : EditorWindow
{
	public FlowGraph flowGraph = null;

	public FlowGraphView graphView = null;

	private SerializedObject serializedObject = null;
	public SerializedObject SerializedObject => serializedObject;

	private PropertyField propertyField = null;
	public PropertyField PropertyField => propertyField;

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
		rootVisualElement.name = "flow-root";

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

		// BLah this is not working
		var resizerType = typeof(GraphElement).Assembly.GetType("UnityEditor.Experimental.GraphView.ElementResizer");
		var resizerConstructor = resizerType.GetConstructor(new Type[] { typeof(VisualElement), typeof(ResizerDirection) });
		var resizer = resizerConstructor.Invoke(new object[] { inspectorRoot, ResizerDirection.Left });

		inspectorRoot.AddManipulator(resizer as IManipulator);

		rootVisualElement.Add(inspectorRoot);

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

		FlowEffectElement.CancelMove();
	}

	private void Undo_OnUndoRedo()
	{
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

		if (selectedEffectElement != null)
		{
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
			else
			{
				propertyField.Unbind();
				propertyField.bindingPath = string.Empty;
			}

			selectedEffectElement.styleSheets.Add(selectedStyle);
		}
		else
		{
			propertyField.Unbind();
		}
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
