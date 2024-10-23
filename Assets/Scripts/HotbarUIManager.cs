using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarUIManager : MonoBehaviour
{
    public InventorySlot[] uiHotbarSlots;
    public InventorySlot selectedHotBarSlot;

    public Transform handTransform; // Reference to the hand's transform
    public GameObject equippedItem; // To track the currently equipped item

    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        // Loop through keys 1 to 9 and map them to hotbar slots
        for (int i = 0; i < uiHotbarSlots.Length; i++)
        {
            // KeyCode.Alpha1 corresponds to the "1" key, Alpha2 to the "2" key, and so on
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                // Select the hotbar slot based on the number key pressed
                selectedHotBarSlot = uiHotbarSlots[i];
                if (selectedHotBarSlot.transform.childCount > 0)
                {
                    Item selectedItem = uiHotbarSlots[i].transform.GetChild(0).GetComponent<InventoryItem>().myItem;
                    EquipItem(selectedItem);
                }
                else
                {
                    if (player.GetComponent<PlayerSwing>().weaponAnimator != null)
                    {
                        player.GetComponent<PlayerSwing>().weaponAnimator = null;
                    }
                    Destroy(equippedItem);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                selectedHotBarSlot = uiHotbarSlots[i];
                Item selectedItem = uiHotbarSlots[i].GetComponent<InventoryItem>().myItem;
                EquipItem(selectedItem);
            }
        }

        if (equippedItem != null)
        {
            equippedItem.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(-90, 0, 0);
        }
    }

    public void EquipItem(Item selectedItem)
    {
        if (equippedItem != null)
        {
            Destroy(equippedItem);
        }

        equippedItem = Instantiate(selectedItem.itemModel, handTransform.position, handTransform.rotation, handTransform);

        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        equippedItem.transform.localScale = new Vector3(1f, 1f, 1f);

        Animator animator = equippedItem.AddComponent<Animator>();
        animator.runtimeAnimatorController = selectedItem.controller;

        player.GetComponent<PlayerSwing>().weaponAnimator = animator;
        player.GetComponent<PlayerSwing>().damage = selectedItem.damage;
        equippedItem.tag = selectedItem.itemTag.ToString();

        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
}
