using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class UdpIntReceiver : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private int listenPort = 49999; // must match Arduino

    [Header("Rotation (Pot -> Yaw)")]
    [Tooltip("Which transform to rotate. If null, rotates this GameObject.")]
    public Transform rotationTarget;
    [Tooltip("Maps pot extremes to ±this many degrees around Y.")]
    public float maxYawDegrees = 90f;
    [Tooltip("Invert pot direction if needed.")]
    public bool invertYaw = false;
    [Range(0f, 1f)]
    [Tooltip("0 = instant, 1 = very smooth.")]
    public float yawSmoothing = 0.2f;

    [Header("Spawn on Button 1")]
    public GameObject spawnPrefab;
    [Tooltip("Optional parent for spawned objects (otherwise scene root).")]
    public Transform spawnParent;
    [Tooltip("Spawn offset from rotationTarget (or this transform if null).")]
    public Vector3 spawnOffset = new Vector3(0, 0, 1);

    [Header("Color toggle on Button 2")]
    public Renderer colorRenderer; // if null, will try GetComponentInChildren<Renderer>()
    public Color idleColor = Color.white;
    public Color activeColor = Color.green;

    [Header("Live Input (read-only)")]
    [Tooltip("Mapped from pot 0..1023 -> -1..+1")]
    public float steeringAxis; // -1..+1
    public bool button1Pressed; // true while held
    public bool button2Pressed;

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
        if (!colorRenderer)
        {
            colorRenderer = GetComponentInChildren<Renderer>();
        }
        if (colorRenderer)
        {
            SetRendererColor(idleColor);
        }
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
        // pot: 0..1023 -> -1..+1
        steeringAxis = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(0f, 1023f, pot));

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
        button2Pressed = !b2;

        lastB1 = b1;
        lastB2 = b2;
    }

    private void OnButton1Pressed()
    {
        if (!spawnPrefab) return;

        // Spawn near the rotationTarget (or this transform)
        Transform anchor = rotTarget ? rotTarget : transform;
        Vector3 pos = anchor.position + anchor.TransformVector(spawnOffset);
        Quaternion rot = anchor.rotation;

        Instantiate(spawnPrefab, pos, rot, spawnParent);
        Debug.Log("[CTRL] Button1 pressed -> Spawned prefab");
    }

    private bool colorActive = false;
    private void OnButton2Pressed()
    {
        colorActive = !colorActive;
        SetRendererColor(colorActive ? activeColor : idleColor);
        Debug.Log("[CTRL] Button2 pressed -> Toggled color");
    }

    private void SetRendererColor(Color c)
    {
        if (!colorRenderer) return;

        // Handle materials safely (assuming a single material target)
        if (colorRenderer.material.HasProperty("_Color"))
        {
            colorRenderer.material.color = c;
        }
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
