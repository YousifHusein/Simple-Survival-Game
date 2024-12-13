using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Grid : MonoBehaviour
{
    public int gridSizeX, gridSizeY;
    public float nodeSize;
    public LayerMask obstacleLayer;
    public LayerMask groundLayer;
    public float maxSlopeAngle = 30f;

    private Node[,] grid;

    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                // Calculate the world position of the node
                Vector3 worldPoint = transform.position + new Vector3(x * nodeSize + nodeSize / 2, 0, z * nodeSize + nodeSize / 2);

                // Raycast to find the height of the ground
                if (Physics.Raycast(worldPoint + Vector3.up * 200, Vector3.down, out RaycastHit hit, 200, groundLayer))
                {
                    worldPoint.y = hit.point.y; // Align node to ground height

                    // Check slope angle for walkability
                    bool walkable = Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle;

                    // Also check for obstacles
                    if (walkable)
                    {
                        walkable = !Physics.CheckSphere(worldPoint, nodeSize / 2, obstacleLayer);
                    }

                    // Create the node
                    grid[x, z] = new Node
                    {
                        position = worldPoint,
                        isWalkable = walkable
                    };
                }
                else
                {
                    // If no ground was detected, mark the node as non-walkable
                    grid[x, z] = new Node
                    {
                        position = worldPoint,
                        isWalkable = false
                    };
                }
            }
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPoint)
    {
        // Offset the world position by the grid's origin
        float gridOriginX = transform.position.x;
        float gridOriginZ = transform.position.z;

        // Calculate the relative position within the grid
        float relativeX = (worldPoint.x - gridOriginX) / nodeSize;
        float relativeZ = (worldPoint.z - gridOriginZ) / nodeSize;

        // Convert relative positions to grid indices
        int x = Mathf.FloorToInt(relativeX);
        int z = Mathf.FloorToInt(relativeZ);

        // Clamp the indices to ensure they stay within bounds
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        z = Mathf.Clamp(z, 0, gridSizeY - 1);

        // Return the correct node
        return grid[x, z];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Calculate the node's indices in the grid
        int nodeX = Mathf.FloorToInt((node.position.x - transform.position.x) / nodeSize);
        int nodeZ = Mathf.FloorToInt((node.position.z - transform.position.z) / nodeSize);

        // Check all possible neighbors
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                // Skip the current node itself
                if (dx == 0 && dz == 0) continue;

                // Calculate neighbor indices
                int checkX = nodeX + dx;
                int checkZ = nodeZ + dz;

                // Ensure neighbor indices are within grid bounds
                if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeY)
                {
                    Node neighbor = grid[checkX, checkZ];
                    if (neighbor.isWalkable) // Only add walkable neighbors
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    /*private void OnDrawGizmos()
    {
        if (grid == null) return;

        // Draw the grid boundary
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(gridSizeX * nodeSize / 2, 0, gridSizeY * nodeSize / 2),
                            new Vector3(gridSizeX * nodeSize, 0, gridSizeY * nodeSize));

        // Draw all nodes
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Node node = grid[x, z];
                Gizmos.color = node.isWalkable ? Color.green : Color.red;
                Gizmos.DrawWireCube(node.position, Vector3.one * nodeSize);
            }
        }
    }*/

}
