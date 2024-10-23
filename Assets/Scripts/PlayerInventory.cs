using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Reference to the inventory panel UI element
    public GameObject inventoryPanel;
    public GameObject uiHotbar;
    public InventorySlot[] inventoryHotbarSlots;
    public InventorySlot[] uiHotbarSlots;
    public bool isInventoryOpen = false;
    public bool isHotbarOpen = true;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        isHotbarOpen = !isHotbarOpen;

        inventoryPanel.SetActive(isInventoryOpen);
        uiHotbar.SetActive(isHotbarOpen);
        updateHotbar();

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void updateHotbar()
    {
        if (isInventoryOpen)
        {
            for (int i = 0; i < uiHotbarSlots.Length; i++)
            {
                if (uiHotbarSlots[i].transform.childCount > 0)
                {
                    uiHotbarSlots[i].transform.GetChild(0).SetParent(inventoryHotbarSlots[i].transform);
                    inventoryHotbarSlots[i].transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
        }
        else
        {
            for (int i = 0; i < inventoryHotbarSlots.Length; i++)
            {
                if (inventoryHotbarSlots[i].transform.childCount > 0)
                {
                    inventoryHotbarSlots[i].transform.GetChild(0).SetParent(uiHotbarSlots[i].transform);
                    uiHotbarSlots[i].transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
        }
    }
}