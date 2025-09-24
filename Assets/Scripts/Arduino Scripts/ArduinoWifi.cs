using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class UdpIntReceiver : MonoBehaviour
{
    [Header("Network")]
    public int listenPort = 49152; // must match Arduino

    [Header("Player Settings")]
    public Rigidbody playerRigidbody;
    public float jumpForce = 5f;

    UdpClient udp;
    Thread recvThread;
    volatile bool running;
    ConcurrentQueue<string> lines = new ConcurrentQueue<string>();

    void Start()
    {
        udp = new UdpClient(listenPort);
        running = true;
        recvThread = new Thread(RecvLoop) { IsBackground = true };
        recvThread.Start();
        Debug.Log($"[UDP] Listening on port {listenPort}. If nothing arrives, check firewall and IP.");
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
                lines.Enqueue(line);
            }
            catch { /* socket closed */ }
        }
    }

    void Update()
    {
        while (lines.TryDequeue(out var line))
        {
            if (int.TryParse(line, out int val))
                ButtonPressed(val);
        }
    }

    private void ButtonPressed(int button)
    {
        switch (button)
        {
            case 9: Debug.Log("Centered"); break;
            case 8: Debug.Log("Jumped"); Jump(); break;
            default: Debug.Log($"Other: {button}"); break;
        }
    }

    private void Jump()
    {
        if (playerRigidbody != null)
        {
            // Optional: Only jump if nearly grounded
            if (Mathf.Abs(playerRigidbody.linearVelocity.y) < 0.01f)
            {
                playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    void OnDestroy()
    {
        running = false;
        try { recvThread?.Join(200); } catch { }
        try { udp?.Close(); } catch { }
    }
}
