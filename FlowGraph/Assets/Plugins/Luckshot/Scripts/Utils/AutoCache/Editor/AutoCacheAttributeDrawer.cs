using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(AutoCacheAttribute))]
public class AutoCacheAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (property.hasMultipleDifferentValues)
		{
			EditorGUI.PropertyField(position, property, label);
			return;
		}

		AutoCacheAttribute autoCache = attribute as AutoCacheAttribute;

		if (property.objectReferenceValue == null)
		{
			Component component = property.serializedObject.targetObject as Component;
			if (component != null)
			{
				// TODO: Must be some way to get this to work with arrays
				Component targetComponent = component.gameObject.GetComponent(fieldInfo.FieldType);

				if (targetComponent == null && autoCache.searchChildren)
					targetComponent = component.gameObject.GetComponentInChildren(fieldInfo.FieldType);

				if (targetComponent == null && autoCache.searchAncestors)
					targetComponent = component.gameObject.GetComponentInParent(fieldInfo.FieldType);

				if (targetComponent != null)
					property.objectReferenceValue = targetComponent;
			}
		}

		EditorGUI.PropertyField(position, property, label);
	}
}
