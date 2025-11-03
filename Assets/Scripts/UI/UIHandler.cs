using UnityEngine;
using TMPro;

public class UIHandler : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI distanceTravelledText;
    [SerializeField] 
    private TMP_Text coinsText;

    //Reference
    [SerializeField] private CarHandler playerCarHandler;

    void Awake()
    {
        playerCarHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<CarHandler>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnEnable()
    {
        // Subscribe to coin updates (if CoinHandler exists)
        if (CoinHandler.Instance != null)
        {
            CoinHandler.OnCoinCountChanged += HandleCoinsChanged;
            // Initialize coins immediately
            HandleCoinsChanged(CoinHandler.Instance.TotalCoins);
        }
        else
        {
            // Safe default if handler not in scene yet
            if (coinsText) coinsText.text = "Coins: 0";
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid leaks
        CoinHandler.OnCoinCountChanged -= HandleCoinsChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (!distanceTravelledText || !playerCarHandler) return;
        distanceTravelledText.text = playerCarHandler.DistanceTravelled.ToString("000000");
    }

    private void HandleCoinsChanged(int total)
    {
        if (coinsText) coinsText.text = $"Coins: {total}";
    }
}
