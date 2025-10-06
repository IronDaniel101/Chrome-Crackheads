using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarHandler : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;

    //Multipliers (sets speed for driving)
    [SerializeField] private float accelerationMultiplier = 3;
    [SerializeField] private float breaksMultiplier = 10;
    [SerializeField] private float steeringMultiplier = 5;

    //Input 
    private Vector2 input = Vector2.zero;

    private void FixedUpdate()
    {
        //Apply Acceleration
        if(input.y> 0)
            Accelerate();
        else
            rb.linearDamping = 0.2f;

        //Applying Brakes
        if (input.y < 0)
            Brake();
        
        Steer();
    }

    void Accelerate()
    {
        rb.linearDamping = 0;

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
