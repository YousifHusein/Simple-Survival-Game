// InventorySlot represents a slot in the inventory (or hotbar)
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static UnityEditor.Progress;
using System;
using UnityEditorInternal.Profiling.Memory.Experimental;

public enum SlotTag { None, Head, Chest, Legs, Feet, Tool }

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public InventoryItem myItem { get; set; }  // The item currently in this slot
    public SlotTag myTag;  // The tag representing the type of slot (e.g., Head, Chest)
    public Inventory inventory;

    public void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    // Handle clicks on the inventory slot
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Inventory.carriedItem == null) return;
            if (myTag != SlotTag.None && Inventory.carriedItem.myItem.itemTag != myTag) return;

            Debug.Log("Im getting her eon left click");
            CombineOrSetItem(Inventory.carriedItem);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Inventory.carriedItem != null)
            {
                // If the slot is empty or compatible, add one item from the carried stack
                if (myItem == null || (myItem.myItem == Inventory.carriedItem.myItem && myItem.currentStackSize < myItem.myItem.maxStackSize))
                {
                    if (myItem == null)
                    {
                        // Create a new item in this slot with 1 stack
                        InventoryItem newItem = Instantiate(Inventory.Singleton.itemPrefab, transform);
                        newItem.Initialize(Inventory.carriedItem.myItem, this, 1);

                        string uniqueName = Inventory.carriedItem.myItem.name + "_RightClick_" + DateTime.Now.Ticks;
                        newItem.name = uniqueName;

                        // Reduce the carried item stack by 1
                        Inventory.carriedItem.currentStackSize -= 1;
                        Inventory.carriedItem.UpdateStackSizeText();

                        myItem = newItem;  // Set the new item in this slot

                        Inventory.Singleton.items.Add(new Items(newItem.myItem, newItem));
                    }
                    else
                    {
                        // Add one item to the existing stack
                        myItem.AddToStack(1);

                        // Reduce the carried item stack by 1
                        Inventory.carriedItem.currentStackSize -= 1;
                        Inventory.carriedItem.UpdateStackSizeText();
                    }

                    // Destroy the carried item if its stack size reaches zero
                    if (Inventory.carriedItem.currentStackSize <= 0)
                    {
                        Destroy(Inventory.carriedItem.gameObject);
                        Inventory.carriedItem = null;
                    }
                }
            }
        }
    }

    // Combine the carried item with the item in this slot, or set the item in the slot
    public void CombineOrSetItem(InventoryItem item)
    {
        // If it's the same item, return it to the slot
        if (myItem == item)
        {
            item.transform.SetParent(transform);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            item.canvasGroup.blocksRaycasts = true;
            Inventory.carriedItem = null;
            return;
        }

        // If the slot is empty, set the item in the slot
        if (myItem == null)
        {
            SetItem(item);
            return;
        }

        // If the items are the same and stackable, combine them
        if (myItem != null && myItem.myItem == item.myItem && myItem.myItem.isStackable)
        {
            Debug.Log("Im getting here I think");
            int availableSpace = myItem.myItem.maxStackSize - myItem.currentStackSize;

            if (item.currentStackSize <= availableSpace)
            {
                Debug.Log("IM GETTING HERE");
                myItem.AddToStack(item.currentStackSize);
                Debug.Log("New inventory Item Stack: " + myItem.currentStackSize);
                Debug.Log("New item stack: " + myItem.myItem.currentStack);
                Destroy(item.gameObject);
                Inventory.carriedItem = null;
            }
            else
            {
                Debug.Log("IM GETTING HERE TOO");
                myItem.AddToStack(availableSpace);

                item.currentStackSize -= availableSpace;
                item.UpdateStackSizeText();
            }

            return;
        }

        // If the items are different, swap them
        SetItem(item);
    }

    // Set the item in this slot
    public void SetItem(InventoryItem item)
    {
        Inventory.carriedItem = null;

        if (item == null)
        {
            Debug.LogError("SetItem called with null item!");
        }

        if (item.activeSlot != null)
        {
            item.activeSlot.myItem = null;
        }

        // Set current slot
        myItem = item;
        myItem.activeSlot = this;
        myItem.transform.SetParent(transform);
        //inventory.AddItem(item.myItem, item);

        RectTransform itemRect = myItem.GetComponent<RectTransform>();
        RectTransform slotRect = GetComponent<RectTransform>();

        if (itemRect != null && slotRect != null)
        {
            // Snap the item to the center of the slot (UI setup)
            itemRect.anchoredPosition = Vector2.zero;  // Center it in the slot
            itemRect.sizeDelta = slotRect.sizeDelta;   // Optionally match the size of the slot
        }
        else
        {
            // If not using RectTransform, snap using local position (for non-UI setup)
            myItem.transform.localPosition = Vector3.zero;  // Center the item within the slot
        }

        myItem.canvasGroup.blocksRaycasts = true;

        // If this is an equipment slot, equip the item
        if (myTag != SlotTag.None)
        { Inventory.Singleton.EquipEquipment(myTag, myItem); }
    }
}
