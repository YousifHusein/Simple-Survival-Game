using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;              // Reference to the item in the slot
    public int quantity;           // Quantity of the item (used if stackable)

    public InventorySlot(Item newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public void AddToStack(int amount)
    {
        if (item != null && item.isStackable)
        {
            quantity += amount;
            if (quantity > item.maxStackSize)
            {
                quantity = item.maxStackSize; // Limit to max stack size
            }
        }
    }

    public void RemoveFromStack(int amount)
    {
        if (item != null)
        {
            quantity -= amount;
            if (quantity <= 0)
            {
                ClearSlot();
            }
        }
    }

    public void ClearSlot()
    {
        item = null;
        quantity = 0;
    }
}
