using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CarHandler : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;



    [SerializeField]
    Transform gameModel;

    //Max Amounts
    float maxSteerVelocity = 2;
    float maxForwardVelocity = 30;

    //Multipliers (sets speed for driving)
    [Header("Multipliers")]
    [SerializeField] private float accelerationMultiplier = 3f;
    [SerializeField] private float breaksMultiplier = 10f;
    [SerializeField] private float steeringMultiplier = 5f; 

    //Boost System For Controller
    [SerializeField] private float boostSpeed = 60f;     // fixed boost top speed
    [SerializeField] private float boostDuration = 4f;   // how long boost lasts
    [SerializeField] private float boostCooldown = 20f;  // time between boosts

    private bool isBoosting = false;
    private float originalMaxSpeed;
    private float lastBoostEndTime = -Mathf.Infinity;


    //Input 
    private Vector2 input = Vector2.zero;



    //Stats
    float carStartPositionZ;
    float distanceTravelled = 0;
    public float DistanceTravelled => distanceTravelled;


    public void Move(InputAction.CallbackContext ctx)
    {
        Vector2 raw = ctx.ReadValue<Vector2>();

        // X = steering, Y = only for braking (down)
        input = new Vector2(raw.x, Mathf.Min(raw.y, 0f));

        // Debug (Prints only when you move left/right past a threshold)
        if (input.x > 0.2f) Debug.Log("Left STICK -> RIGHT");
        if (input.x < -0.2f) Debug.Log("Left STICK -> LEFT");
    }

    void Start()
    {
        carStartPositionZ = transform.position.z;
        originalMaxSpeed = maxForwardVelocity;   // normal top speed
    }

    void Update()
    {
        //Rotate Car Model When "Turning"
        //rb.linearVelocity.x * 5

        float yaw = 180f + rb.linearVelocity.x * 5f;
        gameModel.transform.rotation = Quaternion.Euler(0, yaw, 0);

        //Update Distance Travelled
        distanceTravelled = transform.position.z - carStartPositionZ;
        
    }

    private void FixedUpdate()
    {
        // Always try to accelerate up to max speed
        Accelerate();

        // Allow braking only when pushing the stick down
        if (input.y < 0f)
            Brake();

        // Steering still uses input.x
        Steer();
    }

    void Accelerate()
    {
        rb.linearDamping = 0f;

        // Stay within the speed limit
        if (rb.linearVelocity.z >= maxForwardVelocity)
            return;

        // Constant push forward
        rb.AddForce(rb.transform.forward * accelerationMultiplier);
    }

    public void Boost(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        // Don't start a new boost while already boosting
        if (isBoosting)
            return;

        // Only lock behind the timer
        bool cooldownReady = Time.time >= lastBoostEndTime + boostCooldown;

        if (!cooldownReady)
        {
            Debug.Log("Boost on cooldown");
            return;
        }

        StartCoroutine(BoostRoutine());
    }

    void Brake()
    {
        // Don't brake unless vehicle is moving forward
        if (rb.linearVelocity.z <= 0)
            return;

        rb.AddForce(rb.transform.forward * breaksMultiplier * input.y);
    }

    void Steer()
    {
        if (Mathf.Abs(input.x) > 0)
        {
            //Moves car sideways if car is at least moving by 5 units
            float speedBaseSteerLimit = rb.linearVelocity.z / 5.0f;
            speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);

            rb.AddForce(rb.transform.right * steeringMultiplier * input.x * speedBaseSteerLimit);

            //Normalize the X Velocity
            float normalizedX = rb.linearVelocity.x / maxSteerVelocity;

            //Ensure that we Don't allow it to get bigger than 1 in magnitued.
            normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

            //Make sure we stay within the turn speed limit
            rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
        }
        else
        {
            //Auto Center Car
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, 0, rb.linearVelocity.z), Time.fixedDeltaTime * 3);
        }
    }

    public void SetInput(Vector2 inputVector)
    {
        inputVector.Normalize();

        input = inputVector;
    }


    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxForwardVelocity = newMaxSpeed;
    }

    private IEnumerator BoostRoutine()
    {
        isBoosting = true;

        if (originalMaxSpeed <= 0f)
            originalMaxSpeed = maxForwardVelocity;

        // Apply boost
        maxForwardVelocity = boostSpeed;
        Debug.Log("BOOST START");

        yield return new WaitForSeconds(boostDuration);

        // Restore normal speed
        maxForwardVelocity = originalMaxSpeed;

        // SNAP SPEED BACK TO NORMAL HERE
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            rb.linearVelocity.y,
            originalMaxSpeed
        );

        isBoosting = false;
        lastBoostEndTime = Time.time;
        Debug.Log("BOOST END");
    }

}
