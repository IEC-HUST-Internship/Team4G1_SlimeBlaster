using UnityEngine;
using System.Collections.Generic;

public class UpgradeButtonApearance : MonoBehaviour
{
    [Header("References")]
    public UpgradeButton upgradeButton;

    [Header("UI States")]
    public GameObject canAffordUI;      // Show when can afford
    public GameObject cannotAffordUI;   // Show when cannot afford
    public GameObject fullyUpgradedUI;  // Show when fully upgraded
    public GameObject defaultUI;        // Default state

    private void Update()
    {
        if (upgradeButton == null || upgradeButton.playerStats == null) return;

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        // Check if fully upgraded
        bool isMaxLevel = upgradeButton.nodeInstance.currentLevel >= upgradeButton.nodeInstance.data.maxLevel;
        
        if (isMaxLevel)
        {
            ShowOnly(fullyUpgradedUI);
            return;
        }

        // Get cost and currency type
        int cost = upgradeButton.nodeInstance.GetCostForNextLevel();
        EnumCurrency currencyType = upgradeButton.nodeInstance.data.costUnit;

        // Check if player has enough currency (get playerStats from upgradeButton)
        bool canAfford = upgradeButton.playerStats.HasEnoughCurrency(currencyType, cost);

        if (canAfford)
        {
            ShowOnly(canAffordUI);
        }
        else
        {
            ShowOnly(cannotAffordUI);
        }
    }

    private void ShowOnly(GameObject uiToShow)
    {
        // Hide all UI first
        if (canAffordUI != null) canAffordUI.SetActive(false);
        if (cannotAffordUI != null) cannotAffordUI.SetActive(false);
        if (fullyUpgradedUI != null) fullyUpgradedUI.SetActive(false);
        if (defaultUI != null) defaultUI.SetActive(false);

        // Show only the specified UI
        if (uiToShow != null) uiToShow.SetActive(true);
    }
}
