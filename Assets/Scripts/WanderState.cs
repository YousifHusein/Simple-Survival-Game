using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : IEnemyState
{
    private Vector3 wanderTarget;
    private float wanderTimer = 0f;
    private float timeToNewTarget = 2f; // Time before choosing a new wander target

    public void EnterState(EnemyAI enemy)
    {
        Debug.Log("Entering WanderState");
        SetRandomDestination(enemy);
    }

    public void UpdateState(EnemyAI enemy)
    {
        // Check if the player is within detection range
        if (Vector3.Distance(enemy.transform.position, enemy.target.position) <= enemy.detectionRange)
        {
            Debug.Log("Player detected! Switching to ChaseState.");
            enemy.SwitchState(new ChaseState());
            return;
        }

        // Move toward the wander target
        float distanceToTarget = Vector3.Distance(enemy.transform.position, wanderTarget);
        if (distanceToTarget <= enemy.nodeReachThreshold || wanderTimer >= timeToNewTarget)
        {
            SetRandomDestination(enemy);
            wanderTimer = 0f;
        }
        else
        {
            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position,
                wanderTarget,
                enemy.speed * Time.deltaTime
            );
        }

        // Update timer
        wanderTimer += Time.deltaTime;
    }

    public void ExitState(EnemyAI enemy)
    {
        Debug.Log("Exiting WanderState");
    }

    private void SetRandomDestination(EnemyAI enemy)
    {
        float wanderRadius = 15f; // Define the radius for wandering
        Vector3 randomOffset = new Vector3(
            Random.Range(-wanderRadius, wanderRadius),
            0,
            Random.Range(-wanderRadius, wanderRadius)
        );

        wanderTarget = enemy.transform.position + randomOffset;

        // Align wanderTarget with terrain height if needed
        if (Physics.Raycast(new Vector3(wanderTarget.x, 100f, wanderTarget.z), Vector3.down, out RaycastHit hit, 200f))
        {
            wanderTarget.y = hit.point.y;
        }
    }
}