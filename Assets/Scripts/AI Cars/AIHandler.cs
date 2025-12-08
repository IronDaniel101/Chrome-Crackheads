using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float minSpeed = 20f;
    [SerializeField] private float maxSpeed = 35f;

    [Header("Despawn Settings")]
    [SerializeField] private float despawnDistanceBehind = 20f;   // 20 units behind in player local space
    [SerializeField] private float despawnCheckDelay    = 0.25f;  // wait a bit before we allow despawn

    private float speed;
    private float lifeTimer = 0f;

    private Transform player;
    private AICarSpawner spawner;

    //Link to Explosion
    private static GameObject explosionPrefabStatic;

    // Called by AICarSpawner after Instantiate()
    public void Initialize(Transform playerTransform, AICarSpawner owner)
    {
        player  = playerTransform;
        spawner = owner;

        // Random speed for this car
        speed = Random.Range(minSpeed, maxSpeed);
    }

    private void Awake()
    {
        // Fallback: find player if Initialize hasn't set it yet
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (speed <= 0f)
            speed = Random.Range(minSpeed, maxSpeed);
    }

    private void OnEnable()
    {
        // Reset lifetime so despawn delay works correctly when reused from a pool
        lifeTimer = 0f;

        if (speed <= 0f)
            speed = Random.Range(minSpeed, maxSpeed);
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        // Move along local Z (forward)
        transform.position += transform.forward * speed * Time.deltaTime;

        // Only start considering despawn after a short delay so we don't insta-kill on spawn
        if (lifeTimer >= despawnCheckDelay)
            CheckDespawnBehindPlayer();
    }

    private void CheckDespawnBehindPlayer()
    {
        if (player == null)
            return;

        // Same style as PoliceHandler: convert to player local space
        Vector3 carInPlayerSpace = player.InverseTransformPoint(transform.position);

        // Debug to verify what's going on:
        // Debug.Log($"[AI] localZ={carInPlayerSpace.z:F1}");

        // Despawn when far enough behind the player
        if (carInPlayerSpace.z < -despawnDistanceBehind)
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (spawner != null)
            spawner.OnAICarDestroyed();

        // Return to pool
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SpawnExplosion(transform.position);
        Despawn();
    }
    private void SpawnExplosion(Vector3 position)
    {
        // Attempt to load explosion prefab only once
        if (explosionPrefabStatic == null)
        {
            explosionPrefabStatic = Resources.Load<GameObject>("Explosion");
            
            if (explosionPrefabStatic == null)
            {
                Debug.LogError("Explosion prefab not found! Make sure 'Explosion.prefab' is inside a Resources folder.");
                return;
            }
        }

        GameObject vfx = Instantiate(explosionPrefabStatic, position, Quaternion.identity);
        Destroy(vfx, 3f); // cleanup
    }
}