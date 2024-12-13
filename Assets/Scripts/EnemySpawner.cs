using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnRadius = 20f;
    public int baseMaxEnemies = 10;
    public float spawnInterval = 5f;

    [Header("Day/Night Settings")]
    public TimeManager timeManager;
    public int nightStartHour = 6;
    public int nightEndHour = 2;

    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (IsNight() && activeEnemies.Count < GetMaxEnemiesForDay())
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = player.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = player.position.y;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<EnemyAI>().target = player.transform.Find("PlayerObj");
        enemy.GetComponent<PathFinding>().grid = FindObjectOfType<Grid>();

        activeEnemies.Add(enemy);

        enemy.GetComponent<Health>().OnDeath += () => activeEnemies.Remove(enemy);
    }

    bool IsNight()
    {
        int currentHour = timeManager.Hours;
        return (currentHour >= nightStartHour || currentHour < nightEndHour);
    }

    int GetMaxEnemiesForDay()
    {
        return baseMaxEnemies + (timeManager.Days * 4);
    }
}
