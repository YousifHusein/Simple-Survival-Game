using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class EToPickup : MonoBehaviour
{
    private bool playerInRange = false;
    public Canvas pickupMessageCanvas;
    public TMP_Text pickupMessageText;
    public Item item;
    public float offset = 2.0f;
    private Camera mainCamera;
    public float fadeDuration = 1.0f;

    public InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (pickupMessageCanvas != null)
        {
            pickupMessageCanvas.gameObject.SetActive(false);
        }

        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (pickupMessageCanvas != null)
            {
                pickupMessageCanvas.gameObject.SetActive(true);
                StartCoroutine(FadeTextIn());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (pickupMessageCanvas != null)
            {
                StartCoroutine(FadeTextOut());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Optional: Move the Canvas if needed to hover over the item dynamically
        if (pickupMessageCanvas != null)
        {
            pickupMessageCanvas.transform.position = transform.position + Vector3.up * offset;
            pickupMessageCanvas.transform.LookAt(mainCamera.transform);
            pickupMessageCanvas.transform.Rotate(0, 180, 0);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        inventoryManager.AddItem(item, 1);
        Destroy(gameObject);
        pickupMessageCanvas.gameObject.SetActive(false);
    }

    IEnumerator FadeTextIn()
    {
        float elapsedTime = 0f;
        Color textColor = pickupMessageText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            pickupMessageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }
    }

    IEnumerator FadeTextOut()
    {
        float elapsedTime = 0f;
        Color textColor = pickupMessageText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            pickupMessageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }

        // Hide the canvas after fading out
        pickupMessageCanvas.gameObject.SetActive(false);
    }
}
