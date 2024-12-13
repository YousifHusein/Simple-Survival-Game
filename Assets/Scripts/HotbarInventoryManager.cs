using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

public class HotbarInventoryManager : MonoBehaviour
{
    public InventoryManager inventoryManager; // Reference to the InventoryManager
    public GameObject inventoryPanel;         // Reference to the entire inventory panel (UI)
    public GameObject outsideHotbarPanel;            // Reference to the separate hotbar panel (UI)
    public InventorySlot selectedSlot;

    public Transform handTransform;
    public GameObject equippedItem;
    public GameObject player;

    private int hotbarSlotCount = 10;          // Number of hotbar slots (8 slots)

    private bool isInventoryOpen = false;     // Track if inventory is open

    public Item selectedItem;
    void Start()
    {
        // Set initial visibility states
        inventoryPanel.SetActive(false);
        outsideHotbarPanel.SetActive(true);
        UpdateHotbarUI();
    }

    void Update()
    {
        // Toggle inventory with "E" key press
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        // Loop through keys 1 to the number of hotbar slots to map them to hotbar slots
        for (int i = 0; i < hotbarSlotCount; i++)
        {
            // KeyCode.Alpha1 corresponds to the "1" key, Alpha2 to the "2" key, etc.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Select the hotbar slot based on the number key pressed
                selectedSlot = inventoryManager.inventory[inventoryManager.inventorySlots + i];

                if (selectedSlot != null && selectedSlot.item != null)
                {
                    EquipItem(selectedSlot.item);
                }
                else
                {
                    // Unequip if no item is selected
                    UnequipItem();
                }
            }
        }

        // Keep the equipped item aligned with the camera
        if (equippedItem != null)
        {
            equippedItem.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(-90, 0, 0);
        }
    }

    void ToggleInventory()
    {
        // Toggle inventory panel visibility
        isInventoryOpen = !isInventoryOpen;

        inventoryPanel.SetActive(isInventoryOpen);
        outsideHotbarPanel.SetActive(!isInventoryOpen);

        UpdateHotbarUI();

        // Lock or unlock the cursor based on inventory state
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

    public void UpdateHotbarUI()
    {
        // Update the outside hotbar panel to reflect the items in the last 8 slots of the inventory
        for (int i = 0; i < hotbarSlotCount; i++)
        {
            // Get the inventory hotbar slot information
            InventorySlot hotbarSlot = inventoryManager.inventory[inventoryManager.inventorySlots + i];

            // Get the corresponding outside hotbar slot's UI element
            Transform outsideHotbarSlotTransform = outsideHotbarPanel.transform.GetChild(i);

            // Update the outside hotbar slot's UI to reflect the current item
            if (hotbarSlot.item != null)
            {
                // Set the icon and quantity if there's an item in the slot
                Image itemIcon = outsideHotbarSlotTransform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI quantityText = itemIcon.GetComponentInChildren<TextMeshProUGUI>();

                itemIcon.sprite = hotbarSlot.item.sprite;
                itemIcon.enabled = true;

                quantityText.text = hotbarSlot.quantity > 1 ? hotbarSlot.quantity.ToString() : "";
            }
            else
            {
                // Clear the slot UI if there's no item
                Image itemIcon = outsideHotbarSlotTransform.Find("ItemIcon").GetComponent<Image>();
                TextMeshProUGUI quantityText = itemIcon.GetComponentInChildren<TextMeshProUGUI>();

                itemIcon.enabled = false;
                quantityText.text = "";
            }
        }
    }

    public void EquipItem(Item selectedItem)
    {
        this.selectedItem = selectedItem;
        // Destroy any previously equipped item
        if (equippedItem != null)
        {
            Destroy(equippedItem);
        }

        // Instantiate and equip the new item
        equippedItem = Instantiate(selectedItem.itemModel, handTransform.position, handTransform.rotation, handTransform);

        // Set the local transform to make sure it’s positioned and rotated correctly
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localScale = Vector3.one;
        if (selectedItem.itemTag == "Wand")
        {
            equippedItem.transform.localRotation = Quaternion.Euler(90, 90, 90);
        }
        else if (selectedItem.itemTag == "Sword")
        {
            equippedItem.transform.localRotation = Quaternion.Euler(19, 0, 0);
            equippedItem.transform.localPosition = new Vector3(-.111f, -.723f, -.423f);
            equippedItem.transform.localScale = new Vector3(2f, 2f, 2f);

        }
        else
        {
            equippedItem.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }

        // Attach animator if it has one
        Animator animator = equippedItem.AddComponent<Animator>();
        animator.runtimeAnimatorController = selectedItem.controller;

        if (selectedItem.itemTag == "Tool" || selectedItem.itemTag == "Sword") 
        {
            // Update the player component for swinging and damage
            PlayerSwing playerSwing = player.GetComponent<PlayerSwing>();
            playerSwing.weaponAnimator = animator;
            playerSwing.damage = selectedItem.damage;
            equippedItem.tag = selectedItem.itemTag;
            playerSwing.item = selectedItem;
            playerSwing.enabled = true;

            player.GetComponent<PlayerShoot>().enabled = false;
        }
        else if (selectedItem.itemTag == "Wand")
        {
            // Ranged weapon
            PlayerShoot playerShoot = player.GetComponent<PlayerShoot>();
            playerShoot.weaponAnimator = animator;
            playerShoot.projectilePrefab = selectedItem.projectilePrefab;
            playerShoot.damage = selectedItem.damage;
            Transform firePointTransform = equippedItem.transform.Find("FirePoint");
            if (firePointTransform != null)
            {
                playerShoot.firePoint = firePointTransform;
            }
            else
            {
                Debug.LogError("FirePoint not found on the equipped wand!");
            }
            equippedItem.tag = selectedItem.itemTag;
            playerShoot.enabled = true;

            // Disable PlayerSwing
            player.GetComponent<PlayerSwing>().enabled = false;
        }
        // Make sure Rigidbody is kinematic so that the item is correctly held
        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void UnequipItem()
    {
        // Unequip and remove any reference to the equipped item
        if (equippedItem != null)
        {
            Destroy(equippedItem);
            equippedItem = null;

            // Clear the weaponAnimator in PlayerSwing
            player.GetComponent<PlayerSwing>().weaponAnimator = null;
        }
    }
}
