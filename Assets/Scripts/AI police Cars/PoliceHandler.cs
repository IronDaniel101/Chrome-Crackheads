using UnityEngine;
using System.Collections.Generic;

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

    [SerializeField] private float sirenMinPitch = 0.9f;
    [SerializeField] private float sirenMaxPitch = 1.1f;

    private AudioSource siren;

    public float health = 100f;

    // Track all active police cars so we can decide which one is "loudest"
    private static List<PoliceHandler> allPolice = new List<PoliceHandler>();
    private static Transform listenerTransform;

    private void Awake()
    {
        // Cache the AudioListener (usually on the main camera)
        if (listenerTransform == null)
        {
            var listener = FindObjectOfType<AudioListener>();
            if (listener != null)
            {
                listenerTransform = listener.transform;
            }
        }
    }

    void Start()
    {

        siren = GetComponent<AudioSource>();
        if (siren != null)
        {
            siren.loop = true;
            siren.playOnAwake = false; // we will control when it plays
            siren.pitch = Random.Range(sirenMinPitch, sirenMaxPitch);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Always drive forward
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        // Check if passed the player
        CheckIfPassedPlayer();

        HandleSirenAudio();
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

    private void OnEnable()
    {
        if (!allPolice.Contains(this))
        allPolice.Add(this);
    }

    private void OnDisable()
    {
        allPolice.Remove(this);
    }


    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }


    private void HandleSirenAudio()
    {
        if (siren == null || allPolice.Count == 0)
            return;

        // Decide what point we measure from: listener first, then player as backup
        Transform reference = listenerTransform != null ? listenerTransform : player;
        if (reference == null)
            return;

        // Find the police car closest to the reference point (camera/listener)
        PoliceHandler closest = null;
        float closestDistSq = Mathf.Infinity;

        foreach (var p in allPolice)
        {
            if (p == null) 
                continue;

            Vector3 toRef = p.transform.position - reference.position;
            float dSq = toRef.sqrMagnitude; // cheaper than Distance

            if (dSq < closestDistSq)
            {
                closestDistSq = dSq;
                closest = p;
            }
        }

        // Only the closest car plays its siren; others are muted
        if (closest == this)
        {
            siren.mute = false;
            if (!siren.isPlaying)
                siren.Play();
        }
        else
        {
            siren.mute = true;
        }
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

        if (siren != null)
        siren.Stop();

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
