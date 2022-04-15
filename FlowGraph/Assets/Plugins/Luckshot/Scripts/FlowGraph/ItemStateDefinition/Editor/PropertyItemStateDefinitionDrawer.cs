using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;

[CustomPropertyDrawer(typeof(PropertyItemStateDefinition))]
public class PropertyItemStateDefinitionDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		position.height = EditorGUIUtility.singleLineHeight;

		EditorGUI.PropertyField(position, property, label, false);

		if (property.isExpanded)
		{
			position.y += EditorGUIUtility.singleLineHeight;

			EditorGUI.indentLevel++;

			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 100f;

			// Type Name
			var typeProp = property.FindPropertyRelative("typeName");

			List<Type> propertyTypeNames = FlowTypeCache.GetPropertyItemTypes();
			List<string> propertyItemNames = FlowTypeCache.GetPropertyItemNames();

			int typeIndex = propertyItemNames.IndexOf(typeProp.stringValue);
			if (typeIndex == -1)
				typeIndex = 0;

			EditorGUI.BeginChangeCheck();
			typeIndex = EditorGUI.Popup(position, typeProp.displayName, typeIndex, propertyItemNames.ToArray());
			string propertyName = propertyItemNames[typeIndex];

			if (EditorGUI.EndChangeCheck() || typeProp.stringValue != propertyName)
			{
				typeProp.stringValue = propertyName;
				property.serializedObject.ApplyModifiedProperties();
			}

			// Member Name
			var memberProp = property.FindPropertyRelative("memberName");

			Type propertyType = propertyTypeNames[typeIndex];

			List<string> memberNames = FlowTypeCache.GetPropertyItemMemberNames(propertyType);

			int memberIndex = memberNames.IndexOf(memberProp.stringValue);
			if (memberIndex == -1)
				memberIndex = 0;

			EditorGUI.BeginChangeCheck();

			position.y += EditorGUIUtility.singleLineHeight;
			memberIndex = EditorGUI.Popup(position, memberProp.displayName, memberIndex, memberNames.ToArray());
			string memberName = memberNames[memberIndex];

			if (EditorGUI.EndChangeCheck() || memberProp.stringValue != memberName)
			{
				memberProp.stringValue = memberName;
				property.serializedObject.ApplyModifiedProperties();
			}

			EditorGUIUtility.labelWidth = prevLabelWidth;

			// Args

			MemberInfo memberInfo = FlowTypeCache.GetPropertyItemMember(propertyType, memberNames[memberIndex]);

			MethodInfo methodInfo = memberInfo as MethodInfo;
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			FieldInfo fieldInfo = memberInfo as FieldInfo;

			var argsProp = property.FindPropertyRelative("arguments");

			List<ParameterInfo> parameters = new List<ParameterInfo>();

			if (methodInfo != null)
			{
				var methodParams = methodInfo.GetParameters();
				foreach (var methodParam in methodParams)
				{
					if (methodParam.ParameterType != typeof(Item))
						parameters.Add(methodParam);
				}
			}

			bool matchingParameters = parameters.Count == argsProp.arraySize;
			if (matchingParameters)
			{
				for (int i = 0; i < argsProp.arraySize; i++)
				{
					var argProp = argsProp.GetArrayElementAtIndex(i);
					ArgumentBase argValue = (ArgumentBase)EditorUtils.GetTargetObjectOfProperty(argProp);
					if (argValue.Value.GetType() != parameters[i].ParameterType)
					{
						matchingParameters = false;
						break;
					}
				}
			}

			if (!matchingParameters)
			{
				var existingArgs = (List<ArgumentBase>)EditorUtils.GetTargetObjectOfProperty(argsProp);
				if (existingArgs == null)
					existingArgs = new List<ArgumentBase>();

				List<Type> types = new List<Type>();
				List<ArgumentBase> arguments = new List<ArgumentBase>();

				for (int i = 0; i < parameters.Count; i++)
				{
					var argument = ArgumentHelper.GetArgumentOfType(parameters[i].ParameterType);
					argument.name = parameters[i].Name;

					if (parameters[i].HasDefaultValue)
						argument.Value = parameters[i].DefaultValue;

					types.Add(parameters[i].ParameterType);
					arguments.Add(argument);
				}

				// Retain existing arguments if new arguments are same type
				for (int i = 0; i < arguments.Count && i < existingArgs.Count; i++)
				{
					if (existingArgs[i] != null && arguments[i] != null &&
						existingArgs[i].Value != null &&
						existingArgs[i].type == arguments[i].type)
					{
						arguments[i].Value = existingArgs[i].Value;
					}
				}

				argsProp.ClearArray();
				while (argsProp.arraySize < arguments.Count)
					argsProp.InsertArrayElementAtIndex(argsProp.arraySize);

				for (int i = 0; i < argsProp.arraySize; i++)
				{
					var elementProp = argsProp.GetArrayElementAtIndex(i);
					elementProp.managedReferenceValue = arguments[i];
				}

				argsProp.serializedObject.ApplyModifiedProperties();
			}

			// shouldn't need this
			//argsProp = property.FindPropertyRelative("arguments");

			position.y += EditorGUIUtility.singleLineHeight;

			if (argsProp.arraySize > 0)
			{
				EditorGUI.PropertyField(position, argsProp, false);

				position.y += EditorGUIUtility.singleLineHeight;

				if (argsProp.isExpanded)
				{
					EditorGUI.indentLevel++;

					for (int i = 0; i < argsProp.arraySize; i++)
					{
						var argProp = argsProp.GetArrayElementAtIndex(i);

						position.height = EditorGUI.GetPropertyHeight(argProp, true);
						EditorGUI.PropertyField(position, argProp, true);

						position.y += position.height;
					}

					EditorGUI.indentLevel--;
				}
			}

			// DESIRED RESULT

			position.height = EditorGUIUtility.singleLineHeight;

			Type returnType = typeof(float);

			if (methodInfo != null)
				returnType = methodInfo.ReturnType;
			else if (fieldInfo != null)
				returnType = fieldInfo.FieldType;
			else if (propertyInfo != null)
				returnType = propertyInfo.PropertyType;

			if (returnType == typeof(bool) ||
				returnType == typeof(LensManagerBool))
			{
				EditorGUI.BeginChangeCheck();

				var resultBoolProp = property.FindPropertyRelative("desiredResultBool");

				bool toggle = EditorGUI.Toggle(position, resultBoolProp.displayName, resultBoolProp.boolValue);

				if (EditorGUI.EndChangeCheck())
				{
					resultBoolProp.boolValue = toggle;
					resultBoolProp.serializedObject.ApplyModifiedProperties();
				}
			}
			else if (returnType == typeof(float) ||
					returnType == typeof(LensManagerFloat))
			{
				EditorGUI.BeginChangeCheck();

				var resultFloatProp = property.FindPropertyRelative("desiredResultFloat");
				float floatValue = EditorGUI.FloatField(position, "desired Result", resultFloatProp.floatValue);

				if (EditorGUI.EndChangeCheck())
				{
					resultFloatProp.floatValue = floatValue;
					resultFloatProp.serializedObject.ApplyModifiedProperties();
				}

				position.y += EditorGUIUtility.singleLineHeight;

				EditorGUI.BeginChangeCheck();

				var resultFloatCompareProp = property.FindPropertyRelative("desiredResultFloatCompare");
				CompareType compare = (CompareType)EditorGUI.EnumPopup(position, "Compare Type", (CompareType)resultFloatCompareProp.enumValueIndex);

				if (EditorGUI.EndChangeCheck())
				{
					resultFloatCompareProp.enumValueIndex = (int)compare;
					resultFloatCompareProp.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			height += EditorGUIUtility.singleLineHeight * 2f;

			var argsProp = property.FindPropertyRelative("arguments");

			if (argsProp.arraySize > 0)
			{
				height += EditorGUIUtility.singleLineHeight;

				if (argsProp.isExpanded)
				{
					for (int i = 0; i < argsProp.arraySize; i++)
					{
						var argProp = argsProp.GetArrayElementAtIndex(i);
						height += EditorGUI.GetPropertyHeight(argProp, true);
					}
				}
			}

			List<Type> propertyTypeNames = FlowTypeCache.GetPropertyItemTypes();
			List<string> propertyItemNames = FlowTypeCache.GetPropertyItemNames();

			var typeProp = property.FindPropertyRelative("typeName");
			int typeIndex = propertyItemNames.IndexOf(typeProp.stringValue);
			if (typeIndex == -1)
				typeIndex = 0;

			var memberProp = property.FindPropertyRelative("memberName");

			Type propertyType = propertyTypeNames[typeIndex];

			List<string> memberNames = FlowTypeCache.GetPropertyItemMemberNames(propertyType);
			int memberIndex = memberNames.IndexOf(memberProp.stringValue);
			if (memberIndex == -1)
				memberIndex = 0;

			// Args

			MemberInfo memberInfo = FlowTypeCache.GetPropertyItemMember(propertyType, memberNames[memberIndex]);

			MethodInfo methodInfo = memberInfo as MethodInfo;
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			FieldInfo fieldInfo = memberInfo as FieldInfo;

			Type returnType = typeof(float);

			if (methodInfo != null)
				returnType = methodInfo.ReturnType;
			else if (fieldInfo != null)
				returnType = fieldInfo.FieldType;
			else if (propertyInfo != null)
				returnType = propertyInfo.PropertyType;

			if (returnType == typeof(bool) ||
				returnType == typeof(LensManagerBool))
			{
				height += EditorGUIUtility.singleLineHeight;
			}
			else if (returnType == typeof(float) ||
				returnType == typeof(LensManagerFloat))
			{
				height += EditorGUIUtility.singleLineHeight * 2f;
			}
		}

		return height;
	}
}
