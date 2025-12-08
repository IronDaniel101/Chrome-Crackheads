using UnityEngine;

public class WallFailTrigger : MonoBehaviour
{
    [Header("Optional override")]
    [SerializeField] private GameObject caughtPopup;   // can be left empty on prefabs

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only react once, and only to the player
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;

        // Make sure we have a reference to the popup
        ResolvePopupReference();

        if (caughtPopup != null)
        {
            caughtPopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("WallFailTrigger: Could not find a loss popup in the scene.");
        }
    }

    /// <summary>
    /// Try to find the popup in the scene at runtime.
    /// Works even if the popup starts inactive.
    /// </summary>
    private void ResolvePopupReference()
    {
        if (caughtPopup != null)
            return;

        // Look for a LossPopupUI anywhere in the scene (including inactive objects)
        var popupUI = FindObjectOfType<LossPopupUI>(true); // 'true' includes inactive
        if (popupUI != null)
        {
            caughtPopup = popupUI.gameObject;
        }
        else
        {
            // Fallback: if you prefer using a tag, uncomment this and tag your popup "LossPopup"
            // var popupObj = GameObject.FindGameObjectWithTag("LossPopup");
            // if (popupObj != null)
            //     caughtPopup = popupObj;
        }
    }
}