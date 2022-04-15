using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ItemStateDefinition))]
public class ItemStateDefinitionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        position.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(position, property, label, true);

        if(EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }    
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight * 1f;
        if (property.isExpanded)
        {
            height += EditorGUIUtility.singleLineHeight * 2f;

            var stateDefinitionsProp = property.FindPropertyRelative("propertyStateDefinitions");
            if (stateDefinitionsProp.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight * 2f;

                for (int i = 0; i < stateDefinitionsProp.arraySize; i++)
                {
                    var stateDefinitionProp = stateDefinitionsProp.GetArrayElementAtIndex(i);
                    height += EditorGUI.GetPropertyHeight(stateDefinitionProp, stateDefinitionsProp.isExpanded);
                }
            }
        }

        return height;
    }
}
