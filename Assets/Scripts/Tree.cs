using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public int health = 20;

    public Item requiredItemToMine;

    public Item item1;
    public Item item2;
    public Item item3;

    private GameObject droppedItem1;
    private GameObject droppedItem2;
    private GameObject droppedItem3;

    public int droppedItem1AmountMin;
    public int droppedItem1AmountMax;
    public int droppedItem2AmountMin;
    public int droppedItem2AmountMax;
    public int droppedItem3AmountMin;
    public int droppedItem3AmountMax;

    private PickupItem droppedItem1PickupComponent;
    private PickupItem droppedItem2PickupComponent;
    private PickupItem droppedItem3PickupComponent;

    public void TakeDamage(int damage, Item currentMiningItem)
    {
        if (currentMiningItem.miningLevel >= requiredItemToMine.miningLevel && currentMiningItem.itemType == requiredItemToMine.itemType) 
        {
            Debug.Log("I got here");
            health -= damage;
            if (health <= 0)
            {
                BreakTree();
            }
        }
    }

    void BreakTree()
    {
        Vector3 newPos = transform.position;
        newPos.y += 10;

        if (item1 != null)
        {
            droppedItem1 = Instantiate(item1.itemModel, newPos, Quaternion.identity);

            droppedItem1PickupComponent = droppedItem1.AddComponent<PickupItem>();
            droppedItem1PickupComponent.inventoryManager = FindObjectOfType<InventoryManager>();

            droppedItem1PickupComponent.item = item1;
            droppedItem1PickupComponent.amount = Random.Range(droppedItem1AmountMin, droppedItem1AmountMax);

            droppedItem1.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary

            // Add a Collider if the model doesn't already have one
            Collider item1Collider = droppedItem1.GetComponent<Collider>();
            if (item1Collider == null)
            {
                item1Collider = droppedItem1.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
            }

            droppedItem1.AddComponent<MeshCollider>();
            droppedItem1.GetComponent<MeshCollider>().convex = true;
            item1Collider.isTrigger = true; // Ensure it's a solid collider

            // Add Rigidbody for physics
            Rigidbody item1Rb = droppedItem1.GetComponent<Rigidbody>();
            if (item1Rb == null)
            {
                item1Rb = droppedItem1.AddComponent<Rigidbody>();
            }

            item1Rb.useGravity = true;
            Debug.Log("Getting here!");
            item1Rb.isKinematic = false; // Allow it to be affected by physics
            Debug.Log("Kinematic" + item1Rb.isKinematic);

            // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
            item1Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (item2 != null)
        {
            droppedItem2 = Instantiate(item2.itemModel, newPos, Quaternion.identity);

            droppedItem2PickupComponent = droppedItem2.AddComponent<PickupItem>();
            droppedItem2PickupComponent.inventoryManager = FindObjectOfType<InventoryManager>();

            droppedItem2PickupComponent.item = item2;
            droppedItem2PickupComponent.amount = Random.Range(droppedItem2AmountMin, droppedItem2AmountMax);

            droppedItem2.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary

            // Add a Collider if the model doesn't already have one
            Collider item2Collider = droppedItem2.GetComponent<Collider>();
            if (item2Collider == null)
            {
                item2Collider = droppedItem2.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
            }

            droppedItem2.AddComponent<MeshCollider>();
            droppedItem2.GetComponent<MeshCollider>().convex = true;
            item2Collider.isTrigger = true; // Ensure it's a solid collider

            // Add Rigidbody for physics
            Rigidbody item2Rb = droppedItem2.GetComponent<Rigidbody>();
            if (item2Rb == null)
            {
                item2Rb = droppedItem2.AddComponent<Rigidbody>();
            }

            item2Rb.useGravity = true;
            Debug.Log("Getting here!");
            item2Rb.isKinematic = false; // Allow it to be affected by physics
            Debug.Log("Kinematic" + item2Rb.isKinematic);

            // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
            item2Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (item3 != null)
        {
            droppedItem3 = Instantiate(item3.itemModel, newPos, Quaternion.identity);

            droppedItem3PickupComponent = droppedItem3.AddComponent<PickupItem>();
            droppedItem3PickupComponent.inventoryManager = FindObjectOfType<InventoryManager>();

            droppedItem3PickupComponent.item = item3;
            droppedItem3PickupComponent.amount = Random.Range(droppedItem3AmountMin, droppedItem3AmountMax);

            droppedItem3.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary

            // Add a Collider if the model doesn't already have one
            Collider item3Collider = droppedItem3.GetComponent<Collider>();
            if (item3Collider == null)
            {
                item3Collider = droppedItem3.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
            }

            droppedItem3.AddComponent<MeshCollider>();
            droppedItem3.GetComponent<MeshCollider>().convex = true;
            item3Collider.isTrigger = true; // Ensure it's a solid collider

            // Add Rigidbody for physics
            Rigidbody item3Rb = droppedItem3.GetComponent<Rigidbody>();
            if (item3Rb == null)
            {
                item3Rb = droppedItem3.AddComponent<Rigidbody>();
            }

            item3Rb.useGravity = true;
            Debug.Log("Getting here!");
            item3Rb.isKinematic = false; // Allow it to be affected by physics
            Debug.Log("Kinematic" + item3Rb.isKinematic);

            // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
            item3Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        Destroy(gameObject);
    }

    private IEnumerator EnablePhysicsAfterDelay(Rigidbody rb)
    {
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
    }
}
