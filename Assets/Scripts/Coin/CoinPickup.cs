using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 180f;
    [SerializeField] private Vector3 spinAxis = Vector3.forward;
    [SerializeField] private int coinValue = 1;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Insure that the player is who is hitting the coin
        if (!other.CompareTag("Player")) return;

        //Add the value of coins to the coin count
        CoinHandler.Instance.AddCoins(coinValue);

        //Disable the coin instead of Destroying it (for pooling)
        gameObject.SetActive(false);
        

    }

}
