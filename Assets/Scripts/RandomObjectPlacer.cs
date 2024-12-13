using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectPlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    public Terrain terrain; 
    public GameObject[] prefabs; 
    public int numberOfObjects = 100; 

    [Header("Height Constraints")]
    public float minHeight = 0.1f;
    public float maxHeight = 0.9f; 

    [Header("Object Settings")]
    public float objectScaleMin = 0.8f;
    public float objectScaleMax = 1.2f;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned.");
            return;
        }

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("No prefabs assigned.");
            return;
        }

        PlaceObjects();
    }

    void PlaceObjects()
    {
        UnityEngine.TerrainData terrainData = terrain.terrainData; 
        Vector3 terrainPosition = terrain.transform.position;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float randomX = Random.Range(0f, terrainData.size.x);
            float randomZ = Random.Range(0f, terrainData.size.z);

            float worldX = randomX + terrainPosition.x;
            float worldZ = randomZ + terrainPosition.z;
            float terrainHeight = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));

            float normalizedHeight = terrainHeight / terrainData.size.y;

            if (normalizedHeight >= minHeight && normalizedHeight <= maxHeight)
            {
                Vector3 position = new Vector3(worldX, terrainHeight + 54, worldZ);

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

                if (prefab == prefabs[4])
                {
                    position = new Vector3(worldX, terrainHeight + 55, worldZ);
                }
                else if (prefab == prefabs[5])
                {
                    position = new Vector3(worldX, terrainHeight + 55, worldZ);
                }
                else if (prefab == prefabs[3])
                {
                    position = new Vector3(worldX, terrainHeight + 55, worldZ);
                }
                else if (prefab == prefabs[2])
                {
                    position = new Vector3(worldX, terrainHeight + 55, worldZ);
                }

                GameObject instance = Instantiate(prefab, position, Quaternion.identity);
                instance.transform.parent = terrain.transform;

                float randomScale = Random.Range(objectScaleMin, objectScaleMax);
                instance.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                if (prefab == prefabs[5])
                {
                    instance.transform.rotation = Quaternion.Euler(-90, Random.Range(0, 360), 0);
                }
                else
                {
                    instance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
        }
    }
}
