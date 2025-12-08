using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PoliceHandler : MonoBehaviour
{
    // Speed of Police Car (Needs to be faster than player car)
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 35f;

    // Player reference for passDistance to cause loss
    [Header("Player Reference")]
    [SerializeField] private Transform player;
    [SerializeField] private float passDistanceAhead = 10f;

    [Header("Catch Settings")]
    [SerializeField] private float timeToCatch = 3f; // seconds the police must stay ahead

    [Header("UI References (assigned at runtime)")]
    [HideInInspector] public TextMeshProUGUI countdownText; // 3..2..1
    [HideInInspector] public GameObject caughtPopup;        // "caught" panel

    // Siren Sounds
    [Header("Siren Settings")]
    [SerializeField] private float sirenMinPitch = 0.9f;
    [SerializeField] private float sirenMaxPitch = 1.1f;
    private AudioSource siren;

    // Vehicle
    [Header("Vehicle Stats")]
    public float health = 100f;

    // Internal state
    private bool hasTriggeredFail = false;
    private float aheadTimer = 0f;

    // Track all active police cars so we can decide which one is "loudest"
    private static List<PoliceHandler> allPolice = new List<PoliceHandler>();
    private static Transform listenerTransform;

    // Only one police car at a time is allowed to drive the countdown UI
    private static PoliceHandler catchOwner = null;
    private static bool gameOver = false;

    // Reference back to the spawner so it can decrement its count
    private PoliceSpawner spawner;

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

    private void OnEnable()
    {
        if (!allPolice.Contains(this))
            allPolice.Add(this);
    }

    private void OnDisable()
    {
        allPolice.Remove(this);

        // If this car was the countdown owner, release it and hide countdown
        if (catchOwner == this)
        {
            catchOwner = null;
            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Siren setup
        siren = GetComponent<AudioSource>();
        if (siren != null)
        {
            siren.loop = true;
            siren.playOnAwake = false; // we will control when it plays
            siren.pitch = Random.Range(sirenMinPitch, sirenMaxPitch);
        }

        // Make sure UI starts hidden
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (caughtPopup != null)
            caughtPopup.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Always drive forward
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        // Catch logic (countdown & fail)
        UpdateCatchLogic();

        // Siren audio (only closest car is audible)
        HandleSirenAudio();
    }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void SetSpawner(PoliceSpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void UpdateCatchLogic()
    {
        if (player == null || hasTriggeredFail || gameOver)
            return;

        // Convert police position into the player's local space
        Vector3 policeInPlayerSpace = player.InverseTransformPoint(transform.position);
        bool isAhead = policeInPlayerSpace.z > passDistanceAhead;

        // If this car is not ahead
        if (!isAhead)
        {
            // If this car was the active catch owner, release and hide countdown
            if (catchOwner == this)
            {
                catchOwner = null;
                aheadTimer = 0f;

                if (countdownText != null)
                    countdownText.gameObject.SetActive(false);
            }

            // Non-owner cars do nothing to the countdown UI
            return;
        }

        // At this point, this car IS ahead.

        // If there is no current owner, become the owner and reset timer
        if (catchOwner == null)
        {
            catchOwner = this;
            aheadTimer = 0f;
        }

        // If this car is not the owner, ignore countdown logic
        if (catchOwner != this)
            return;

        // Owner car drives the countdown
        aheadTimer += Time.deltaTime;
        float timeLeft = Mathf.Clamp(timeToCatch - aheadTimer, 0f, timeToCatch);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
        }

        // If ahead long enough -> caught
        if (aheadTimer >= timeToCatch)
        {
            hasTriggeredFail = true;
            gameOver = true; // lock out further catch logic

            if (countdownText != null)
                countdownText.gameObject.SetActive(false);

            MissionFailed();
        }
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
            float dSq = toRef.sqrMagnitude; // cheaper than Vector3.Distance

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

    // Damage system for police cars
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

        // If this car was the owner, release and hide countdown
        if (catchOwner == this)
        {
            catchOwner = null;
            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }

        // Notify spawner so it can decrement its current count
        if (spawner != null)
            spawner.OnPoliceDestroyed();

        Debug.Log("Police car destroyed!");
        Destroy(gameObject);
    }

    private void MissionFailed()
    {
        Debug.Log("MISSION FAILED EVENT TRIGGERED");

        // Show caught UI popup
        if (caughtPopup != null)
            caughtPopup.gameObject.SetActive(true);

        //freeze the game
        //Time.timeScale = 0f;
    }

    public static void ResetGameState()
    {
        gameOver = false;
        catchOwner = null;
        allPolice.Clear();
    }
}