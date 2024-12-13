using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [SerializeField]
    private int currentHealth;

    public GameObject gameOverCanvas;
    public GameObject mainCanvas;

    [Header("Enemy Drop Settings")]
    public List<ItemDrop> dropItems; // List of items to drop
    public Transform dropPoint; // Optional: Where items are dropped

    [Header("Player Death Settings")]
    public bool isPlayer = false;

    [Header("UI Settings")]
    public Slider healthBar;

    [Header("Regeneration Settings")]
    public float regenRate = 5f;
    public float regenInterval = 1f;

    private float regenTimer = 0f;

    public PlayerFoodHandler playerFoodHandler;

    public delegate void DeathHandler();
    public event DeathHandler OnDeath;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update()
    {
        HandleHealthRegeneration();
    }

    private void HandleHealthRegeneration()
    {
        if (playerFoodHandler != null && playerFoodHandler.currentFood >= 90 && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                Heal((int)(regenRate * regenInterval));
                regenTimer = 0f;
            }
        }
        else
        {
            regenTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"{gameObject.name} healed by {amount}. Current health: {currentHealth}");

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        if (isPlayer)
        {
            currentHealth = 0;
            TriggerGameOver();
        }
        else
        {
            Debug.Log($"{gameObject.name} died!");
            DropItems();
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    private void DropItems()
    {
        foreach (ItemDrop itemDrop in dropItems)
        {
            // Determine drop position
            Vector3 dropPosition = dropPoint != null ? dropPoint.position : transform.position;
            dropPosition.y += 1.0f; // Slightly raise the drop position to prevent overlap with the ground

            // Instantiate the item
            GameObject droppedItem = Instantiate(itemDrop.item.itemModel, dropPosition, Quaternion.identity);

            // Add PickupItem component for interaction
            PickupItem pickupComponent = droppedItem.AddComponent<PickupItem>();
            pickupComponent.inventoryManager = FindObjectOfType<InventoryManager>();
            pickupComponent.item = itemDrop.item;
            pickupComponent.amount = Random.Range(itemDrop.minAmount, itemDrop.maxAmount + 1);

            // Add Rigidbody for physics
            Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Optional: Add a collider if not present
            if (droppedItem.GetComponent<Collider>() == null)
            {
                droppedItem.AddComponent<BoxCollider>();
            }

            droppedItem.AddComponent<MeshCollider>();
            droppedItem.GetComponent<MeshCollider>().convex = true;
            droppedItem.GetComponent<BoxCollider>().isTrigger = true;

            Debug.Log($"Dropped {itemDrop.item.name} x{pickupComponent.amount}");
        }
    }

    void TriggerGameOver()
    {
        gameOverCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 0f;
    }
}

[System.Serializable]
public class ItemDrop
{
    public Item item; // The item to drop
    public int minAmount = 1; // Minimum amount to drop
    public int maxAmount = 3; // Maximum amount to drop
}
