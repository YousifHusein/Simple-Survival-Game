using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightState : IEnemyState
{
    private float attackCooldown = 2f;
    private bool canAttack = true;

    public void EnterState(EnemyAI enemy) { }

    public void UpdateState(EnemyAI enemy)
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.target.position);

        if (distanceToPlayer > enemy.attackRange)
        {
            enemy.SwitchState(new ChaseState());
            return;
        }

        // Implement attack logic here
        if (canAttack)
        {
            ShootProjectile(enemy);
        }
    }

    public void ExitState(EnemyAI enemy) { }

    private void ShootProjectile(EnemyAI enemy)
    {
        if (enemy.projectilePrefab == null || enemy.firePoint == null) return;

        // Instantiate the projectile at the enemy's firePoint
        GameObject projectile = GameObject.Instantiate(
            enemy.projectilePrefab,
            enemy.firePoint.position,
            Quaternion.identity
        );

        // Assign the target to the projectile
        HomingProjectile homingProjectile = projectile.GetComponent<HomingProjectile>();
        if (homingProjectile != null)
        {
            homingProjectile.target = enemy.target;
        }

        Debug.Log("Enemy is shooting a projectile!");

        canAttack = false;
        enemy.StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
