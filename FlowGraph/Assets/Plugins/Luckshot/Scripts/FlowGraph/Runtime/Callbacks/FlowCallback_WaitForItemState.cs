using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowCallback_WaitForItemState : FlowCallback
{
    private Item item = null;

    private ItemStateDefinition itemStateDefinition = null;
    private PropertyItemStateDefinition propertyStateDefinition = null;

    private float checkInterval = 0f;

    public FlowCallback_WaitForItemState(FlowEffectInstance effect, Item item, 
        ItemStateDefinition itemStateDefinition,
        PropertyItemStateDefinition propertyStateDefinition, 
        float checkInterval) : base(effect)
    {
        this.item = item;
        this.itemStateDefinition = itemStateDefinition;
        this.propertyStateDefinition = propertyStateDefinition;
        this.checkInterval = checkInterval;

        item.OnStateChanged += Item_OnStateChanged;
        item.OnItemDestroyed += Item_OnItemDestroyed;

        if(checkInterval > 0f)
            TimeManager.Invoke(CheckStateRepeating, checkInterval);
    }

    private void CheckStateRepeating()
    {
        if(!CheckState())
            TimeManager.Invoke(CheckStateRepeating, checkInterval);
    }

    private void Item_OnItemDestroyed(Item item)
    {
        Cleanup();
        Cancel();
    }

    private void Item_OnStateChanged(Item item, PropertyItem propertyItem)
    {
        CheckState();
    }

    private bool CheckState()
    {
        bool readyToComplete = false;

        if (itemStateDefinition != null &&
            itemStateDefinition.CheckState(item))
        {
            readyToComplete = true;
        }
        else if (propertyStateDefinition != null &&
            propertyStateDefinition.CheckState(item))
        {
            readyToComplete = true;
        }

        if(readyToComplete)
        {
            Cleanup();
            Complete();

            return true;
        }

        return false;
    }

    public override void Cancel()
    {
        base.Cancel();
        Cleanup();
    }

    private void Cleanup()
    {
        if (item != null)
        {
            item.OnStateChanged -= Item_OnStateChanged;
            item.OnItemDestroyed -= Item_OnItemDestroyed;
        }
    }
}
