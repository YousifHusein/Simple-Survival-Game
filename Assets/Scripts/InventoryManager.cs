using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Net;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;             // Prefab of the slot (UI button)
    public Transform inventoryPanel;          // Reference to Inventory Panel
    public Transform hotbarPanel;
    public Transform outsideHotbarPanel;
    public Button addItemButton;              // Reference to the Add Item button
    public int inventorySlots = 30;            // Total number of inventory slots
    public int hotbarSlots = 10;
    public int totalSlots = 40;

    public List<Item> availableItems;         // List of available items in the game

    public List<InventorySlot> inventory = new List<InventorySlot>();
    private List<GameObject> slotUIList = new List<GameObject>();
    private GameObject pickedUpItem = null;   // Reference to the picked up item
    private InventorySlot pickedUpSlot = null;
    private int pickedUpSlotIndex = -1;

    private bool halfing = false;

    private void Awake()
    {
        // Initialize inventory data slots (without UI instantiation)
        for (int i = 0; i < inventorySlots; i++)
        {
            InventorySlot newSlot = new InventorySlot(null, 0);
            inventory.Add(newSlot);
        }

        // Initialize hotbar data slots
        for (int i = 0; i < hotbarSlots; i++)
        {
            InventorySlot newSlot = new InventorySlot(null, 0);
            inventory.Add(newSlot);
        }

        // Safety check: Ensure the inventory list has the correct number of slots
        if (inventory.Count != totalSlots)
        {
            Debug.LogError($"Awake() Inventory initialization error: Expected {totalSlots} slots but got {inventory.Count} slots.");
        }
    }

    void Start()
    {
        // Initialize inventory UI slots (for inventory panel)
        for (int i = 0; i < inventorySlots; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, inventoryPanel);

            // Set up the Slot UI and add it to the slot UI list
            slotUIList.Add(slotObject);
            AddSlotListeners(slotObject, i);
        }

        // Initialize hotbar UI slots (for hotbar panel)
        for (int i = 0; i < hotbarSlots; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, hotbarPanel);

            // Set up the Slot UI and add it to the slot UI list
            slotUIList.Add(slotObject);
            AddSlotListeners(slotObject, inventorySlots + i);
        }

        for (int i = 0; i < hotbarSlots; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, outsideHotbarPanel);

            // Set up the Slot UI and add it to the slot UI list
            slotUIList.Add(slotObject);
            AddSlotListeners(slotObject, inventorySlots + i);
        }

        // Safety check: Ensure the inventory list has the correct number of slots
        if (inventory.Count != totalSlots)
        {
            Debug.LogError($"Inventory initialization error: Expected {totalSlots} slots but got {inventory.Count} slots.");
        }

        // Assign the button click event for adding items (optional)
        if (addItemButton != null)
        {
            addItemButton.onClick.AddListener(AddRandomItem);
        }

        // Update all slots UI initially
        for (int i = 0; i < totalSlots; i++)
        {
            UpdateSlotUI(i);
            Canvas.ForceUpdateCanvases();
        }

        SyncHotbarSlots();
    }

    void AddSlotListeners(GameObject slotObject, int index)
    {
        // Add click event listener to each slot
        Button slotButton = slotObject.GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(() => OnSlotClicked(index));
        }

        // Add right-click event listener
        EventTrigger trigger = slotObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slotObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry rightClickEntry = new EventTrigger.Entry();
        rightClickEntry.eventID = EventTriggerType.PointerClick;
        int capturedIndex = index;
        rightClickEntry.callback.AddListener((BaseEventData data) => OnSlotRightClicked((PointerEventData)data, capturedIndex));
        trigger.triggers.Add(rightClickEntry);
    }

    void SyncHotbarSlots()
    {
        for (int i = 0; i < hotbarSlots; i++)
        {
            // Get the corresponding slot from the hotbar panel and the outside hotbar panel
            Transform hotbarSlotTransform = hotbarPanel.GetChild(i);
            Transform outsideHotbarSlotTransform = outsideHotbarPanel.GetChild(i);

            // Get the inventory slot representing this hotbar slot
            InventorySlot hotbarSlot = inventory[inventorySlots + i];

            // Update the outside hotbar slot's UI to match the inventory
            if (hotbarSlot.item != null)
            {
                Image itemIcon = outsideHotbarSlotTransform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI quantityText = itemIcon.GetComponentInChildren<TextMeshProUGUI>();

                itemIcon.sprite = hotbarSlot.item.sprite;
                itemIcon.enabled = true;
                quantityText.text = hotbarSlot.quantity > 1 ? hotbarSlot.quantity.ToString() : "";
            }
            else
            {
                Image itemIcon = outsideHotbarSlotTransform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI quantityText = itemIcon.GetComponentInChildren<TextMeshProUGUI>();

                itemIcon.enabled = false;
                quantityText.text = "";
            }
        }
    }

    public InventorySlot[] GetHotbarSlots()
    {
        // Ensure that inventory has enough slots before accessing
        if (inventory.Count < totalSlots)
        {
            Debug.LogError("Error: Inventory does not contain enough slots.");
            return new InventorySlot[0]; // Return an empty array to avoid errors
        }

        // Log inventory count to ensure we have all slots
        Debug.Log($"Inventory count at GetHotbarSlots(): {inventory.Count} (Expected: {totalSlots})");

        InventorySlot[] totalHotbarSlots = new InventorySlot[hotbarSlots];

        // Iterate over the hotbar slots and try to populate them
        for (int i = 0; i < hotbarSlots; i++)
        {
            try
            {
                totalHotbarSlots[i] = inventory[inventorySlots + i];
                Debug.Log($"Hotbar Slot {i} correctly assigned from Inventory Slot {inventorySlots + i}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error accessing hotbar slot {i} from inventory index {inventorySlots + i}: {ex.Message}");
            }
        }

        return totalHotbarSlots;
    }

    private void Update()
    {
        if (pickedUpItem != null)
        {
            // Follow mouse position
            Vector3 mousePosition = Input.mousePosition;
            pickedUpItem.transform.position = mousePosition;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"Current Inventory Count: {inventory.Count}");
            Debug.Log($"Current Total Slots: {totalSlots}");
        }
    }

    public void AddItem(Item item, int amount)
    {
        // If the item is stackable, try to find an existing stack
        if (item.isStackable)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].item == item)
                {
                    int availableSpace = item.maxStackSize - inventory[i].quantity;
                    int amountToAdd = Mathf.Min(amount, availableSpace);
                    inventory[i].AddToStack(amountToAdd);
                    UpdateSlotUI(i);
                    amount -= amountToAdd;

                    if (amount <= 0)
                        return;
                }
            }
        }

        // Add the item to an empty slot if it is non-stackable or if there is remaining quantity
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == null)
            {
                int amountToAdd = Mathf.Min(amount, item.maxStackSize);
                inventory[i] = new InventorySlot(item, amountToAdd);
                UpdateSlotUI(i);
                amount -= amountToAdd;

                if (amount <= 0)
                    return;
            }
        }
    }

    private void AddRandomItem()
    {
        Debug.Log("Getting here on click");
        // Ensure that available items list is not empty
        if (availableItems != null && availableItems.Count > 0)
        {
            Item randomItem = availableItems[Random.Range(0, availableItems.Count)];
            AddItem(randomItem, Random.Range(1, randomItem.maxStackSize + 1));
            Debug.Log("Added a random item to the inventory for testing purposes");
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        InventorySlot slot = inventory[slotIndex];
        if (pickedUpItem == null && slot.item != null)
        {
            // Pick up the item
            pickedUpItem = new GameObject("PickedUpItem");
            Image icon = pickedUpItem.AddComponent<Image>();
            GameObject textObject = new GameObject("childObject");
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = slot.quantity.ToString();
            textObject.transform.SetParent(pickedUpItem.transform);
            icon.sprite = slot.item.sprite;
            icon.raycastTarget = false;
            text.raycastTarget = false;
            pickedUpItem.transform.SetParent(inventoryPanel.parent, false);
            pickedUpSlot = slot;
            pickedUpSlotIndex = slotIndex;
            inventory[pickedUpSlotIndex] = new InventorySlot(null, 0);
            UpdateSlotUI(pickedUpSlotIndex);

            // Hide the original slot item icon
            GameObject slotObject = slotUIList[slotIndex];
            Transform itemIconTransform = slotObject.transform.Find("ItemIcon");
            if (itemIconTransform != null)
            {
                itemIconTransform.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
            }
        }
        else if (pickedUpItem != null)
        {
            if (slotIndex == pickedUpSlotIndex)
            {
                // Place the item in the clicked slot
                inventory[slotIndex] = pickedUpSlot;
                UpdateSlotUI(slotIndex);

                // Destroy the picked-up item
                Destroy(pickedUpItem);
                pickedUpItem = null;
                pickedUpSlot = null;
                pickedUpSlotIndex = -1;
            }
            // If the clicked slot has the same item and is stackable, try to merge the stacks
            else if (slotIndex != pickedUpSlotIndex && slot.item != null && slot.item == pickedUpSlot.item && slot.item.isStackable)
            {
                int availableSpace = slot.item.maxStackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, pickedUpSlot.quantity);
                inventory[slotIndex].AddToStack(amountToAdd);
                pickedUpSlot.quantity -= amountToAdd;

                UpdateSlotUI(slotIndex);

                // If picked up slot is now empty, destroy the picked-up item
                if (pickedUpSlot.quantity <= 0)
                {
                    inventory[pickedUpSlotIndex] = new InventorySlot(null, 0);
                    Destroy(pickedUpItem);
                    pickedUpItem = null;
                    pickedUpSlot = null;
                    pickedUpSlotIndex = -1;
                }
                else
                {
                    // Update the picked-up item's icon to reflect the reduced quantity
                    TextMeshProUGUI text = pickedUpItem.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = pickedUpSlot.quantity.ToString();
                }
            }
            else if (slotIndex != pickedUpSlotIndex && slot.item == null)
            {
                // Place the item in the clicked slot
                inventory[slotIndex] = new InventorySlot(pickedUpSlot.item, pickedUpSlot.quantity);

                if (!halfing)
                {
                    inventory[pickedUpSlotIndex] = new InventorySlot(null, 0);
                }
                halfing = false;

                UpdateSlotUI(slotIndex);
                UpdateSlotUI(pickedUpSlotIndex);

                // Destroy the picked-up item
                Destroy(pickedUpItem);
                pickedUpItem = null;
                pickedUpSlot = null;
                pickedUpSlotIndex = -1;
            }
            else if (pickedUpItem != null && slotIndex != pickedUpSlotIndex && slot.item != null)
            {
                // Swap the items in the inventory array
                InventorySlot tempSlot = inventory[slotIndex];
                inventory[slotIndex] = pickedUpSlot;
                inventory[pickedUpSlotIndex] = tempSlot;

                // Update the visual representation of pickedUpItem
                pickedUpItem.GetComponent<Image>().sprite = inventory[pickedUpSlotIndex].item.sprite;
                TextMeshProUGUI text = pickedUpItem.GetComponentInChildren<TextMeshProUGUI>();
                text.text = inventory[pickedUpSlotIndex].quantity.ToString();

                // Update the UI for both slots
                UpdateSlotUI(slotIndex);
                //UpdateSlotUI(pickedUpSlotIndex);

                // Update the pickedUpSlot to the new item
                pickedUpSlot = inventory[pickedUpSlotIndex]; // Ensure pickedUpSlot points to the new data
            }
        }
    }

    public void OnSlotRightClicked(PointerEventData data, int slotIndex)
    {
        if (data.button == PointerEventData.InputButton.Right)
        {
            InventorySlot slot = inventory[slotIndex];

            if (pickedUpItem == null && slot.item != null && slot.quantity > 1)
            {
                // Pick up half the stack
                int halfQuantity = Mathf.CeilToInt(slot.quantity / 2f);
                slot.RemoveFromStack(halfQuantity);
                pickedUpSlot = new InventorySlot(slot.item, halfQuantity);

                // Create a picked-up item icon
                pickedUpItem = new GameObject("PickedUpItem");
                Image icon = pickedUpItem.AddComponent<Image>();
                GameObject textObject = new GameObject("childObject");
                TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
                text.text = halfQuantity.ToString();
                textObject.transform.SetParent(pickedUpItem.transform);
                icon.sprite = slot.item.sprite;
                icon.raycastTarget = false;
                text.raycastTarget = false;
                pickedUpItem.transform.SetParent(inventoryPanel.parent, false);
                pickedUpSlotIndex = slotIndex;
                halfing = true;

                UpdateSlotUI(slotIndex);
            }
            else if (pickedUpItem != null && pickedUpSlot != null && slot.item != null && slot.item == pickedUpSlot.item && slot.item.isStackable)
            {
                // Place one item in the clicked slot if stackable
                int amountToAdd = 1;
                if (slot.quantity < slot.item.maxStackSize)
                {
                    inventory[slotIndex].AddToStack(amountToAdd);
                    pickedUpSlot.quantity -= amountToAdd;

                    UpdateSlotUI(slotIndex);

                    // If picked up slot is now empty, destroy the picked-up item
                    if (pickedUpSlot.quantity <= 0)
                    {
                        Destroy(pickedUpItem);
                        pickedUpItem = null;
                        pickedUpSlot = null;
                        pickedUpSlotIndex = -1;
                    }
                    else
                    {
                        // Update the picked-up item's icon to reflect the reduced quantity
                        TextMeshProUGUI text = pickedUpItem.GetComponentInChildren<TextMeshProUGUI>();
                        text.text = pickedUpSlot.quantity.ToString();
                    }
                }
            }
            else if (slotIndex == pickedUpSlotIndex)
            {
                // Place the item in the clicked slot
                inventory[slotIndex] = new InventorySlot(pickedUpSlot.item, 1);
                pickedUpSlot.quantity--;

                UpdateSlotUI(slotIndex);

                if (pickedUpSlot.quantity <= 0)
                {
                    Destroy(pickedUpItem);
                    pickedUpItem = null;
                    pickedUpSlot = null;
                    pickedUpSlotIndex = -1;
                }
                else
                {
                    // Update the picked-up item's icon to reflect the reduced quantity
                    TextMeshProUGUI text = pickedUpItem.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = pickedUpSlot.quantity.ToString();
                }
            }
            else if (pickedUpItem != null && pickedUpSlot != null && slot.item == null)
            {
                // Place the item in the clicked slot
                halfing = true;
                inventory[slotIndex] = new InventorySlot(pickedUpSlot.item, 1);
                pickedUpSlot.quantity--;

                UpdateSlotUI(slotIndex);

                if (pickedUpSlot.quantity <= 0)
                {
                    Destroy(pickedUpItem);
                    pickedUpItem = null;
                    pickedUpSlot = null;
                    pickedUpSlotIndex = -1;
                }
                else
                {
                    // Update the picked-up item's icon to reflect the reduced quantity
                    TextMeshProUGUI text = pickedUpItem.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = pickedUpSlot.quantity.ToString();
                }
            }
        }
    }

    public void UpdateSlotUI(int index)
    {
        InventorySlot slot = inventory[index];
        GameObject slotObject = slotUIList[index];

        // Find and validate the "ItemIcon" GameObject
        Transform itemIconTransform = slotObject.transform.Find("ItemIcon");
        if (itemIconTransform == null)
        {
            Debug.LogError($"ItemIcon not found in Slot {index}. Make sure the prefab has the correct structure.");
            return;
        }
        Image icon = itemIconTransform.GetComponent<Image>();
        if (icon == null)
        {
            Debug.LogError($"Image component missing on ItemIcon in Slot {index}");
            return;
        }

        // Find and validate the "QuantityText" GameObject (TextMeshPro)
        Transform quantityTextTransform = itemIconTransform.Find("QuantityText");
        if (quantityTextTransform == null)
        {
            Debug.LogError($"QuantityText not found in Slot {index}. Make sure the prefab has the correct structure.");
            return;
        }
        TextMeshProUGUI quantityText = quantityTextTransform.GetComponent<TextMeshProUGUI>();
        if (quantityText == null)
        {
            Debug.LogError($"TextMeshProUGUI component missing on QuantityText in Slot {index}");
            return;
        }

        // Update icon and quantity text
        if (slot.item != null)
        {
            icon.gameObject.SetActive(true);
            icon.enabled = true;
            icon.sprite = slot.item.sprite;
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        }
        else
        {
            icon.enabled = false;
            quantityText.text = "";
        }
    }
}
