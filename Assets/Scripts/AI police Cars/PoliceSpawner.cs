using UnityEngine;

public class PoliceSpawner : MonoBehaviour
{

    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private float spawnInterval = 3f; //Seconds between spawning cars.
    [SerializeField] private int maxPoliceCars = 5; //Caps the amount of cars
    [SerializeField] private float spawnHeight = 0.5f; //  Set height of spawning cars.


    private BoxCollider box;
    private float timer;
    private int currentPoliceCount;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
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
        // Random X and Z only
        Vector3 localRandomPoint = new Vector3(
            Random.Range(-box.size.x * 0.5f, box.size.x * 0.5f),
            0f, // Y = 0 in local space; we'll override world Y next
            Random.Range(-box.size.z * 0.5f, box.size.z * 0.5f)
        );

        Vector3 worldPoint = box.transform.TransformPoint(box.center + localRandomPoint);

        // Force a fixed spawn height
        float spawnHeight = transform.position.y; 
        worldPoint.y = spawnHeight;

        GameObject police = Instantiate(policeCarPrefab, worldPoint, Quaternion.identity);

        currentPoliceCount++;
    }

}
