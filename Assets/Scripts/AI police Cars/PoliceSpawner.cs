using UnityEngine;
using TMPro;

public class PoliceSpawner : MonoBehaviour
{
    [Header("Police Setup")]
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private Transform player;          // player car transform

    [SerializeField] private float spawnInterval = 3f;  // Seconds between spawning cars
    [SerializeField] private int maxPoliceCars = 5;     // Maximum active police cars
    [SerializeField] private float spawnHeight = 0.5f;  // World Y position for spawned cars

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText; // center 3..2..1 text
    [SerializeField] private GameObject caughtPopup;        // "caught" panel

    private BoxCollider box;
    private float timer;
    private int currentPoliceCount;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();

        if (box == null)
        {
            Debug.LogError("PoliceSpawner requires a BoxCollider on the same GameObject.");
        }

        // Ensure UI is hidden at start
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (caughtPopup != null)
            caughtPopup.gameObject.SetActive(false);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentPoliceCount < maxPoliceCars)
        {
            timer = 0f;
            SpawnPoliceCar();
        }
    }

    private void SpawnPoliceCar()
    {
        if (box == null || policeCarPrefab == null)
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

        GameObject police = Instantiate(policeCarPrefab, worldPoint, Quaternion.identity);
        currentPoliceCount++;

        // Hook up PoliceHandler
        var handler = police.GetComponent<PoliceHandler>();
        if (handler != null)
        {
            handler.Initialize(player);           // player transform
            handler.countdownText = countdownText;
            handler.caughtPopup = caughtPopup;
            handler.SetSpawner(this);
        }
        else
        {
            Debug.LogWarning("Spawned police car prefab is missing PoliceHandler component.");
        }
    }

    public void OnPoliceDestroyed()
    {
        currentPoliceCount = Mathf.Max(0, currentPoliceCount - 1);
    }
}