using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using System.Linq;

public class DisplayRecipe : MonoBehaviour
{
    public Image item1Picture;
    public Image item2Picture;
    public Image item3Picture;

    public TMP_Text item1Text;
    public TMP_Text item2Text;
    public TMP_Text item3Text;

    private int item1CurrentStack = 0;
    private int item2CurrentStack = 0;
    private int item3CurrentStack = 0;

    private bool item1Check = false;
    private bool item2Check = false;
    private bool item3Check = false;

    private bool removeItem1 = false;
    private bool removeItem2 = false;
    private bool removeItem3 = false;

    public Button sticks;
    public Button woodAxe;
    public Button woodPickaxe;
    public Button stoneAxe;
    public Button stonePickaxe;

    public Button craftButton;

    private Item pickedItem;

    public Inventory inventory;
    private InventoryItem inventoryItem;

    private InventoryItem inventoryItem1;
    private InventoryItem inventoryItem2;
    private InventoryItem inventoryItem3;

    // Update is called once per frame
    void Start()
    {
        sticks.onClick.AddListener(() => Display(Resources.Load<Item>("Stick")));
        woodAxe.onClick.AddListener(() => Display(Resources.Load<Item>("WoodAxe")));
        woodPickaxe.onClick.AddListener(() => Display(Resources.Load<Item>("WoodPickaxe")));
        stoneAxe.onClick.AddListener(() => Display(Resources.Load<Item>("StoneAxe")));
        stonePickaxe.onClick.AddListener(() => Display(Resources.Load<Item>("StonePickaxe")));

        craftButton.onClick.AddListener(Craft);
    }

    void Display(Item item)
    {
        this.pickedItem = item;
        craftButton.gameObject.SetActive(true);

        if (item.item1 != null)
        {
            this.item1Picture.sprite = item.item1.sprite;
            this.item1Picture.gameObject.SetActive(true);
            this.item1Text.text = item.item1Amount.ToString();
            this.item1Text.gameObject.SetActive(true);
        }
        else
        {
            this.item1Picture.gameObject.SetActive(false);
            this.item1Text.gameObject.SetActive(false);
        }

        if (item.item2 != null)
        {
            this.item2Picture.sprite = item.item2.sprite;
            this.item2Picture.gameObject.SetActive(true);
            this.item2Text.text = item.item2Amount.ToString();
            this.item2Text.gameObject.SetActive(true);
        }
        else
        {
            this.item2Picture.gameObject.SetActive(false);
            this.item2Text.gameObject.SetActive(false);
        }

        if (item.item3 != null)
        {
            this.item3Picture.sprite = item.item3.sprite;
            this.item3Picture.gameObject.SetActive(true);
            this.item3Text.text = item.item3Amount.ToString();
            this.item3Text.gameObject.SetActive(true);
        }
        else
        {
            this.item3Picture.gameObject.SetActive(false);
            this.item3Text.gameObject.SetActive(false);
        }
    }

    void Craft()
    {
        // Reset current stacks before checking
        item1CurrentStack = 0;
        item2CurrentStack = 0;
        item3CurrentStack = 0;

        // Sum up the total quantities across all stacks in the inventory for each item
        CheckAndAddStack(pickedItem.item1, ref item1CurrentStack, ref item1Check, ref removeItem1, ref inventoryItem1, pickedItem.item1Amount);
        CheckAndAddStack(pickedItem.item2, ref item2CurrentStack, ref item2Check, ref removeItem2, ref inventoryItem2, pickedItem.item2Amount);
        CheckAndAddStack(pickedItem.item3, ref item3CurrentStack, ref item3Check, ref removeItem3, ref inventoryItem3, pickedItem.item3Amount);

        // Handle null cases (no item required)
        item1Check = pickedItem.item1 == null ? true : item1Check;
        item2Check = pickedItem.item2 == null ? true : item2Check;
        item3Check = pickedItem.item3 == null ? true : item3Check;

        // Crafting logic
        if (item1Check && item2Check && item3Check)
        {
            Inventory.Singleton.SpawnInventoryItem(pickedItem, pickedItem.amountIfCrafted);

            // Remove the required amounts from the inventory across multiple stacks
            RemoveItemsFromStacks(pickedItem.item1, pickedItem.item1Amount);
            RemoveItemsFromStacks(pickedItem.item2, pickedItem.item2Amount);
            RemoveItemsFromStacks(pickedItem.item3, pickedItem.item3Amount);

            ResetCraftingState();
        }
    }


    private void RemoveItemsFromStacks(Item item, int amountToRemove)
    {
        if (item == null || amountToRemove <= 0) return;

        // Iterate over all inventory items and remove the required amount across multiple stacks
        foreach (var tuple in inventory.items.ToList()) // Use ToList() to avoid modification issues during iteration
        {
            InventoryItem inventoryItem = tuple.inventoryItem;

            // Check if this is the correct item by comparing the item names
            if (inventoryItem.myItem != null && inventoryItem.myItem.name == item.name)
            {
                // Check how many items we can remove from this stack
                int amountToTake = Mathf.Min(inventoryItem.currentStackSize, amountToRemove);

                Debug.Log("Removing: " + amountToTake + " from stack with " + inventoryItem.currentStackSize);

                // Remove the calculated amount from the current stack
                inventoryItem.RemoveFromStack(amountToTake);

                // Decrease the remaining amount to remove
                amountToRemove -= amountToTake;

                // If we've removed enough, stop
                if (amountToRemove <= 0)
                {
                    break;
                }
            }
        }

        // If we haven't removed enough (this shouldn't normally happen), log a warning
        if (amountToRemove > 0)
        {
            Debug.LogWarning("Not enough items to remove. Missing: " + amountToRemove);
        }
    }

    // Helper to reset crafting state
    void ResetCraftingState()
    {
        item1CurrentStack = 0;
        item2CurrentStack = 0;
        item3CurrentStack = 0;

        item1Check = false;
        item2Check = false;
        item3Check = false;

        inventoryItem1 = null;
        inventoryItem2 = null;
        inventoryItem3 = null;
    }

    private bool CheckAndAddStack(Item item, ref int currentStack, ref bool check, ref bool removeItem, ref InventoryItem invItem, int requiredAmount)
    {
        if (item == null) return false;

        // Iterate over all items in the inventory
        foreach (var tuple in inventory.items)
        {
            InventoryItem inventoryItem = tuple.inventoryItem;
            Debug.Log("Item: " + tuple.item);
            Debug.Log("Invetnryo Item: " + tuple.inventoryItem);

            // If it's the same item, add its stack size
            if (inventoryItem.myItem == item)
            {
                Debug.Log("Current stack: " + currentStack + " for item: " + inventoryItem);
                currentStack += inventoryItem.currentStackSize;
                Debug.Log("Current stack after: " + currentStack + " for item: " + inventoryItem);

                // If the total stack size reaches or exceeds the required amount, mark it for removal
                if (currentStack >= requiredAmount)
                {
                    check = true;
                    removeItem = true;
                    invItem = inventoryItem; // Store the inventory item reference (used for removal)
                    break;  // Exit once we have enough items
                }
            }
        }

        return check;
    }
}
