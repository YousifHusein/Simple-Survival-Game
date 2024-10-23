using UnityEngine;

public class PlayerWaterInteraction : MonoBehaviour
{
    [Header("Water Interaction")]
    public Transform waterPlane; // Reference to the water plane
    public float buoyancyForce = 10f; // Strength of the buoyancy force
    public float waterDrag = 3f; // Drag when in water to simulate slower movement
    public float waterAngularDrag = 1f; // Angular drag in water

    public float swimUpSpeed = 5f; // Speed at which the player swims up
    public float swimDownSpeed = 5f; // Speed at which the player swims down

    private bool isInWater = false; // Tracks whether the player is in the water
    private bool isSubmerged = false; // Tracks if the player is submerged below the water surface

    private Rigidbody rb;
    public CapsuleCollider playerCollider;

    [Header("Keybinds")]
    public KeyCode swimUpKey = KeyCode.Space; // Key to swim up
    public KeyCode swimDownKey = KeyCode.LeftControl; // Key to swim down (crouch)

    void Start()
    {
        // Get the Rigidbody from the parent object (this GameObject)
        rb = GetComponent<Rigidbody>();

        // Find the PlayerObj child by name and get its CapsuleCollider
        Transform playerObject = transform.Find("PlayerObj"); // Match the name exactly as "PlayerObj"

        if (playerObject != null)
        {
            playerCollider = playerObject.GetComponent<CapsuleCollider>();

            if (playerCollider == null)
            {
                Debug.LogError("CapsuleCollider not found on PlayerObj!");
            }
            else
            {
                Debug.Log("CapsuleCollider found and assigned!");
            }
        }
        else
        {
            Debug.LogError("PlayerObj not found! Ensure the player hierarchy is correct.");
        }
    }

    void FixedUpdate()
    {
        CheckWaterState(); // Always check the player's position relative to the water

        if (isInWater || isSubmerged) // If the player is in or below the water
        {
            ApplyBuoyancy();
            SwimControls(); // Handle swimming controls for up and down
        }
    }

    // Detect when the player enters the water
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            EnterWater();
        }
    }

    // Detect when the player exits the water (only when fully above the surface)
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Water") && playerCollider.transform.position.y > waterPlane.position.y)
        {
            ExitWater();
        }
    }

    private void EnterWater()
    {
        isInWater = true;

        // Increase drag to simulate water resistance
        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;

        // Optionally disable gravity to focus only on buoyancy and swimming
        rb.useGravity = false;
    }

    private void ExitWater()
    {
        isInWater = false;
        isSubmerged = false; // Ensure the player is no longer considered submerged

        // Restore normal drag values
        rb.drag = 0;
        rb.angularDrag = 0.05f;

        // Re-enable gravity for normal movement
        rb.useGravity = true;
    }

    // Apply buoyancy to the player when underwater
    private void ApplyBuoyancy()
    {
        float waterSurfaceY = waterPlane.position.y; // Get the water plane's Y position
        float playerY = playerCollider.transform.position.y; // Get the Y position of the player's collider (PlayerObject)

        // If the player is below the water surface, apply upward buoyancy force
        if (playerY < waterSurfaceY)
        {
            // Calculate the depth of the player relative to the water surface
            float depth = waterSurfaceY - playerY;

            // Apply buoyancy proportional to the depth (the further below the surface, the stronger the force)
            rb.AddForce(Vector3.up * buoyancyForce * depth, ForceMode.Acceleration);
        }
    }

    // Allow the player to swim up or down while in water
    private void SwimControls()
    {
        // Swim up by holding the swimUpKey (e.g., Space)
        if (Input.GetKey(swimUpKey))
        {
            rb.AddForce(Vector3.up * swimUpSpeed, ForceMode.Acceleration);
        }

        // Swim down by holding the swimDownKey (e.g., Left Control)
        if (Input.GetKey(swimDownKey))
        {
            rb.AddForce(Vector3.down * swimDownSpeed, ForceMode.Acceleration);
        }
    }

    // Check the player's Y position relative to the water surface to maintain water state
    private void CheckWaterState()
    {
        float playerY = playerCollider.transform.position.y;
        float waterSurfaceY = waterPlane.position.y;

        // Check if the player is below the water surface, even if they exit the trigger
        if (playerY < waterSurfaceY)
        {
            isSubmerged = true; // Player is below the water
            if (!isInWater) // If not in water already, enter water state
            {
                EnterWater();
            }
        }
        else if (playerY >= waterSurfaceY)
        {
            isSubmerged = false; // Player is above the water
        }
    }
}