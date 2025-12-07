using UnityEngine;

public class PoliceHandler : MonoBehaviour
{
    // Speed of Police Car (Needs to be faster than player car)
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 35f;


    // Player reference for passDistance to cause loss
    [Header("Player Reference")]
    [SerializeField] private Transform player;       
    [SerializeField] private float passDistanceAhead = 10f;

    public float health = 100f;

    // Update is called once per frame
    private void Update()
    {
         // Always drive forward
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        // Check if passed the player
        CheckIfPassedPlayer();
    }

    private void CheckIfPassedPlayer()
    {
        if (player == null) return; //if no player, then return.

        // Convert police position into the player's local space
        Vector3 policeInPlayerSpace = player.InverseTransformPoint(transform.position);

        // If Z is positive and large enough, then it will play the loss logic.
        if (policeInPlayerSpace.z > passDistanceAhead)
        {
            Debug.Log("Police passed the player! Mission failed.");

            // This should then play "Mission Failed" Logic because the police car has overtaken the player.
        }
    }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    //Added damage system for police cars
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Police car destroyed!");
        Destroy(gameObject);
    }
}
