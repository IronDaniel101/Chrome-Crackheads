using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 4;
    [SerializeField] private int maxCoins = 8;

    [Header("Spawn Settings")]
    [SerializeField] private float edgePadding = 0.5f;
    [SerializeField] private float heightOffset = 0.1f; // small lift above ground
    [SerializeField] private LayerMask groundLayers = ~0; // everything by default

    [Header("Rotation & Scale")]
    [Tooltip("If true, coin 'up' will match the road surface. Your prefab rotation is preserved on top of that.")]
    [SerializeField] private bool alignToSurface = false;

    [Tooltip("Extra rotation applied after alignment & prefab rotation (leave 0,0,0 if your prefab already looks right).")]
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("Locks coin world-size to the prefab's world scale so parenting won't stretch it.")]
    [SerializeField] private bool preservePrefabScale = true;

    private BoxCollider area;
    private List<GameObject> spawnedCoins = new();
    private Vector3 prefabWorldScale;

    private void Awake()
    {
        area = GetComponent<BoxCollider>();
        // Cache the coin's intended world size (usually 1,1,1). If the prefab has a child mesh,
        // keep the scale on the root coin prefab, not just the mesh.
        prefabWorldScale = (coinPrefab != null) ? coinPrefab.transform.lossyScale : Vector3.one;
    }

    private void OnEnable()
    {
        SpawnCoins();
    }

    private void OnDisable()
    {
        ClearCoins();
    }

    public void SpawnCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning($"No coin prefab assigned for {name}");
            return;
        }

        ClearCoins();

        int count = Random.Range(minCoins, maxCoins + 1);
        Bounds b = area.bounds;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(b.min.x + edgePadding, b.max.x - edgePadding),
                b.center.y + 5f, // start above to raycast down
                Random.Range(b.min.z + edgePadding, b.max.z - edgePadding)
            );

            // Raycast down to find surface height
            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 20f, groundLayers))
            {
                Vector3 spawnPos = hit.point + Vector3.up * heightOffset;

                // --- Rotation ---
                // Keep the prefab's rotation (e.g., your 90° X) and optionally align to the surface.
                Quaternion prefabRot   = coinPrefab.transform.rotation;
                Quaternion surfaceAlign = alignToSurface
                    ? Quaternion.FromToRotation(Vector3.up, hit.normal)
                    : Quaternion.identity;
                Quaternion extraOffset = Quaternion.Euler(rotationOffsetEuler);

                // First align to ground, then apply the prefab look, then any extra offset.
                Quaternion finalRot = surfaceAlign * prefabRot * extraOffset;

                // --- Instantiate without parent to avoid inheriting scale/rotation ---
                GameObject coin = Instantiate(coinPrefab, spawnPos, finalRot);

                // Ensure correct world size (prevents stretching if parent is scaled).
                if (preservePrefabScale)
                    coin.transform.localScale = prefabWorldScale;

                // Now parent it, preserving world transform so nothing changes visually.
                coin.transform.SetParent(transform, worldPositionStays: true);

                spawnedCoins.Add(coin);
            }
        }
    }

    private void ClearCoins()
    {
        for (int i = 0; i < spawnedCoins.Count; i++)
        {
            if (spawnedCoins[i])
                Destroy(spawnedCoins[i]);
        }
        spawnedCoins.Clear();
    }
}