using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
    public void EnterState(EnemyAI enemy) { }

    public void UpdateState(EnemyAI enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        if (distanceToPlayer > enemy.detectionRange)
        {
            enemy.SwitchState(new WanderState());
            return;
        }

        if (distanceToPlayer <= enemy.attackRange)
        {
            enemy.SwitchState(new FightState());
            return;
        }

        enemy.UpdatePath(enemy.target.position);
        enemy.MoveAlongPath();
    }

    public void ExitState(EnemyAI enemy) { }
}
