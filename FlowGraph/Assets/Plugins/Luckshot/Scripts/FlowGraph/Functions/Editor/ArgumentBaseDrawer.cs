using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.Profiling;

[CustomPropertyDrawer(typeof(ArgumentBase), true)]
public class ArgumentBaseDrawer : PropertyDrawer
{
	private void SetSourceToTemplate(SerializedProperty sourceProp)
	{
		if (sourceProp.intValue != (int)ArgumentSource.Template)
		{
			sourceProp.intValue = (int)ArgumentSource.Template;
			sourceProp.serializedObject.ApplyModifiedProperties();
		}
	}

	private void SetSourceToValue(SerializedProperty sourceProp)
	{
		if (sourceProp.intValue != (int)ArgumentSource.Value)
		{
			sourceProp.intValue = (int)ArgumentSource.Value;
			sourceProp.serializedObject.ApplyModifiedProperties();
		}
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		property.serializedObject.Update();

		position.height = EditorGUIUtility.singleLineHeight;

		var sourceProp = property.FindPropertyRelative("source");
		if (sourceProp == null)
		{
			EditorGUI.EndProperty();
			return;
		}

		if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && 
			position.Contains(Event.current.mousePosition) &&
			!property.propertyPath.Contains("templateArguments"))
		{
			GenericMenu context = new GenericMenu();
			
			context.AddItem(new GUIContent("SetArgumentType/Template"), 
				sourceProp.intValue == (int)ArgumentSource.Template, 
				() => SetSourceToTemplate(sourceProp));

			context.AddItem(new GUIContent("SetArgumentType/Value"),
				sourceProp.intValue == (int)ArgumentSource.Value, 
				() => SetSourceToValue(sourceProp));

			context.ShowAsContext();
		}

