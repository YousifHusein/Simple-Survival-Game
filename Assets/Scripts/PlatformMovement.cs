using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using TMPro;

public class PlatformMovement : MonoBehaviour
{
    public float speed = 5f;

    public int time;

    public GameObject[] terrains;
    private GameObject pickedTerrain;

    private bool moving = true;
    private bool waiting;

    public TMP_Text timer;
    private string currentPhase;
    private int currentTime;

    private void Start()
    {
        currentPhase = "Finding";
        currentTime = time;
        timer.text = currentPhase + ": " + currentTime.ToString();
        StartCoroutine(RepeatActionEvery30Seconds());
        StartCoroutine(UpdateTimer());
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (moving)
        {
            collision.gameObject.transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    IEnumerator RepeatActionEvery30Seconds()
    {
        Debug.Log("I entered the first 30 seconds timer");
        while (true)
        {
            if (!waiting)
            {
                currentPhase = "Finding";
                currentTime = time;
                yield return new WaitForSeconds(time);
                Debug.Log("Waited 10 seconds");
                if (pickedTerrain != null)
                {
                    pickedTerrain.SetActive(false);
                    moving = true;
                    pickedTerrain = null;
                }
                int random = Random.Range(0, 2);
                Debug.Log("Picked a random number: " + random);
                if (random == 1)
                {
                    random = Random.Range(0, terrains.Length);
                    Debug.Log("Length: " + terrains.Length);
                    pickedTerrain = terrains[random];
                    Debug.Log("Picked terrain: " + pickedTerrain.name);
                }

                if (pickedTerrain != null)
                {
                    moving = false;
                    pickedTerrain.transform.position = transform.position;
                    pickedTerrain.transform.position = pickedTerrain.transform.position + new Vector3(-50, -48, -50);
                    pickedTerrain.SetActive(true);
                    currentPhase = "Loot";
                    currentTime = 20;
                    StartCoroutine(WaitFor5Minutes());
                    waiting = true;
                }
            }
            yield return null;
        }
    }

    IEnumerator WaitFor5Minutes()
    {
        yield return new WaitForSeconds(20);
        waiting = false;
    }

    IEnumerator UpdateTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            currentTime -= 1;
            timer.text = currentPhase + ": " + currentTime.ToString();

            if (currentTime <= 0)
            {
                currentTime = 0;
            }
        }
    }
}
