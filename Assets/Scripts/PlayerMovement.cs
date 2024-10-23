using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement parameters
    public float walkSpeed = 5f;
    public float sprintSpeed = 20f;
    public float currentSpeed;
    public float acceleration = 10f; // Controls how quickly the player accelerates
    public float jumpForce = 5f; // Controls jump height
    private bool isGrounded; // Tracks if the player is on the ground
    private bool sprinting; // Tracks if the player is sprinting

    // Mouse look parameters
    public float mouseSens = 300f;
    public float smoothingFactor = 0.1f; // Not currently used
    private Vector2 smoothedVelocity; // Tracks the mouse's velocity
    private Vector2 currentLooking; // Tracks the current look direction

    public Transform playerCamera; // Reference to the player's camera

    // Rigidbody for movement physics
    private Rigidbody rb;

    // For placing items in the world
    public GameObject droppedItemPrefab;
    public Item leftClickItem;
    public Item rightClickItem;
    public Inventory inventory;
    public PlayerInventory playerInventory;

    private void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize the Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents the player from rotating due to physics
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Continuous collision detection to avoid clipping at high speeds

        // Initialize variables
        sprinting = false;
        currentSpeed = walkSpeed; // Start with walking speed
    }

    void Update()
    {
        if (playerInventory.isInventoryOpen)
        {
            HandleMovement();
            return;
        }
        HandleMouseLook();
        // Left-click to place a block
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // The place position is the point where the raycast hits
                Vector3 placePosition = hit.point;
                placePosition.y += 2f;

                // The item you want to place (should be a reference to a ScriptableObject)
                Item itemToPlace = leftClickItem;  // This is an example function to get the selected item

                // Place the block at the clicked position
                // UNCOMMENT TO PLACE placeBlock(placePosition, itemToPlace);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // The place position is the point where the raycast hits
                Vector3 placePosition = hit.point;
                placePosition.y += 2f;

                // The item you want to place (should be a reference to a ScriptableObject)
                Item itemToPlace = rightClickItem;  // This is an example function to get the selected item

                // Place the block at the clicked position
                // UNCOMMENT TO PLACE placeBlock(placePosition, itemToPlace);
            }
        }
        HandleMovement();
    }

    // Handles the player's mouse look
    void HandleMouseLook()
    {
        // Get mouse input for X and Y axis
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        // Update the smooth velocity for mouse movement (optional smoothing can be added here)
        smoothedVelocity.x = mouseX;
        smoothedVelocity.y = mouseY;

        // Adjust the current look direction
        currentLooking.x += smoothedVelocity.x;
        currentLooking.y -= smoothedVelocity.y;
        currentLooking.y = Mathf.Clamp(currentLooking.y, -90f, 90f); // Clamp vertical look between -90 and 90 degrees

        // Apply the look direction to the camera and player
        playerCamera.localRotation = Quaternion.Euler(currentLooking.y, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, currentLooking.x, 0f);
    }

    // Handles player movement (walking, sprinting, jumping)
    void HandleMovement()
    {
        // Get movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement vector
        Vector3 movement = transform.right * moveX + transform.forward * moveZ;

        // Handle sprinting
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }

        // Smoothly transition between walking and sprinting speeds
        if (sprinting)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, sprintSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, walkSpeed, Time.deltaTime * acceleration);
        }

        // Apply movement based on speed
        movement *= currentSpeed * Time.deltaTime;

        // Raycast check to prevent clipping through walls or objects
        if (!Physics.Raycast(transform.position, movement.normalized, 0.5f,
                         LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            rb.MovePosition(rb.position + movement);
        }

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Detects if the player touches the ground
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Places an item in the world when the player clicks
    public void placeBlock(Vector3 placePosition, Item item)
    {
        if (item != null && item.itemModel != null)
        {
            // Adjust the place position to ensure the block spawns above the terrain
            placePosition.y += 0.5f;  // Slight offset above the terrain to prevent spawning inside it

            // Instantiate the item's model at the adjusted position
            GameObject placedBlock = Instantiate(item.itemModel, placePosition, Quaternion.identity);

            PickupItem pickupComponent = placedBlock.AddComponent<PickupItem>();

            pickupComponent.item = item;
            pickupComponent.amount = 1;

            // Adjust the scale of the placed block (optional)
            placedBlock.transform.localScale = new Vector3(1f, 1f, 1f); // Modify if necessary

            // Add a Collider if the model doesn't already have one
            Collider blockCollider = placedBlock.GetComponent<Collider>();
            if (blockCollider == null)
            {
                blockCollider = placedBlock.AddComponent<BoxCollider>(); // Default to BoxCollider if none exists
            }

            placedBlock.AddComponent<MeshCollider>();
            placedBlock.GetComponent<MeshCollider>().convex = true;
            blockCollider.isTrigger = true; // Ensure it's a solid collider

            // Add Rigidbody for physics
            Rigidbody rb = placedBlock.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = placedBlock.AddComponent<Rigidbody>();
            }

            rb.useGravity = true;
            rb.isKinematic = false; // Allow it to be affected by physics

            // Set Rigidbody's collision detection to continuous to prevent it from passing through objects
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            //rb.freezeRotation = true; // Optional: Prevent it from rotating randomly

            // Add a slight delay before allowing the physics engine to fully interact with the block (optional)
            StartCoroutine(EnablePhysicsAfterDelay(rb));
        }
        else
        {
            Debug.LogError("Item or itemModel is missing! Cannot place block.");
        }
    }

    private IEnumerator EnablePhysicsAfterDelay(Rigidbody rb)
    {
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
    }

}