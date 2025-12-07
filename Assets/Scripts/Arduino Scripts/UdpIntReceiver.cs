using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class UdpIntReceiver : MonoBehaviour
{
    //Arduino Stuff very important
    [Header("Network")]
    [SerializeField] private int listenPort = 49999; // must match Arduino
    [Header("Rotation (Pot -> Yaw)")]
    public Transform rotationTarget;
    public float maxYawDegrees = 90f;
    public bool invertYaw = false;
    [Range(0f, 1f)]
    public float yawSmoothing = 0.2f;

    //Button 1 stuff
    [Header("Button 1")]
    public BoxCollider crosshairBox;
    public float damageAmount = 10f;

    //Button 2 stuff
    [Header("Button 2 Action")]
    public GameObject bombPrefab;
    public Transform bombPoint;
    public float bombDelay = 2f;
    public float bombRadius = 5f;
    public float bombDamage = 100f;
    private float bombCooldownTimer = 0f;
    public float bombCooldown = 5f; // seconds
    public GameObject explosionEffectPrefab;

    //Shows inputs
    [Header("Live Input (read-only)")]
    public float steeringAxis; // -1..+1
    public bool button1Pressed; // true while held
    public bool button2Pressed; // true while held

    private UdpClient udp;
    private Thread recvThread;
    private volatile bool running;
    private readonly ConcurrentQueue<string> lines = new ConcurrentQueue<string>();

    // Button edge detection (Arduino: 1=released, 0=pressed)
    private bool lastB1 = true;
    private bool lastB2 = true;

    // Rotation state
    private float currentYaw; // degrees
    private Transform rotTarget;

    void Awake()
    {
        rotTarget = rotationTarget != null ? rotationTarget : transform;
        // Initialize yaw to current object yaw
        currentYaw = rotTarget.eulerAngles.y;
    }

    void Start()
    {
        udp = new UdpClient(listenPort);
        running = true;
        recvThread = new Thread(RecvLoop) { IsBackground = true };
        recvThread.Start();
        Debug.Log($"[UDP] Listening on UDP {listenPort}. If nothing arrives, check firewall and IP.");
    }

    void RecvLoop()
    {
        IPEndPoint any = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                var data = udp.Receive(ref any);
                var line = Encoding.ASCII.GetString(data).Trim();
                if (!string.IsNullOrEmpty(line))
                    lines.Enqueue(line);
            }
            catch
            {
                // socket closed or interrupted
            }
        }
    }

    void Update()
    {
        // Drain all queued lines (latest packet wins)
        while (lines.TryDequeue(out var line))
        {
            var parts = line.Split(',');
            if (parts.Length >= 3
                && int.TryParse(parts[0], out int pot)
                && int.TryParse(parts[1], out int b1Int)
                && int.TryParse(parts[2], out int b2Int))
            {
                HandleCsv(pot, b1Int, b2Int);
            }
            else
            {
                Debug.LogWarning($"[UDP] Unrecognized packet: '{line}'");
            }
        }
        //Bomb cooldown timer
        if (bombCooldownTimer > 0f)
            bombCooldownTimer -= Time.deltaTime;

        // Apply smoothed absolute yaw each frame
        float targetYaw = steeringAxis * maxYawDegrees * (invertYaw ? -1f : 1f);

        // Exponential smoothing that's framerate-friendly
        float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(yawSmoothing), Time.deltaTime * 60f);
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, t);

        var e = rotTarget.eulerAngles;
        e.y = currentYaw;
        rotTarget.eulerAngles = e;
    }

    private void HandleCsv(int pot, int b1Int, int b2Int)
    {
        // pot: 0..1023 -> -2..+2
        steeringAxis = Mathf.Lerp(2f, -2f, Mathf.InverseLerp(0f, 1023f, pot));

        bool b1 = (b1Int != 0); // true = released
        bool b2 = (b2Int != 0);

        // Edge detect: released (true) -> pressed (false)
        if (lastB1 && !b1)
        {
            OnButton1Pressed();
        }
        if (lastB2 && !b2)
        {
            OnButton2Pressed();
        }

        button1Pressed = !b1; // true while held
        button2Pressed = !b2; // true while held

        lastB1 = b1;
        lastB2 = b2;
    }

    private void OnButton1Pressed()
    {
        Debug.Log("Button1 pressed -> Damage Dealt");

        if (crosshairBox != null)
        {
            // Calculate world center of the box
            Vector3 worldCenter = crosshairBox.transform.TransformPoint(crosshairBox.center);
            Vector3 halfExtents = Vector3.Scale(crosshairBox.size, crosshairBox.transform.lossyScale) * 0.5f;

            Collider[] hits = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                crosshairBox.transform.rotation
            );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemy = hit.GetComponent<MonoBehaviour>();
                    if (enemy != null)
                    {
                        var method = enemy.GetType().GetMethod("TakeDamage");
                        if (method != null)
                        {
                            method.Invoke(enemy, new object[] { damageAmount });
                        }
                    }
                }
            }
        }

    }

    //Stuff that happens when button 2 is pressed
    private void OnButton2Pressed()
    {
        if (bombCooldownTimer > 0f)
        {
            Debug.Log("Bomb is on cooldown!");
            return;
        }

        Debug.Log("Button2 pressed -> Dropping bomb");

        if (bombPrefab != null && bombPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, bombPoint.position, bombPoint.rotation);
            StartCoroutine(BombRoutine(bomb));
            bombCooldownTimer = bombCooldown; // Start cooldown
        }
    }

    //Bomb logic
    private System.Collections.IEnumerator BombRoutine(GameObject bomb)
    {
        yield return new WaitForSeconds(bombDelay);

        // Play explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, bomb.transform.position, Quaternion.identity);
        }

        // Damage police cars in radius
        Collider[] hits = Physics.OverlapSphere(bomb.transform.position, bombRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<MonoBehaviour>();
                if (enemy != null)
                {
                    var method = enemy.GetType().GetMethod("TakeDamage");
                    if (method != null)
                    {
                        method.Invoke(enemy, new object[] { bombDamage });
                    }
                }
            }
        }

        Destroy(bomb);
    }

    void OnDestroy()
    {
        running = false;
        try { udp?.Close(); } catch { }
        try { recvThread?.Join(200); } catch { }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
