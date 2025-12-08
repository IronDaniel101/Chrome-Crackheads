using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICarSpawner : MonoBehaviour
{
    [Header("AI Car Setup")]
    [SerializeField] private GameObject aiCarPrefab;
    [SerializeField] private Transform player;          // player car transform

    [SerializeField] private float spawnInterval = 2f;  // Seconds between spawning cars
    [SerializeField] private int maxAICars = 10;        // Maximum active AI cars
    [SerializeField] private float spawnHeight = 0.5f;  // World Y position for spawned cars

    private BoxCollider box;
    private float timer;
    private int currentAICount;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();

        if (box == null)
        {
            Debug.LogError("AICarSpawner requires a BoxCollider on the same GameObject.");
        }

        // Fallback if not assigned in Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || box == null || aiCarPrefab == null)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentAICount < maxAICars)
        {
            timer = 0f;
            SpawnAICar();
        }
    }

   private void SpawnAICar()
    {
        if (box == null || aiCarPrefab == null)
            return;

        // Random position inside BoxCollider (local space)
        Vector3 localRandomPoint = new Vector3(
            Random.Range(-box.size.x * 0.5f, box.size.x * 0.5f),
            0f,
            Random.Range(-box.size.z * 0.5f, box.size.z * 0.5f)
        );

        // Convert to world space
        Vector3 worldPoint = box.transform.TransformPoint(box.center + localRandomPoint);

        // Force a fixed spawn height
        worldPoint.y = spawnHeight;

        //New: flat direction toward player
        Vector3 toPlayer = player.position - worldPoint;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
        {
            toPlayer = -player.forward;
            toPlayer.y = 0f;
        }

        Quaternion rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

        GameObject aiCar = Instantiate(aiCarPrefab, worldPoint, rotation);
        currentAICount++;

        var handler = aiCar.GetComponent<AIHandler>();
        if (handler != null)
        {
            handler.Initialize(player, this);
        }
        else
        {
            Debug.LogWarning("Spawned AI car prefab is missing AIHandler component.");
        }
    }

    public void OnAICarDestroyed()
    {
        currentAICount = Mathf.Max(0, currentAICount - 1);
    }
}