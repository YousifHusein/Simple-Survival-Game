using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    public bool isInventoryOpen;

    private void Awake()
    {
        isInventoryOpen = false;
    }

    private void Update()
    {
        if (!isInventoryOpen)
        {
            transform.position = cameraPosition.position;
        }
    }
}
