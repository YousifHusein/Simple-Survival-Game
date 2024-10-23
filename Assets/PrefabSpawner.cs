using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject treePrefab;         // Tree prefab
    public GameObject bigRockPrefab;      // Big rock prefab
    public GameObject smallRockPrefab;    // Small rock prefab
    public int numberOfTrees = 100;       // Number of trees to spawn
    public float spawnRadius = 50f;       // Radius for random tree placement
    public LayerMask groundLayer;         // LayerMask for ground layer
    public LayerMask waterLayer;          // LayerMask for water layer (if using layers for water)
    public float heightOffset = 5f;       // Offset to apply to the Y-coordinate
    public int maxRaycastAttempts = 10;   // Maximum attempts per tree placement
    public bool useWaterTag = true;       // Use water tag check instead of water layer
    public float maxSlopeAngle = 45f;     // Maximum slope angle for tree and small rock placement
    public float minSlopeForBigRocks = 45f; // Minimum slope angle for big rocks
    public float maxSlopeForBigRocks = 70f; // Maximum slope angle for big rocks
    public float smallRockLowerOffset = 0.5f; // How much lower to place small rocks
    public Vector3 smallRockRotation = new Vector3(-90, 45, 0); // Rotation for small rocks

    public void SpawnTreesAndRocks()
    {
        int treesSpawned = 0;
        int bigRocksSpawned = 0;
        int smallRocksSpawned = 0;

        for (int i = 0; i < numberOfTrees; i++)
        {
            bool objectSpawned = false;
            int attempts = 0;

            while (!objectSpawned && attempts < maxRaycastAttempts)
            {
                // Generate a random position within a circular radius
                Vector3 randomPosition = new Vector3(
                    Random.Range(transform.position.x - spawnRadius, transform.position.x + spawnRadius),
                    transform.position.y + 50,  // Start the raycast high above the terrain
                    Random.Range(transform.position.z - spawnRadius, transform.position.z + spawnRadius)
                );

                // Perform a raycast to find the ground
                if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                        Debug.Log($"Detected slope angle: {slopeAngle} at position {hit.point}");

                        // Spawn big rock on steep slopes
                        if (slopeAngle >= minSlopeForBigRocks && slopeAngle <= maxSlopeForBigRocks)
                        {
                            Vector3 rockPosition = hit.point + new Vector3(0, heightOffset, 0);
                            GameObject bigRock = Instantiate(bigRockPrefab, rockPosition, Quaternion.identity);
                            bigRock.transform.SetParent(transform);
                            Debug.Log($"Big rock spawned at: {rockPosition} on slope angle {slopeAngle}");
                            bigRocksSpawned++;
                            objectSpawned = true;
                        }
                        else if (slopeAngle <= maxSlopeAngle) // Tree and small rock area
                        {
                            Vector3 spawnPosition = hit.point + new Vector3(0, heightOffset, 0);
                            bool isUnderWater = false;

                            if (useWaterTag)
                            {
                                if (Physics.Raycast(spawnPosition, Vector3.up, out RaycastHit waterHit, Mathf.Infinity))
                                {
                                    if (waterHit.collider.CompareTag("Water"))
                                    {
                                        isUnderWater = true;
                                    }
                                }
                            }
                            else
                            {
                                if (Physics.Raycast(spawnPosition, Vector3.up, out RaycastHit waterHit, Mathf.Infinity, waterLayer))
                                {
                                    isUnderWater = true;
                                }
                            }

                            if (!isUnderWater)
                            {
                                // Spawn the tree
                                GameObject tree = Instantiate(treePrefab, spawnPosition, Quaternion.identity);
                                tree.transform.SetParent(transform);
                                treesSpawned++;

                                // Spawn small rock, slightly lower and rotated 90 degrees
                                Vector3 smallRockPosition = spawnPosition + new Vector3(Random.Range(-1f, 1f), -smallRockLowerOffset, Random.Range(-1f, 1f)); // Lower it slightly            
                                GameObject smallRock = Instantiate(smallRockPrefab, smallRockPosition, Quaternion.identity);
                                smallRock.transform.SetParent(transform);
                                
                                smallRock.transform.localRotation = Quaternion.Euler(smallRockRotation); // Rotate 90 degrees on Y-axis

                                Rigidbody smallRockRb = smallRock.GetComponent<Rigidbody>();
                                if (smallRockRb != null)
                                {
                                    smallRockRb.isKinematic = true;  // Disable physics briefly to apply rotation
                                    smallRock.transform.localRotation = Quaternion.Euler(smallRockRotation);  // Apply rotation
                                    smallRockRb.isKinematic = false; // Re-enable physics
                                }
                                smallRocksSpawned++;

                                Debug.Log($"Tree and small rock spawned at: {spawnPosition}");
                                objectSpawned = true;
                            }
                            else
                            {
                                Debug.Log($"Skipped tree spawn at {spawnPosition} because it's underwater.");
                            }
                        }
                        else
                        {
                            Debug.Log($"Skipped object spawn on steep slope (angle {slopeAngle} degrees).");
                        }
                    }
                }

                attempts++;
            }

            if (!objectSpawned)
            {
                Debug.LogWarning($"Failed to place object after {maxRaycastAttempts} attempts.");
            }
        }

        Debug.Log($"Total Trees Spawned: {treesSpawned}, Total Big Rocks Spawned: {bigRocksSpawned}, Total Small Rocks Spawned: {smallRocksSpawned}");
    }

    private void Start()
    {
        SpawnTreesAndRocks();
    }
}