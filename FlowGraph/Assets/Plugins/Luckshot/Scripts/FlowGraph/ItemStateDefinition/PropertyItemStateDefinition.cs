using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[System.Serializable]
public class PropertyItemStateDefinition
{
	public string typeName = string.Empty;
    public string memberName = string.Empty;

    [NonReorderable]
    [SerializeReference]
    public List<ArgumentBase> arguments = new List<ArgumentBase>();

    public bool desiredResultBool = true;

    public float desiredResultFloat = 0f;
    public CompareType desiredResultFloatCompare = CompareType.Equal;

    public PropertyItemStateDefinition(PropertyItemStateDefinition other)
    {
        typeName = other.typeName;
        memberName = other.memberName;

        arguments.Clear();
        for (int i = 0; i < other.arguments.Count; i++)
            arguments.Add((ArgumentBase)other.arguments[i].Clone());

        desiredResultBool = other.desiredResultBool;
        desiredResultFloat = other.desiredResultFloat;
        desiredResultFloatCompare = other.desiredResultFloatCompare;
    }

    public PropertyItemStateDefinition GetCopyWithDirectArguments(FlowTemplate source, List<ArgumentBase> overrideArguments = null)
    {
        PropertyItemStateDefinition propertyItemStateDefinition = new PropertyItemStateDefinition(this);
        foreach (var argument in propertyItemStateDefinition.arguments)
        {
            argument.Value = argument.GetValueFromSource(source, overrideArguments);
            argument.source = (int)ArgumentSource.Value;
        }

        return propertyItemStateDefinition;
    }

    public bool CheckState(Item item)
    {
        object returnValue = Invoke(item);
        if (returnValue == null)
            return false;

        Type returnType = returnValue.GetType();

        if( returnType == typeof(bool) ||
            returnType == typeof(LensManagerBool))
        {
            bool boolValue = (bool)returnValue;
            return boolValue == desiredResultBool;
        }
        else if(returnType == typeof(float) ||
            returnType == typeof(LensManagerFloat))
        {
            float floatValue = (float)returnValue;
            return CompareUtils.CompareTo(floatValue, desiredResultFloatCompare, desiredResultFloat);
        }

        return true;
    }

    public object GetDefault(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }

    public object Invoke(Item item)
    {
        object returnValue = null;

        Type propertyType = FlowTypeCache.GetPropertyType(typeName);
        if (propertyType != null)
        {
            MemberInfo memberInfo = null;

            PropertyItem propertyComponent = item.GetProperty(propertyType);
            if(propertyComponent != null)
                memberInfo = FlowTypeCache.GetPropertyItemMember(propertyComponent.GetType(), memberName);
            
            if (memberInfo != null)
            {
                MethodInfo methodInfo = memberInfo as MethodInfo;
                if (methodInfo != null)
                {
                    if (propertyComponent == null)
                    {
                        returnValue = GetDefault(methodInfo.ReturnType);
                    }
                    else
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        int nonItemIndex = 0;
                        object[] parameterObjs = new object[parameters.Length];
                        for (int i = 0; i < parameterObjs.Length; i++)
                        {
                            if (parameters[i].ParameterType == typeof(Item))
                            {
                                parameterObjs[i] = item;
                            }
                            else
                            {
                                parameterObjs[i] = arguments[nonItemIndex].Value;
                                nonItemIndex++;
                            }
                        }

                        returnValue = methodInfo.Invoke(propertyComponent, parameterObjs);
                    }
                }
                else
                {
                    FieldInfo fieldInfo = memberInfo as FieldInfo;
                    if(fieldInfo != null)
                    {
                        if (propertyComponent == null)
                            returnValue = GetDefault(fieldInfo.FieldType);
                        else
                            returnValue = fieldInfo.GetValue(propertyComponent);
                    }
                    else
                    {
                        PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                        if(propertyInfo != null)
                        {
                            if (propertyComponent == null)
                                returnValue = GetDefault(propertyInfo.PropertyType);
                            else
                                returnValue = propertyInfo.GetValue(propertyComponent);
                        }
                    }
                }
            }
        }

        return returnValue;
    }
}
