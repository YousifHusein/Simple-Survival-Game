using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class SwitchingInventories : MonoBehaviour
{
    public Button inventoryMenu;
    public Button craftingMenu;
    public GameObject inventory;
    public GameObject crafting;
    private bool isOpen = false;

    private DisplayRecipe displayRecipe;

    private void Start()
    {
        craftingMenu.onClick.AddListener(OpenCrafting);
        displayRecipe = crafting.GetComponent<DisplayRecipe>();
    }


    void OpenCrafting()
    {
        isOpen = !isOpen;

        if (!isOpen)
        {
            displayRecipe.item1Picture.gameObject.SetActive(false);
            displayRecipe.item1Text.gameObject.SetActive(false);
            displayRecipe.item2Picture.gameObject.SetActive(false);
            displayRecipe.item2Text.gameObject.SetActive(false);
            displayRecipe.item3Picture.gameObject.SetActive(false);
            displayRecipe.item3Text.gameObject.SetActive(false);
            displayRecipe.craftButton.gameObject.SetActive(false);
        }

        crafting.SetActive(isOpen);
    }
}
