// InventoryItem represents an item that can be picked up and managed in the inventory
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static UnityEditor.Progress;
using System.Linq;
using System;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    Image itemIcon;  // The icon of the item

    public bool fullyInitialized = false;  // Whether the item has been fully initialized
    public CanvasGroup canvasGroup { get; private set; }  // CanvasGroup for UI interactions

    public Item myItem { get; set; }  // Reference to the item data
    public InventorySlot activeSlot { get; set; }  // The slot that holds this item

    public int currentStackSize = 1;  // Current stack size of the item

    [SerializeField] private TMP_Text stackSizeText;  // UI text to show stack size

    public RuntimeAnimatorController controller;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 2f;

    public Inventory inventory;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemIcon = GetComponent<Image>();

        stackSizeText = GetComponentInChildren<TMP_Text>();

        inventory = FindObjectOfType<Inventory>();
    }

    // Initialize the item with data and a parent slot
    public void Initialize(Item item, InventorySlot parent, int stackSize = 1)
    {
        if (item == null)
        {
            Debug.LogError("Initialize called with a null item!");
            return;
        }

        gameObject.name = item.name;

        // Check if the parent slot is null
        if (parent != null)
        {
            activeSlot = parent;
            activeSlot.myItem = this;  // Only assign if the parent slot is valid
        }

        myItem = item;
        currentStackSize = stackSize;
        itemIcon.sprite = item.sprite;
        myItem.controller = item.controller;

        UpdateStackSizeText();

        fullyInitialized = true;

        if (activeSlot != null)
        {
            ResizeToFitSlot();
        }
    }

    // Resize the item to fit its parent slot
    public void ResizeToFitSlot()
    {
        if (activeSlot != null)
        {
            RectTransform itemRect = GetComponent<RectTransform>();
            RectTransform slotRect = activeSlot.GetComponent<RectTransform>();

            if (itemRect != null && slotRect != null)
            {
                itemRect.sizeDelta = slotRect.sizeDelta;
                itemRect.localScale = Vector3.one;
                itemRect.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            Debug.LogWarning("No active slot available to resize the item to.");
        }
    }

    public void AddToStack(int amount)
    {
        currentStackSize += amount;
        UpdateStackSizeText();
    }

    public void RemoveFromStack(int amount)
    {
        currentStackSize -= amount;
        UpdateStackSizeText();

        if (currentStackSize <= 0)
        {
            if (activeSlot != null)
            {
                activeSlot.myItem = null;
            }

            Destroy(this.gameObject);
        }
    }

    // Check if the stack is full
    public bool IsStackFull()
    {
        return currentStackSize >= myItem.maxStackSize;
    }

    // Update the stack size text UI
    public void UpdateStackSizeText()
    {
        myItem.currentStack = currentStackSize;
        if (stackSizeText != null)
        {
            if (currentStackSize > 1)
            {
                stackSizeText.text = currentStackSize.ToString();
                stackSizeText.gameObject.SetActive(true);
            }
            else
            {
                stackSizeText.gameObject.SetActive(false);
            }
        }
    }

    // Handle clicks on the item
    public void OnPointerClick(PointerEventData eventData)
    {
        // Regular click handling
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Inventory.Singleton.SetCarriedItem(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right-click to split the stack
            int halfStack = Mathf.FloorToInt(currentStackSize / 2);
            Debug.Log("CURRENT STACK: " + currentStackSize);
            Debug.Log("HALF OF THE STACK: " + halfStack);
            if (halfStack > 0)
            {
                // Reduce the current stack size by half
                currentStackSize -= halfStack;
                UpdateStackSizeText();

                // Instantiate the new split item (halfItem)
                InventoryItem halfItem = Instantiate(Inventory.Singleton.itemPrefab, this.activeSlot.transform);

                // Immediately set the correct size and scale for halfItem (even though it's not yet in a slot)
                RectTransform halfItemRect = halfItem.GetComponent<RectTransform>();

                if (halfItemRect != null)
                {
                    halfItemRect.localScale = Vector3.one;  // Ensure it's at normal scale
                    halfItemRect.sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
                    halfItemRect.anchoredPosition = Vector2.zero;  // Center it, in case needed
                    Debug.Log("Scale after forced reset: " + halfItemRect.localScale);
                }

                string uniqueName = myItem.name + "_Split_" + DateTime.Now.Ticks;

                // Initialize the half item (without a slot for now)
                halfItem.Initialize(myItem, null, halfStack);

                halfItem.name = uniqueName;
                Inventory.Singleton.items.Add(new Items(myItem, halfItem));

                // Let SetCarriedItem handle resizing and parenting properly
                Inventory.Singleton.SetCarriedItem(halfItem);
                Debug.Log("Split Half Item: " + halfItem.myItem);
                Debug.Log("Split Half Inventory ITem: " + halfItem);
                Debug.Log("Split half Item Size: " + halfItem.myItem.currentStack);
            }
        }
    }

    private void AutoStackItems()
    {
        foreach (InventorySlot slot in Inventory.Singleton.inventorySlots)
        {
            if (slot.myItem != null && slot.myItem.myItem == myItem && slot.myItem != this)
            {
                InventoryItem otherItem = slot.myItem;

                int availableSpace = myItem.maxStackSize - currentStackSize;

                if (availableSpace > 0)
                {
                    int itemsToMove = Mathf.Min(availableSpace, otherItem.currentStackSize);

                    AddToStack(itemsToMove);

                    otherItem.currentStackSize -= itemsToMove;
                    otherItem.UpdateStackSizeText();

                    if (otherItem.currentStackSize <= 0)
                    {
                        Destroy(otherItem.gameObject);
                        slot.myItem = null;
                    }

                    UpdateStackSizeText();

                    if (IsStackFull())
                    {
                        break;
                    }
                }
            }
        }
    }
}