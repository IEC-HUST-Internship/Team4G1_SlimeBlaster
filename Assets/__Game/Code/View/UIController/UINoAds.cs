using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UINoAds : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI noAdsStatusText;
    [SerializeField] private Button noAdsButton;
    [SerializeField] private Button noAdsPurchaseButton;

    private bool lastNoAdsState = false;

    private void OnEnable()
    {
        CheckNoAdsState();
    }

    private void Update()
    {
        bool current = ADSController.Instance != null && ADSController.Instance.IsNoAds;
        if (current != lastNoAdsState)
        {
            lastNoAdsState = current;
            CheckNoAdsState();
        }
    }

    public void UpdateStatusNoAdsTxt()
    {
        CheckNoAdsState();
    }

    private void CheckNoAdsState()
    {
        bool purchased = ADSController.Instance != null && ADSController.Instance.IsNoAds;

        // Update text
        if (noAdsStatusText != null)
            noAdsStatusText.text = purchased ? "Claimed" : "$9.99";

        // Disable no ads button if already purchased
        if (noAdsButton != null)
            noAdsButton.interactable = !purchased;

        // Disable purchase button if already purchased
        if (noAdsPurchaseButton != null)
            noAdsPurchaseButton.interactable = !purchased;
    }
}
