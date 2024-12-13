using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSwing : MonoBehaviour
{
    public HotbarInventoryManager hotbarInventoryManager;

    public Animator weaponAnimator;
    public float swingCooldown = 0.5f;
    private bool canSwing = true;

    public float swingRange = 2.5f;
    public LayerMask treeLayer;

    public int damage;

    public GameObject orientation;

    public Item item;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canSwing && hotbarInventoryManager.equippedItem != null && (hotbarInventoryManager.equippedItem.CompareTag("Tool") || hotbarInventoryManager.equippedItem.CompareTag("Sword"))) 
        {
            Swing();
        }
    }

    void Swing()
    {
        weaponAnimator.SetTrigger("SwingTrigger");
        // Check if we're within range to hit a tree
        RaycastHit hit;
        Debug.DrawRay(transform.position, orientation.transform.forward * swingRange, Color.red, 1.0f);

        if (Physics.Raycast(transform.position, orientation.transform.forward, out hit, swingRange, treeLayer))
        {
            // If we hit something in the tree layer within range, apply damage
            if (hotbarInventoryManager.equippedItem.CompareTag("Tool"))
            {
                Tree tree = hit.collider.GetComponent<Tree>();
                if (tree != null)
                {
                    tree.TakeDamage(damage, item);
                }
            }
            if (hotbarInventoryManager.equippedItem.CompareTag("Sword"))
            {
                Health enemy = hit.collider.GetComponent<Health>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        canSwing = false;
        StartCoroutine(SwingCooldown());
    }

    IEnumerator SwingCooldown()
    {
        yield return new WaitForSeconds(swingCooldown);

        canSwing = true;
    }
}
