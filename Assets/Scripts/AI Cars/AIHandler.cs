using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour
{
    [SerializeField]
    CarHandler carHandler;

    private void Awake()
    {
        if (CompareTag("Player"))
        {
            Destroy(this);
            return;
        }
    }
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float accelerationInput = 1.0f;

        float steerInput = 0.0f;

        steerInput = Mathf.Clamp(steerInput, -1.0f, 1.0f);

        carHandler.SetInput(new Vector2(steerInput, accelerationInput));
    }

    //Events
    private void OnEnable()
    {
        carHandler.SetMaxSpeed(Random.Range(2, 4));
    }


}
