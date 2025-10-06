using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CarHandler : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;

    //Multipliers (sets speed for driving)
    [Header("Multipliers")]
    [SerializeField] private float accelerationMultiplier = 3f;
    [SerializeField] private float breaksMultiplier = 10f;
    [SerializeField] private float steeringMultiplier = 5f;

    //Input 
    private Vector2 input = Vector2.zero;


    public void Move(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();

        //Debug (Prints only when you move left/right past a threshold)
        if (input.x > 0.2f) Debug.Log("Left STICK -> RIGHT");
        if (input.x > -0.2f) Debug.Log("Left STICK -> LEFT");

    }

    private void FixedUpdate()
    {
        //Apply Acceleration
        if(input.y> 0) Accelerate();
        else        rb.linearDamping = 0.2f;

        //Applying Brakes
        if (input.y < 0f) Brake();
        Steer();
    }

    void Accelerate()
    {
        rb.linearDamping = 0f;
        rb.AddForce(rb.transform.forward * accelerationMultiplier * input.y);
    }

    void Brake()
    {
        //Don't Brake unless vehicle is moving forward
        if(rb.linearVelocity.z <= 0)
            return;

        rb.AddForce(rb.transform.forward * breaksMultiplier * input.y);
    }

    void Steer()
    {
        if (Mathf.Abs(input.x) > 0)
        {
            rb.AddForce(rb.transform.right * steeringMultiplier * input.x);
        }
    }

    public void SetInput(Vector2 inputVector)
    {
        inputVector.Normalize();

        input = inputVector;
    }

}
