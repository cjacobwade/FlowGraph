using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView; 
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using UnityEngine.Profiling;

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

	private StyleSheet runningEffectStyle = null;
	private StyleSheet runningPreEffectStyle = null;
	private StyleSheet runningNodeStyle = null;

	// example of width resizing based on mouse capture
	// https://github.com/Unity-Technologies/UIElementsExamples/blob/master/Assets/QuickIntro/Editor/FloatingDemoWindow.cs

	public static FlowGraphWindow OpenWindow(FlowGraph flowGraph)
	{
		FlowGraphWindow flowGraphWindow = GetWindow<FlowGraphWindow>();
		return flowGraphWindow;
	}

	private void OnEnable()
	{
		Selection.selectionChanged -= OnSelectionChanged;
		Selection.selectionChanged += OnSelectionChanged;

		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

		FlowGraph flowGraph = Selection.activeObject as FlowGraph;
		if (flowGraph == null)
			return;

		Undo.undoRedoPerformed -= Undo_OnUndoRedo;
		Undo.undoRedoPerformed += Undo_OnUndoRedo;

		this.flowGraph = flowGraph;
		this.titleContent = new GUIContent(flowGraph.name);

		rootVisualElement.Clear();
		rootVisualElement.name = "flow-root";

		serializedObject = new SerializedObject(flowGraph);
		rootVisualElement.AddStyleSheet("FlowGraph");

		selectedStyle = UIElementUtils.GetStyleSheet("FlowGraph_Selected");

		runningEffectStyle = UIElementUtils.GetStyleSheet("FlowGraph_RunningEffect");
		runningPreEffectStyle = UIElementUtils.GetStyleSheet("FlowGraph_RunningPreEffect");
		runningNodeStyle = UIElementUtils.GetStyleSheet("FlowGraph_RunningNode");

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

		inspector.RegisterCallback<MouseMoveEvent>((e) =>
		{
			if (selectedEffectElement != null)
				selectedEffectElement.RenameButton();
		});

		inspector.Add(propertyField);

		FlowEffectElement.ResetMoveState();

		if (Application.isPlaying)
			RegisterRuntime();
		else
			UnregisterRuntime();
	}

	private void OnDisable()
	{
		UnregisterRuntime();
	}

	public void PlayFromNode(FlowNode node)
	{
		if (FlowManager.Instance != null)
		{
			FlowGraphInstance targetGraph = null;
			FlowNodeInstance targetNode = null;

			foreach (var graphInstance in FlowManager.Instance.ActiveGraphs)
			{
				foreach (var nodeInstance in graphInstance.nodeInstances)
				{
					if (nodeInstance.node == node)
					{
						targetGraph = graphInstance;
						targetNode = nodeInstance;
					}
				}
			}

			if (targetGraph != null)
			{
				targetGraph.Stop();
				targetNode.Play();
			}
		}
	}

	private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
	{
		OnEnable();

		if (stateChange == PlayModeStateChange.EnteredPlayMode)
		{
			RegisterRuntime();
		}
		else if(stateChange == PlayModeStateChange.ExitingPlayMode)
		{
			UnregisterRuntime();
		}
	}

	private void RegisterRuntime()
	{
		if (FlowManager.Instance != null)
		{
			FlowManager.Instance.OnGraphPlayed += GraphInstance_OnPlayed;
			FlowManager.Instance.OnGraphStopped += GraphInstance_OnStopped;

			foreach (var activeGraph in FlowManager.Instance.ActiveGraphs)
			{
				GraphInstance_OnPlayed(activeGraph);

				foreach (var activeNode in activeGraph.ActiveNodes)
				{
					NodeInstance_OnStarted(activeNode);

					foreach (var activeEffect in activeNode.ActiveEffects)
					{
						EffectInstance_OnInvoked(activeEffect);
					}
				}
			}
		}
	}

	private void UnregisterRuntime()
	{
		if (FlowManager.Instance != null)
		{
			FlowManager.Instance.OnGraphPlayed -= GraphInstance_OnPlayed;
			FlowManager.Instance.OnGraphStopped -= GraphInstance_OnStopped;

			foreach (var activeGraph in FlowManager.Instance.ActiveGraphs)
			{
				GraphInstance_OnStopped(activeGraph);

				foreach (var activeNode in activeGraph.ActiveNodes)
				{
					NodeInstance_OnStoppedOrComplete(activeNode);

					foreach (var activeEffect in activeNode.ActiveEffects)
					{
						EffectInstance_OnStoppedOrComplete(activeEffect);
					}
				}
			}
		}
	}

	private void GraphInstance_OnPlayed(FlowGraphInstance graphInstance)
	{
		foreach (var nodeInstance in graphInstance.nodeInstances)
		{
			nodeInstance.OnStarted += NodeInstance_OnStarted;

			nodeInstance.OnStopped += NodeInstance_OnStoppedOrComplete;
			nodeInstance.OnComplete += NodeInstance_OnStoppedOrComplete;

			foreach (var effectInstance in nodeInstance.effectInstances)
			{
				effectInstance.OnInvoked += EffectInstance_OnInvoked;
				effectInstance.OnStarted += EffectInstance_OnStarted;

				effectInstance.OnStopped += EffectInstance_OnStoppedOrComplete;
				effectInstance.OnComplete += EffectInstance_OnStoppedOrComplete;
			}
		}
	}

	private void GraphInstance_OnStopped(FlowGraphInstance graphInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			n.styleSheets.Remove(runningNodeStyle);
			n.Query<FlowEffectElement>().ForEach(e => 
			{
				e.styleSheets.Remove(runningEffectStyle);
			});
		});

		foreach (var nodeInstance in graphInstance.nodeInstances)
		{
			nodeInstance.OnStarted -= NodeInstance_OnStarted;

			nodeInstance.OnStopped -= NodeInstance_OnStoppedOrComplete;
			nodeInstance.OnComplete -= NodeInstance_OnStoppedOrComplete;

			foreach (var effectInstance in nodeInstance.effectInstances)
			{
				effectInstance.OnInvoked -= EffectInstance_OnInvoked;
				effectInstance.OnStarted -= EffectInstance_OnStarted;

				effectInstance.OnStopped -= EffectInstance_OnStoppedOrComplete;
				effectInstance.OnComplete -= EffectInstance_OnStoppedOrComplete;
			}
		}
	}

	private void NodeInstance_OnStarted(FlowNodeInstance nodeInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			if(n.node == nodeInstance.node)
				n.styleSheets.Add(runningNodeStyle);
		});
	}

	private void NodeInstance_OnStoppedOrComplete(FlowNodeInstance nodeInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			if (n.node == nodeInstance.node)
				n.styleSheets.Remove(runningNodeStyle);
		});
	}

	private void EffectInstance_OnInvoked(FlowEffectInstance effectInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			n.Query<FlowEffectElement>().ForEach(e =>
			{
				if (e.effect == effectInstance.effect)
				{
					e.styleSheets.Add(runningPreEffectStyle);
				}
			});
		});
	}

	private void EffectInstance_OnStarted(FlowEffectInstance effectInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			n.Query<FlowEffectElement>().ForEach(e =>
			{
				if (e.effect == effectInstance.effect)
				{
					e.styleSheets.Remove(runningPreEffectStyle);
					e.styleSheets.Add(runningEffectStyle);
				}
			});
		});
	}

	private void EffectInstance_OnStoppedOrComplete(FlowEffectInstance effectInstance)
	{
		rootVisualElement.Query<FlowNodeElement>().ForEach(n =>
		{
			n.Query<FlowEffectElement>().ForEach(e =>
			{
				if (e.effect == effectInstance.effect)
				{
					e.styleSheets.Remove(runningPreEffectStyle);
					e.styleSheets.Remove(runningEffectStyle);
				}
			});
		});
	}

	private void OnSelectionChanged()
	{
		if(Selection.activeObject != flowGraph)
			OnEnable();	
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
		if (selectedEffectElement != null)
			selectedEffectElement.styleSheets.Remove(selectedStyle);

		selectedEffectElement = effectRot;

		if (selectedEffectElement != null)
		{
			var effectProp = selectedEffectElement.FindEffectProperty();
			if (effectProp != null)
			{
				if(!effectProp.isExpanded)
					effectProp.isExpanded = true;

				var iter = effectProp.Copy();
				while (iter.Next(true))
				{
					if(!iter.isExpanded)
						iter.isExpanded = true;
				}

				serializedObject.ApplyModifiedProperties();

				Profiler.BeginSample("BindProperty");

				propertyField.BindProperty(effectProp);

				Profiler.EndSample(); // BindProperty

				var effect = selectedEffectElement.effect;
				string label = string.Format("{0} - {1}", effect.function.module.Replace("FlowModule_", ""), effect.function.function);
				propertyField.label = label;
			}

			selectedEffectElement.styleSheets.Add(selectedStyle);
		}
	}

	private void EffectButton_OnClicked()
	{
		
	}
}
