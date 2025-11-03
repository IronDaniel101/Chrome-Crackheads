using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICarSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject[] carAIPrefabs;

    GameObject[] carAIPool = new GameObject[20];

    Transform playerCarTransform;

    WaitForSeconds wait = new WaitForSeconds(0.5f);

    //Timing
    float timeLastCarSpawned = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;

        for (int i = 0; i < carAIPool.Length; i++)
        {
            carAIPool[i] = Instantiate(carAIPrefabs[prefabIndex]);
            carAIPool[i].SetActive(false);

            prefabIndex++;

            //Loop the Prefab index if we run out of prefabs
            if (prefabIndex > carAIPrefabs.Length - 1)
                prefabIndex = 0;
        }

        StartCoroutine(UpdateLessOftenCO());
    }

    IEnumerator UpdateLessOftenCO()
    {
        while (true)
        {
            CleanUpCarsBeyondView();
            SpawnNewCars();

            yield return wait;
        }
    }

    void SpawnNewCars()
    {
        if (Time.time - timeLastCarSpawned < 2)
            return;

        GameObject carToSpawn = null;

        //Find a car to spawn
        foreach (GameObject aiCar in carAIPool)
        {
            //Skip active cars
            if (aiCar.activeInHierarchy)
                continue;

            carToSpawn = aiCar;
            break;
        }

        //No available car to spawn
        if (carToSpawn == null)
            return;

        Vector3 spawnPosition = new Vector3(0, 0, playerCarTransform.transform.position.z + 100);

        carToSpawn.transform.position = spawnPosition;
        carToSpawn.SetActive(true);

        timeLastCarSpawned = Time.time;
    }

    void CleanUpCarsBeyondView()
    {
        foreach (GameObject aiCar in carAIPool)
        {
            //Skip inactive cars
            if (!aiCar.activeInHierarchy)
                continue;

            //Check if AI car is too far ahead
            if (aiCar.transform.position.z - playerCarTransform.position.z > 200)
                aiCar.SetActive(false);

            //check if AI car is too far behind
            if (aiCar.transform.position.z - playerCarTransform.position.z < -50)
                aiCar.SetActive(false);
            
        }
    }

}