		if (sourceProp.intValue == (int)ArgumentSource.Template)
		{
			position.height = EditorGUIUtility.singleLineHeight;

			List<string> argumentNames = new List<string>();
			List<int> argumentIndices = new List<int>();

			var templateArgsProp = property.serializedObject.FindProperty("templateArguments");

			var typeProp = property.FindPropertyRelative("type");
			Type myType = Type.GetType(typeProp.stringValue);

			var args = (List<ArgumentBase>)EditorUtils.GetTargetObjectOfProperty(templateArgsProp);
			for (int i = 0; i < args.Count; i++)
			{
				var argType = Type.GetType(args[i].type);
				if (argType == myType || argType.IsAssignableFrom(myType))
				{
					argumentNames.Add(args[i].name);
					argumentIndices.Add(i);
				}
			}

			var templateIndexProp = property.FindPropertyRelative("templateIndex");

			if (argumentNames.Count > 0)
			{
				int selectedIndex = Mathf.Max(0, argumentIndices.IndexOf(templateIndexProp.intValue));
				selectedIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, argumentNames.ToArray());

				int selectedTemplateIndex = argumentIndices[selectedIndex];
				if (templateIndexProp.intValue != selectedTemplateIndex)
				{
					templateIndexProp.intValue = selectedTemplateIndex;
					templateIndexProp.serializedObject.ApplyModifiedProperties();
				}
			}
			else
			{
				EditorGUI.LabelField(position, property.displayName + " (template)", "No Match Found");
			}
		}
		else
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var typeProp = property.FindPropertyRelative("type");
				Type type = Type.GetType(typeProp.stringValue);
				if (type != null)
				{
					var argProp = property.FindPropertyRelative("value");
					if (argProp == null && type.IsEnum)
						argProp = property.FindPropertyRelative("enumValue");

					if (argProp != null)
					{
						string displayName = type.Name;

						var easyTypeLabel = new GUIContent(string.Format("{0} ({1})", label.text, displayName));

						Type elementType = null;
						if (type.IsArray)
							elementType = type.GetElementType();

						position.height = EditorGUIUtility.singleLineHeight;

						if (typeof(UnityEngine.Object).IsAssignableFrom(type))
						{
							EditorGUI.ObjectField(position, argProp, type, easyTypeLabel);
						}
						else if (elementType != null &&
							typeof(UnityEngine.Object).IsAssignableFrom(elementType))
						{
							EditorGUI.PropertyField(position, argProp, easyTypeLabel, false);
							position.y += EditorGUI.GetPropertyHeight(argProp, false);

							if (argProp.isExpanded)
							{
								EditorGUI.indentLevel++;

								int arraySize = Mathf.Max(0, EditorGUI.IntField(position, new GUIContent("Size"), argProp.arraySize));
								if (arraySize != argProp.arraySize)
									argProp.arraySize = arraySize;

								position.y += EditorGUIUtility.singleLineHeight;

								for (int i = 0; i < arraySize; i++)
								{
									var elementProp = argProp.GetArrayElementAtIndex(i);
									position.height = EditorGUI.GetPropertyHeight(elementProp, true);

									EditorGUI.ObjectField(position, elementProp, elementType, new GUIContent(elementProp.displayName));
									position.y += position.height;
								}

								EditorGUI.indentLevel--;
							}
						}
						else if (type == typeof(string))
						{
							string text = EditorGUI.TextField(position, easyTypeLabel, argProp.stringValue);
							if (argProp.stringValue != text)
								argProp.stringValue = text;
						}
						else if (type.IsEnum)
						{
							Enum e = EditorGUI.EnumPopup(position, easyTypeLabel, (Enum)Enum.ToObject(type, argProp.intValue));
							int index = Convert.ToInt32(e);

							if (argProp.intValue != index)
								argProp.intValue = index;
						}
						else
						{
							var propertyDrawer = PropertyDrawerFinder.Find(argProp);
							if (propertyDrawer != null)
							{
								position.height = propertyDrawer.GetPropertyHeight(argProp, easyTypeLabel);
								propertyDrawer.OnGUI(position, argProp, easyTypeLabel);

							}
							else
							{
								EditorGUI.PropertyField(position, argProp, easyTypeLabel, true);
							}
						}
					}
					else
					{

					}
				}

				if (GUI.changed)
				{
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = 0f;

		var sourceProp = property.FindPropertyRelative("source");
		if (sourceProp == null)
			return 0f;

		if (sourceProp.intValue == (int)ArgumentSource.Template)
		{
			height = EditorGUIUtility.singleLineHeight;
		}
		else
		{
			var typeProp = property.FindPropertyRelative("type");
			Type type = Type.GetType(typeProp.stringValue);
			if (type == null)
				return 0f;

			var argProp = property.FindPropertyRelative("value");
			if (argProp == null)
				argProp = property.FindPropertyRelative("enumValue");

			if (argProp != null)
			{
				Type elementType = null;
				if (type.IsArray)
					elementType = type.GetElementType();

				if (typeof(UnityEngine.Object).IsAssignableFrom(type))
				{
					height = EditorGUIUtility.singleLineHeight;
				}
				else if (elementType != null &&
					typeof(UnityEngine.Object).IsAssignableFrom(elementType))
				{
					height += EditorGUI.GetPropertyHeight(argProp, false);

					if (argProp.isExpanded)
					{
						height += EditorGUIUtility.singleLineHeight;

						for (int i = 0; i < argProp.arraySize; i++)
						{
							var elementProp = argProp.GetArrayElementAtIndex(i);
							height += EditorGUI.GetPropertyHeight(elementProp, true);
						}
					}
				}
				else if (type == typeof(string))
				{
					height += EditorGUIUtility.singleLineHeight;
				}
				else if (type.IsEnum)
				{
					height += EditorGUIUtility.singleLineHeight;
				}
				else
				{
					var propertyDrawer = PropertyDrawerFinder.Find(argProp);
					if (propertyDrawer != null)
					{
						height += propertyDrawer.GetPropertyHeight(argProp, new GUIContent());
					}
					else
					{
						height += EditorGUI.GetPropertyHeight(argProp, true);
					}
				}
			}
		}

		return height;
	}
}