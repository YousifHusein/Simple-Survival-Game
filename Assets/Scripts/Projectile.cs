using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10; // Default damage
    public float lifespan = 5f; // Time before the projectile is destroyed
    public float speed = 20f;

    private Rigidbody rb;

    void Start()
    {
        Destroy(gameObject, lifespan); // Destroy after a set time

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed; // Set forward velocity
        }
    }

    void Update()
    {
        // If no Rigidbody is attached, move the projectile manually
        if (rb == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        // Destroy the projectile on impact
        Destroy(gameObject);
    }
}
