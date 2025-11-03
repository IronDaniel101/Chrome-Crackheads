using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 180f;
    [SerializeField] private Vector3 spinAxis = Vector3.forward;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);
    }
}
