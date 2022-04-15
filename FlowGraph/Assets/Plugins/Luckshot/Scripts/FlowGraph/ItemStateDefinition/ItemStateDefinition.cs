using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStateDefinition
{
    public enum ListEvaluateMode
    {
        AnyTrue,
        AllTrue
    }

    [NonReorderable]
    public List<PropertyItemStateDefinition> propertyStateDefinitions = new List<PropertyItemStateDefinition>();

    public ListEvaluateMode evaluateMode = ListEvaluateMode.AllTrue;

    public ItemStateDefinition(ItemStateDefinition other)
    {
        propertyStateDefinitions.Clear();
        for(int i = 0; i < other.propertyStateDefinitions.Count; i++)
            propertyStateDefinitions.Add(new PropertyItemStateDefinition(other.propertyStateDefinitions[i]));

        evaluateMode = other.evaluateMode;
    }

    public ItemStateDefinition GetCopyWithDirectArguments(FlowTemplate source, List<ArgumentBase> overrideArguments = null)
    {
        ItemStateDefinition itemStateDefinition = new ItemStateDefinition(this);
        foreach (var propertyStateDefinition in itemStateDefinition.propertyStateDefinitions)
        {
            foreach (var argument in propertyStateDefinition.arguments)
            {
                argument.Value = argument.GetValueFromSource(source, overrideArguments);
                argument.source = (int)ArgumentSource.Value;
            }
        }

        return itemStateDefinition;
    }

	public bool CheckState(Item item)
    {
        if (propertyStateDefinitions.Count == 0)
            return true;

        for(int i = 0; i < propertyStateDefinitions.Count; i++)
        {
            bool conditionTrue = propertyStateDefinitions[i].CheckState(item);

            if (conditionTrue)
            {
                if (evaluateMode == ListEvaluateMode.AnyTrue)
                    return true;
            }
            else
            {
                if (evaluateMode == ListEvaluateMode.AllTrue)
                    return false;
            }
        }

        if (evaluateMode == ListEvaluateMode.AllTrue)
            return true;
        else if (evaluateMode == ListEvaluateMode.AnyTrue)
            return false;
        else
        {
            Debug.LogError("This shouldn't be possible", item);
            return false;
        }
    }
}
