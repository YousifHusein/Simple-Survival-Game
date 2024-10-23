using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwing : MonoBehaviour
{
    public Rigidbody weaponRb;
    public float swingForce = 10f;

    private bool swinging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button
        {
            Swing();
        }
    }

    void Swing()
    {
        // Apply a force to simulate a physical swing
        weaponRb.AddForce(transform.forward * swingForce, ForceMode.Impulse);
        swinging = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Getting here!!!");
        if (swinging && collision.gameObject.CompareTag("Tree"))
        {
            Tree tree = collision.gameObject.GetComponent<Tree>();
            if (tree != null)
            {
                tree.TakeDamage(10);  // Apply damage to the tree
                Debug.Log("Getting here");
            }

            swinging = false;  // Stop swinging after the hit
        }
    }
}
