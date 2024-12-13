using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    public PathFinding pathfinding;
    public Transform target;
    public float speed = 5f;
    public float aStarSpeed = 10f;
    public float updateInterval = 0.2f;
    public float pathUpdateRadius = 10f;
    public float nodeReachThreshold = 2f;

    public float detectionRange = 10f;
    public float attackRange = 3f;

    private List<Node> path;
    private int currentPathIndex = 0;
    private float pathTimer = 0f;
    private Vector3 lastTargetPosition;

    private IEnemyState currentState;

    public GameObject projectilePrefab;
    public Transform firePoint;

    void Start()
    {
        currentState = new WanderState(); // Set initial state
        currentState.EnterState(this);
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void SwitchState(IEnemyState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;
        currentState.EnterState(this);
    }

    public void UpdatePath(Vector3 destination)
    {
        if (pathfinding != null && target != null)
        {
            // Only recalculate the path if:
            // - Target moved more than nodeReachThreshold
            // - Path is null or empty
            if ((Vector3.Distance(target.position, lastTargetPosition) > nodeReachThreshold) || path == null || path.Count == 0)
            {
                path = pathfinding.FindPath(transform.position, target.position);
                currentPathIndex = 0; // Reset the path index
                lastTargetPosition = target.position;
            }
        }
    }

    public void MoveAlongPath()
    {
        if (path == null || currentPathIndex >= path.Count)
        {
            return;
        }

        // Get the current node in the path
        Node currentNode = path[currentPathIndex];
        Vector3 targetPosition = currentNode.position;

        // Move toward the target position
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, aStarSpeed * Time.deltaTime));

        // Check if the enemy is close enough to the current node
        float distanceToNode = Vector3.Distance(transform.position, targetPosition);

        if (distanceToNode <= nodeReachThreshold)
        {
            currentPathIndex++;
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(path[i].position, path[i + 1].position);
            }

            foreach (var node in path)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.position, 0.2f);
            }
        }

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pathUpdateRadius);
        }
    }
}