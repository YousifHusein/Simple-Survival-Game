using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Tree : MonoBehaviour
{
    public int health = 20;
    public Item log;
    public Item stick;

    public void TakeDamage(int damage)
    {
        Debug.Log("I got here");
        health -= damage;
        if (health <= 0)
        {
            BreakTree();
        }
    }

    void BreakTree()
    {
        GameObject droppedLog = Instantiate(log.itemModel, transform.position, Quaternion.identity);
        GameObject droppedStick = Instantiate(stick.itemModel, transform.position, Quaternion.identity);

        PickupItem logPickupComponent = droppedLog.AddComponent<PickupItem>();
        PickupItem stickPickupComponent = droppedStick.AddComponent<PickupItem>();

        logPickupComponent.item = log;
        logPickupComponent.amount = Random.Range(4, 6);

        stickPickupComponent.item = stick;
        stickPickupComponent.amount = Random.Range(1, 3);

        // Adjust the scale of the placed block (optional)
        droppedLog.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary
        droppedStick.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary

        // Add a Collider if the model doesn't already have one
        Collider logBlockCollider = droppedLog.GetComponent<Collider>();
        if (logBlockCollider == null)
        {
            logBlockCollider = droppedLog.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
        }

        droppedLog.AddComponent<MeshCollider>();
        droppedLog.GetComponent<MeshCollider>().convex = true;
        logBlockCollider.isTrigger = true; // Ensure it's a solid collider

        // Add a Collider if the model doesn't already have one
        Collider stickBlockCollider = droppedStick.GetComponent<Collider>();
        if (stickBlockCollider == null)
        {
            stickBlockCollider = droppedStick.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
        }

        droppedStick.AddComponent<MeshCollider>();
        droppedStick.GetComponent<MeshCollider>().convex = true;
        stickBlockCollider.isTrigger = true; // Ensure it's a solid collider

        // Add Rigidbody for physics
        Rigidbody logRb = droppedLog.GetComponent<Rigidbody>();
        if (logRb == null)
        {
            logRb = droppedLog.AddComponent<Rigidbody>();
        }

        logRb.useGravity = true;
        Debug.Log("Getting here!");
        logRb.isKinematic = false; // Allow it to be affected by physics
        Debug.Log("Kinematic" + logRb.isKinematic);

        // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
        logRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Add Rigidbody for physics
        Rigidbody stickRb = droppedStick.GetComponent<Rigidbody>();
        if (stickRb == null)
        {
            stickRb = droppedStick.AddComponent<Rigidbody>();
        }

        stickRb.useGravity = true;
        Debug.Log("Getting here!");
        stickRb.isKinematic = false; // Allow it to be affected by physics
        Debug.Log("Kinematic" + stickRb.isKinematic);

        // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
        stickRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        //rb.freezeRotation = true; // Optional: Prevent it from rotating randomly

        // Add a slight delay before allowing the physics engine to fully interact with the block (optional)
        //StartCoroutine(EnablePhysicsAfterDelay(rb));
        Destroy(gameObject);
    }

    private IEnumerator EnablePhysicsAfterDelay(Rigidbody rb)
    {
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
    }
}
