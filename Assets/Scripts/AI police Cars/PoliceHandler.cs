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
    private bool hasTriggeredFail = false;

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
        if (player == null || hasTriggeredFail) 
            return;

        Vector3 policeInPlayerSpace = player.InverseTransformPoint(transform.position);

        if (policeInPlayerSpace.z > passDistanceAhead)
        {
            hasTriggeredFail = true;  // prevent future triggers

            Debug.Log("Police passed the player! Mission failed.");

            // Call mission failed logic ONCE here
            MissionFailed();
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

    private void MissionFailed()
    {
        // Trigger your lose screen, reload scene, etc.
        // Example:
        // GameManager.Instance.ShowMissionFailedScreen();

        Debug.Log("MISSION FAILED EVENT TRIGGERED");
    }
}
