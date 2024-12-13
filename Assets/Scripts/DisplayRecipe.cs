using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

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

    public Button sticks;
    public Button woodAxe;
    public Button woodPickaxe;
    public Button stoneAxe;
    public Button stonePickaxe;
    public Button stoneSword;
    public Button staff;

    public Button craftButton;

    private Item pickedItem;

    public InventoryManager inventoryManager; // Reference to InventoryManager

    void Start()
    {
        sticks.onClick.AddListener(() => Display(Resources.Load<Item>("Stick")));
        woodAxe.onClick.AddListener(() => Display(Resources.Load<Item>("WoodAxe")));
        woodPickaxe.onClick.AddListener(() => Display(Resources.Load<Item>("WoodPickaxe")));
        stoneAxe.onClick.AddListener(() => Display(Resources.Load<Item>("StoneAxe")));
        stonePickaxe.onClick.AddListener(() => Display(Resources.Load<Item>("StonePickaxe")));
        stoneSword.onClick.AddListener(() => Display(Resources.Load<Item>("StoneSword")));
        staff.onClick.AddListener(() => Display(Resources.Load<Item>("Staff")));

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
        CheckAndAddStack(pickedItem.item1, ref item1CurrentStack, ref item1Check, pickedItem.item1Amount);
        CheckAndAddStack(pickedItem.item2, ref item2CurrentStack, ref item2Check, pickedItem.item2Amount);
        CheckAndAddStack(pickedItem.item3, ref item3CurrentStack, ref item3Check, pickedItem.item3Amount);

        // Handle null cases (no item required)
        item1Check = pickedItem.item1 == null ? true : item1Check;
        item2Check = pickedItem.item2 == null ? true : item2Check;
        item3Check = pickedItem.item3 == null ? true : item3Check;

        // Crafting logic
        if (item1Check && item2Check && item3Check)
        {
            inventoryManager.AddItem(pickedItem, pickedItem.amountIfCrafted);

            // Remove the required amounts from the inventory across multiple stacks
            RemoveItemsFromStacks(pickedItem.item1, pickedItem.item1Amount);
            RemoveItemsFromStacks(pickedItem.item2, pickedItem.item2Amount);
            RemoveItemsFromStacks(pickedItem.item3, pickedItem.item3Amount);

            ResetCraftingState();
        }
    }

    public void RemoveItemsFromStacks(Item item, int amountToRemove)
    {
        if (item == null || amountToRemove <= 0) return;

        // Iterate over all inventory slots and remove the required amount across multiple stacks
        foreach (InventorySlot slot in inventoryManager.inventory)
        {
            if (slot.item != null && slot.item == item)
            {
                int amountToTake = Mathf.Min(slot.quantity, amountToRemove);

                slot.RemoveFromStack(amountToTake);
                amountToRemove -= amountToTake;

                inventoryManager.UpdateSlotUI(inventoryManager.inventory.IndexOf(slot));

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
    }

    private void CheckAndAddStack(Item item, ref int currentStack, ref bool check, int requiredAmount)
    {
        if (item == null) return;

        // Iterate over all items in the inventory
        foreach (InventorySlot slot in inventoryManager.inventory)
        {
            // If it's the same item, add its stack size
            if (slot.item == item)
            {
                currentStack += slot.quantity;

                // If the total stack size reaches or exceeds the required amount, mark it as sufficient
                if (currentStack >= requiredAmount)
                {
                    check = true;
                    break;  // Exit once we have enough items
                }
            }
        }
    }
}
