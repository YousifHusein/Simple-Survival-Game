using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public HotbarInventoryManager hotbarInventoryManager;

    public Animator weaponAnimator;
    public float shootCooldown = 0.5f;
    private bool canShoot = true;

    public GameObject projectilePrefab; // The projectile to shoot
    public Transform firePoint; // The point where the projectile spawns
    public int damage;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canShoot && hotbarInventoryManager.equippedItem != null && hotbarInventoryManager.equippedItem.CompareTag("Wand"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        weaponAnimator.SetTrigger("ShootTrigger");

        // Instantiate the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Set the projectile's direction and velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * 20f; // Adjust the speed as needed
        }

        // Assign damage to the projectile
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = damage;
        }

        canShoot = false;
        StartCoroutine(ShootCooldown());
    }

    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
}
