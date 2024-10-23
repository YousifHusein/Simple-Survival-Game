using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    public Animator weaponAnimator;
    public float swingCooldown = 0.5f;
    private bool canSwing = true;
    public HotbarUIManager hotbarUIManager;

    public float swingRange = 2.5f;
    public LayerMask treeLayer;

    public int damage;

    public GameObject orientation;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canSwing && hotbarUIManager.equippedItem != null && hotbarUIManager.equippedItem.CompareTag("Tool")) 
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
            Tree tree = hit.collider.GetComponent<Tree>();
            if (tree != null)
            {
                tree.TakeDamage(damage);  // Example: Apply 10 damage to the tree
                Debug.Log("Tree hit!");
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
