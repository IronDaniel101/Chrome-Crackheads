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

    [Header("Player Settings")]
    public Rigidbody playerRigidbody;
    public float jumpForce = 5f;

    [Header("Live Input (read-only)")]
    [Tooltip("Mapped from pot 0..1023 -> -1..+1")]
    public float steeringAxis; // -1..+1
    public bool button1Pressed; // true while held
    public bool button2Pressed;

    private UdpClient udp;
    private Thread recvThread;
    private volatile bool running;
    private readonly ConcurrentQueue<string> lines = new ConcurrentQueue<string>();

    // For edge detection
    private bool lastB1 = true; // Arduino sends 1=released, 0=pressed
    private bool lastB2 = true;

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
    }

    private void HandleCsv(int pot, int b1Int, int b2Int)
    {
        // Arduino sends: pot 0..1023, b1/b2 1=released, 0=pressed
        steeringAxis = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(0f, 1023f, pot));

        bool b1 = (b1Int != 0);
        bool b2 = (b2Int != 0);

        // Edge detect: released (true) -> pressed (false)
        if (lastB1 && !b1)
        {
            Jump();
            Debug.Log("[CTRL] Button1 pressed -> Jump");
        }
        if (lastB2 && !b2)
        {
            Debug.Log("[CTRL] Button2 pressed");
        }

        button1Pressed = !b1; // true while held
        button2Pressed = !b2;

        lastB1 = b1;
        lastB2 = b2;
    }

    private void Jump()
    {
        if (!playerRigidbody) return;

        // Optional simple "ground-ish" check using vertical speed
        if (Mathf.Abs(playerRigidbody.linearVelocity.y) < 0.05f)
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
