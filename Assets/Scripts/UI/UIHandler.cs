using UnityEngine;
using TMPro;

public class UIHandler : MonoBehaviour
{

    [SerializeField]
    TextMeshProUGUI distanceTravelledText;

    //Reference
    CarHandler playerCarHandler;

    void Awake()
    {
        playerCarHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<CarHandler>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelledText.text = playerCarHandler.DistanceTravelled.ToString("000000");
    }
}
