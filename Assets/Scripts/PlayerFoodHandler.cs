using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFoodHandler : MonoBehaviour
{
    [Header("Food Settings")]
    public float maxFood = 100f;
    public float currentFood;
    public float foodDepletionRate = 0.5f;
    public Slider foodBar;

    [Header("Inventory")]
    public HotbarInventoryManager hotbarInventoryManager;
    public GameObject displayRecipe;

    // Start is called before the first frame update
    void Start()
    {
        currentFood = maxFood;

        if (foodBar != null)
        {
            foodBar.maxValue = maxFood;
            foodBar.value = currentFood;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleFoodDepletion();

        if (Input.GetMouseButtonDown(0))
        {
            TryEatEquippedItem();
        }
    }

    private void HandleFoodDepletion()
    {
        currentFood -= foodDepletionRate * Time.deltaTime;

        if (currentFood < 0) currentFood = 0;

        if (foodBar != null)
        {
            foodBar.value = currentFood;
        }

        if (currentFood <= 0)
        {
            Debug.Log("Player is starving");
        }
    }

    private void TryEatEquippedItem()
    {
        if (hotbarInventoryManager.equippedItem != null)
        {
            Item equippedItem = hotbarInventoryManager.selectedItem;
            if (equippedItem.itemTag == "Food")
            {
                EatFood(equippedItem);
            }
        }
    }

    private void EatFood(Item foodItem)
    {
        currentFood += foodItem.foodValue;
        if (currentFood > maxFood) currentFood = maxFood;

        Debug.Log($"Ate {foodItem.name}, replensihed {foodItem.foodValue} food.");
        displayRecipe.SetActive(true);
        displayRecipe.GetComponent<DisplayRecipe>().RemoveItemsFromStacks(hotbarInventoryManager.selectedItem, 1);
        if (hotbarInventoryManager.selectedItem.currentStack <= 0)
        {
            hotbarInventoryManager.UnequipItem();
        }
        hotbarInventoryManager.UpdateHotbarUI();
        displayRecipe.SetActive(false);
    }
}
