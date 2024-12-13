using System.Collections;
using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public Transform target; // The target to track (e.g., the player)
    public float speed = 10f; // Speed of the projectile
    public float lifespan = 5f; // Time before the projectile is destroyed
    public int damage = 10; // Damage dealt by the projectile

    void Start()
    {
        Destroy(gameObject, lifespan); // Destroy after a set time
    }

    void Update()
    {
        if (target == null) return;

        // Move toward the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate to face the target
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // Apply damage to the target
            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }

            Destroy(gameObject); // Destroy on impact
        }
    }
}
