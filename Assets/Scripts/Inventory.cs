using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    // Singleton instance of the Inventory to allow easy access
    public static Inventory Singleton;
    // The currently carried item (the one being dragged by the player)
    public static InventoryItem carriedItem;

    // Array of all the inventory slots and hotbar slots
    [SerializeField] public InventorySlot[] inventorySlots;
    [SerializeField] InventorySlot[] hotbarSlots;

    // Array of equipment slots for different body parts (0=Head, 1=Chest, 2=Legs, 3=Feet)
    [SerializeField] InventorySlot[] equipmentSlots;

    // Transform to hold items that are being dragged
    [SerializeField] public Transform draggablesTransform;
    // Prefab for inventory items
    [SerializeField] public InventoryItem itemPrefab;

    // List of all possible items
    [Header("Item List")]
    //[SerializeField] public List<Tuple<Item, InventoryItem>> items = new List<Tuple<Item, InventoryItem>>();
    public List<Items> items = new List<Items>();

    // Debug button to spawn items manually
    [Header("Debug")]
    [SerializeField] Button giveItemBtn;

    // UI panel for the inventory
    [SerializeField] GameObject inventoryPanel;

    // Stores the original slot of the item being dragged
    private InventorySlot originalSlot;

    private bool isSwapping = false;
    private GameObject player;

    //private Item[] items;

    // Set Singleton reference when the script wakes up
    void Awake()
    {
        Singleton = this;
        player = GameObject.FindWithTag("Player");
    }

    // Update runs every frame
    void Update()
    {
        // If there is a carried item, move it with the mouse
        if (carriedItem == null) return;

        carriedItem.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject uiElement = GetUIElementUnderPointer();
            if (uiElement != null)
            {
                Debug.Log("Pointer is over: " + uiElement.name);
            }
            else
            {
                Debug.Log("Pointer is not over any UI element");
                DropCarriedItem();
            }
        }
    }

    // Set the item that the player is currently carrying
    public void SetCarriedItem(InventoryItem item)
    {
        // If there is already a carried item
        if (carriedItem != null)
        {
            // If the player is returning the item to its original slot
            if (originalSlot != null && originalSlot == item.activeSlot)
            {
                originalSlot.SetItem(carriedItem);

                // Return the carried item to its original slot
                if (carriedItem != null && originalSlot != null)
                {
                    carriedItem.transform.SetParent(originalSlot.transform);
                    carriedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    carriedItem.canvasGroup.blocksRaycasts = true;

                    carriedItem = null;
                    originalSlot = null; // Reset the original slot
                    return;
                }
                else
                {
                    return;
                }
            }

            // If the clicked slot has an item, swap it with the carried item
            if (item.activeSlot != null && item.activeSlot.myItem != null && !isSwapping)
            {
                // Block further interactions during the swap process
                isSwapping = true;

                InventoryItem itemInSlot = item.activeSlot.myItem;

                bool isStackable = carriedItem.myItem.isStackable;
                bool isStackFull = carriedItem.currentStackSize >= carriedItem.myItem.maxStackSize;
                bool isHoveredStackFull = itemInSlot.currentStackSize >= itemInSlot.myItem.maxStackSize;

                // Check if the items are the same and stackable to merge them
                if (carriedItem.myItem == itemInSlot.myItem && carriedItem.myItem.isStackable)
                {
                    // Calculate available space in the target stack
                    int availableSpace = itemInSlot.myItem.maxStackSize - itemInSlot.currentStackSize;

                    if (availableSpace > 0)
                    {
                        // Add as many items as possible to the target stack
                        int itemsToAdd = Mathf.Min(availableSpace, carriedItem.currentStackSize);
                        Debug.Log("Got here for stacking");
                        itemInSlot.AddToStack(itemsToAdd);  // Add items to the stack in the slot
                        carriedItem.currentStackSize -= itemsToAdd;  // Reduce the carried item stack

                        carriedItem.UpdateStackSizeText();  // Update the UI text for carried item

                        // If the carried item's stack is now empty, destroy the carried item
                        if (carriedItem.currentStackSize <= 0)
                        {
                            Destroy(carriedItem.gameObject);  // Remove the carried item from the game
                            carriedItem = null;
                            originalSlot = null;  // Clear original slot reference
                        }

                        // Reset input debounce flag after merging is done
                        isSwapping = false;
                        return;  // Exit here as merging is done
                    }
                    else
                    {
                        Debug.Log("No space available in the target stack.");
                    }

                    isSwapping = false;

                    return;
                }

                // If items cannot be merged, proceed with swapping the items if they are different
                if (carriedItem != null && item.activeSlot != null && carriedItem.name != itemInSlot.name)
                {
                    // Ensure that the slot transform is valid
                    if (item.activeSlot.transform == null)
                    {
                        isSwapping = false;  // Reset debounce flag
                        return;
                    }

                    // Store reference to the carried item's current slot
                    InventorySlot originalSlot = carriedItem.activeSlot;

                    // Place the carried item into the new slot
                    carriedItem.transform.SetParent(item.activeSlot.transform);
                    carriedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    carriedItem.canvasGroup.blocksRaycasts = true;
                    item.activeSlot.SetItem(carriedItem);

                    // Clear the carried item from its original slot
                    if (originalSlot != null && originalSlot.myItem == carriedItem)
                    {
                        originalSlot.myItem = null;  // Clear the original slot only if it still holds the carried item
                    }

                    // Now carry the item that was previously in the slot (swap)
                    carriedItem = itemInSlot;
                    carriedItem.activeSlot = originalSlot;  // Set the active slot for the carried item back to the original slot

                    // Make sure carriedItem is valid after the swap
                    if (carriedItem != null && carriedItem.transform != null)
                    {
                        carriedItem.transform.SetParent(draggablesTransform);
                        carriedItem.canvasGroup.blocksRaycasts = false;
                    }
                    else
                    {
                        Debug.LogError("Error: carriedItem is null or does not have a transform after the swap.");
                    }

                    // Reset the original slot after swapping
                    originalSlot = null;

                    // Reset input debounce flag after swapping
                    isSwapping = false;
                    return;
                }

                // Reset input debounce flag if the swapping conditions were not met
                isSwapping = false;
            }

            // If the slot is empty or valid for the carried item
            if (item.activeSlot.myTag == SlotTag.None || item.activeSlot.myTag == carriedItem.myItem.itemTag)
            {
                if (item.activeSlot != null)
                {
                    if (item.activeSlot.myItem != null)
                    {
                        Debug.Log("Item name: " + item.activeSlot.myItem.name);
                    }
                    else
                    {
                        // You can add logic here to handle empty slots
                        // For example, you could allow placing the carried item into the empty slot
                        item.activeSlot.SetItem(carriedItem);
                        carriedItem = null;  // Clear the carried item after placing
                        originalSlot = null;  // Reset the original slot
                    }
                }
                else
                {
                    Debug.Log("Error: item.activeSlot is null.");
                }
            }

            return;
        }

        // If not carrying an item, pick up the clicked item
        if (originalSlot == null && item.activeSlot != null)
        {
            originalSlot = item.activeSlot;
            carriedItem = item;
        }

        // Equip item if it belongs to an equipment slot
        if (item.activeSlot != null && item.activeSlot.myTag != SlotTag.None)
        {
            EquipEquipment(item.activeSlot.myTag, null);
        }

        carriedItem = item;

        // Ensure the carried item is valid before proceeding
        if (carriedItem == null || carriedItem.canvasGroup == null)
        {
            Debug.LogError("Error: carriedItem or carriedItem.canvasGroup is null when picking up the item.");
            return;
        }

        // Reset scale and position of the carried item
        RectTransform itemRect = carriedItem.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            itemRect.localScale = Vector3.one;
            itemRect.anchoredPosition = Vector2.zero;
        }

        // Disable raycasts while carrying the item
        carriedItem.canvasGroup.blocksRaycasts = false;
        item.transform.SetParent(draggablesTransform);
    }

    // Equip an item based on its slot type (head, chest, etc.)
    public void EquipEquipment(SlotTag tag, InventoryItem item = null)
    {
        switch (tag)
        {
            case SlotTag.Head:
                if (item == null)
                {
                    // Code to remove the equipped head item
                }
                else
                {
                    // Code to equip a new head item
                }
                break;
            case SlotTag.Chest:
                break;
            case SlotTag.Legs:
                break;
            case SlotTag.Feet:
                break;
        }
    }

    // Spawn a new inventory item in the inventory
    public void SpawnInventoryItem(Item item, int stackSize = 1)
    {
        Item _item = item;
        if (_item == null) return;

        CanvasGroup canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        bool wasInventoryClosed = false;

        // Temporarily open the inventory if it's closed
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            wasInventoryClosed = true;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        // Check for an existing stackable item to add to
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];

            if (slot.myItem != null && slot.myItem.myItem == item && item.isStackable)
            {
                if (!slot.myItem.IsStackFull())
                {
                    // Add items to the existing stack if there is space
                    int spaceLeft = item.maxStackSize - slot.myItem.currentStackSize;
                    int amountToAdd = Mathf.Min(spaceLeft, stackSize);
                    slot.myItem.AddToStack(amountToAdd);

                    slot.myItem.UpdateStackSizeText();

                    if (wasInventoryClosed)
                    {
                        ResetInventoryPanel(canvasGroup);
                    }
                    return;
                }
            }
        }

        // Find an empty slot for the item
        InventorySlot emptySlot = FindEmptySlot();

        if (emptySlot != null)
        {
            // Instantiate a new item in the empty slot
            InventoryItem newItem = Instantiate(itemPrefab, emptySlot.transform);

            newItem.Initialize(item, emptySlot, stackSize);

            // Set the item's position and scale to fit the slot
            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            RectTransform slotRect = emptySlot.GetComponent<RectTransform>();

            newItem.AddComponent<Animator>();
            newItem.GetComponent<Animator>().runtimeAnimatorController = newItem.controller;

            if (itemRect != null && slotRect != null)
            {
                itemRect.sizeDelta = slotRect.sizeDelta;
                itemRect.localScale = Vector3.one;
                itemRect.anchoredPosition = Vector2.zero;
            }

            newItem.UpdateStackSizeText();
            Debug.Log("I got here: Adding item to item List: " + item);
            this.AddItem(newItem.myItem, newItem);

            if (wasInventoryClosed)
            {
                ResetInventoryPanel(canvasGroup);
            }
        }
    }

    // Find an empty slot in the inventory or hotbar
    public InventorySlot FindEmptySlot()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.myItem == null)
            {
                return slot;
            }
        }

        foreach (InventorySlot slot in hotbarSlots)
        {
            if (slot.myItem == null)
            {
                return slot;
            }
        }

        return null;
    }

    // Reset the inventory panel after spawning an item
    private void ResetInventoryPanel(CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        inventoryPanel.SetActive(false);
    }

    public GameObject GetUIElementUnderPointer()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition // Get the current mouse position
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            // Return the topmost UI element under the pointer
            return raycastResults[0].gameObject;
        }

        return null; // No UI element under the pointer
    }

    void DropCarriedItem()
    {
        Vector3 dropPosition = player.transform.position + player.transform.forward * 2;
        GameObject droppedItem = Instantiate(Inventory.carriedItem.myItem.itemModel, dropPosition, Quaternion.identity);
        PickupItem pickupComponent = droppedItem.AddComponent<PickupItem>();

        pickupComponent.item = Inventory.carriedItem.myItem;
        pickupComponent.amount = Inventory.carriedItem.currentStackSize;

        // Optionally adjust item's properties (e.g., scale, physics)
        droppedItem.transform.localScale = new Vector3(1f, 1f, 1f);

        // Add a Collider if the model doesn't already have one
        Collider blockCollider = droppedItem.GetComponent<Collider>();
        if (blockCollider == null)
        {
            blockCollider = droppedItem.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
        }

        droppedItem.AddComponent<MeshCollider>();
        droppedItem.GetComponent<MeshCollider>().convex = true;
        blockCollider.isTrigger = true; // Ensure it's a solid collider

        Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        pickupComponent.canPickup = false;
        StartCoroutine(EnablePickupAfterDelay(pickupComponent, 1f));
        pickupComponent.canPickup = true;

        Debug.Log("Dropped item: " + Inventory.carriedItem.myItem.name);

        // Clean up the carried item in the inventory
        //items.Remove(Inventory.carriedItem.myItem);
        this.RemoveItem(Inventory.carriedItem.myItem, 100);
        Destroy(Inventory.carriedItem.gameObject);
        Inventory.carriedItem = null;
    }

    IEnumerator EnablePickupAfterDelay(PickupItem item, float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public void AddItem(Item newItem, InventoryItem newInventoryItem)
    {
        // If the item is stackable, check if an existing stack can be updated
        if (newItem.isStackable)
        {
            foreach (Items item in items)
            {
                if (item.item == newItem)
                {
                    // If the stack isn't full, add the new items to it
                    if (!item.inventoryItem.IsStackFull())
                    {
                        int spaceLeft = newItem.maxStackSize - item.inventoryItem.currentStackSize;
                        int amountToAdd = Mathf.Min(spaceLeft, newInventoryItem.currentStackSize);
                        item.inventoryItem.AddToStack(amountToAdd);

                        // Update the new stack after adding to the existing stack
                        newInventoryItem.currentStackSize -= amountToAdd;

                        // If newInventoryItem is now empty, destroy it and return
                        if (newInventoryItem.currentStackSize <= 0)
                        {
                            Destroy(newInventoryItem.gameObject);
                            return;
                        }
                    }
                }
            }
        }

        // If there's no existing stack or the item isn't stackable, add it as a new entry
        items.Add(new Items(newItem, newInventoryItem));
    }

    public void AddItem(Item newItem, int quantity)
    {
        foreach (Items item in items)
        {
            if (item.item == newItem)
            {
                if (newItem.isStackable)
                {
                    Debug.Log("Stack before: " + item.inventoryItem.currentStackSize);
                    item.inventoryItem.currentStackSize += quantity;  // Add the specific quantity
                    item.inventoryItem.UpdateStackSizeText();
                    Debug.Log("Stack after: " + item.inventoryItem.currentStackSize);
                    return;
                }
            }
        }

        // If the item doesn't exist in the inventory or it's not stackable, add a new slot with the given quantity
        InventoryItem newInventoryItem = Instantiate(itemPrefab); // Instantiate the prefab
        newInventoryItem.Initialize(newItem, null, quantity); // Initialize it
        items.Add(new Items(newItem, newInventoryItem));
    }

    public void RemoveItem(Item itemToRemove, int quantity)
    {
        foreach (Items item in items)
        {
            if (item.item == itemToRemove)
            {
                if (item.item.isStackable)
                {
                    item.inventoryItem.currentStackSize -= quantity;

                    if (item.inventoryItem.currentStackSize <= 0)
                    {
                        items.Remove(item);
                        Destroy(item.inventoryItem.gameObject);  // Destroy the UI element
                    }
                    else
                    {
                        item.inventoryItem.UpdateStackSizeText();
                    }
                    return;
                }
                else
                {
                    items.Remove(item);
                    Destroy(item.inventoryItem.gameObject);  // Non-stackable items are directly removed
                    return;
                }
            }
        }
    }

    public void RemoveItem(InventoryItem inventoryItemToRemove, int quantity)
    {
        foreach (Items item in items)
        {
            if (item.inventoryItem == inventoryItemToRemove)
            {
                if (item.item.isStackable)
                {
                    item.inventoryItem.currentStackSize -= quantity;

                    if (item.inventoryItem.currentStackSize <= 0)
                    {
                        items.Remove(item);
                    }
                    return;
                }
                else
                {
                    items.Remove(item);
                    return;
                }
            }
        }
    }

    public void SplitItem(Item itemToSplit, int amountToSplit)
    {
        foreach (Items item in items)
        {
            if (item.inventoryItem.currentStackSize >= amountToSplit)
            {
                item.inventoryItem.currentStackSize -= amountToSplit;

                // Instantiate new InventoryItem prefab for the split item
                InventoryItem newInventoryItem = Instantiate(itemPrefab);
                newInventoryItem.Initialize(itemToSplit, null, amountToSplit); // Initialize the new split item

                items.Add(new Items(itemToSplit, newInventoryItem));
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (Singleton == this) Singleton = null;
    }
}

public class Items{
    public Item item;
    public InventoryItem inventoryItem;

    public Items(Item item, InventoryItem inventoryItem)
    {
        this.item = item;
        this.inventoryItem = inventoryItem;
    }
}