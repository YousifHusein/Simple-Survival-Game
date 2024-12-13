// PickupItem handles dropped items in the world
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Item item;  // Reference to the item being dropped/picked up
    public int amount;  // Quantity of the item
    public bool canPickup = true;

    public InventoryManager inventoryManager;

    // Drop the item at a specific position in the world
    public void DropItem(Vector3 dropPosition)
    {
        if (item != null && item.itemModel != null)
        {
            // Instantiate the dropped item in the world
            GameObject droppedItem = Instantiate(item.itemModel, dropPosition, Quaternion.identity);

            // Add the DroppedItem component for pickup functionality
            DroppedItem droppedComponent = droppedItem.AddComponent<DroppedItem>();
            droppedComponent.Initialize(item, amount);

            // Add a Rigidbody to the dropped item for physics
            Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        else
        {
            Debug.LogError("Item or item model is missing!");
        }
    }

    // When the player touches the item, add it to the inventory
    private void OnTriggerEnter(Collider other)
    {
        if (canPickup && other.CompareTag("Player"))
        {
            if (item == null)
            {
                Debug.LogError("Item in PickupItem is null!");
                return;
            }
            // Add the item to the player's inventory
            inventoryManager.AddItem(item, amount);

            // Destroy the pickup item from the world
            Destroy(gameObject);
        }
    }

    public void EnablePickup()
    {
        canPickup = true;
    }
}

// DroppedItem is a component that allows the player to pick up dropped items
public class DroppedItem : MonoBehaviour
{
    private Item item;   // Reference to the item data
    private int amount;  // Amount of the item dropped

    public InventoryManager inventoryManager;

    // Initialize the dropped item with the item data and amount
    public void Initialize(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;

        // Ensure there is a collider on the dropped item for pickup detection
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();  // Add BoxCollider if none exists
        }

        // Set the collider to be a trigger to detect player collisions
        collider.isTrigger = true;
    }

    // When the player collides with the dropped item, pick it up
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add the item to the player's inventory
            inventoryManager.AddItem(item, amount);

            // Destroy this dropped item from the world
            Destroy(gameObject);
        }
    }
}
