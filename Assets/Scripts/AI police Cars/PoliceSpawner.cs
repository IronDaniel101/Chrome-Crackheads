using UnityEngine;

public class PoliceSpawner : MonoBehaviour
{

    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private float spawnInterval = 3f; //Seconds between spawning cars.
    [SerializeField] private int maxPoliceCars = 5; //Caps the amount of cars


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
        //Get a random point inside the Box Collider (World Space)

    }




}
