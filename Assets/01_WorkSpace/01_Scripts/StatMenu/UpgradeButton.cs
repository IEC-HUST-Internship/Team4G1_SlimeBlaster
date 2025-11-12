using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("Node Data")]
    public NodeInstance nodeInstance;

    [Header("Unlock Root")]
    public GameObject unlockRootObject; // Drag the GameObject in inspector
    private UpgradeButton unlockRootButton;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        // Get UpgradeButton from the GameObject at runtime
        if (unlockRootObject != null)
        {
            unlockRootButton = unlockRootObject.GetComponent<UpgradeButton>();
            if (unlockRootButton != null)
            {
                nodeInstance.unlockRoot = unlockRootButton.nodeInstance;
            }
        }

        UpdateUnlockStatus();
    }

    private void OnClick()
    {
        if (!nodeInstance.CanUpgrade()) return;

        int cost = nodeInstance.GetCostForNextLevel();

        if (playerStats.HasEnoughCurrency(nodeInstance.data.costUnit, cost))
        {
            playerStats.SpendCurrency(nodeInstance.data.costUnit, cost);
            nodeInstance.Upgrade();
            ApplyUpgradeEffect();
            Debug.Log($"{nodeInstance.data.upgradeName} upgraded to level {nodeInstance.currentLevel}");

            if (unlockRootButton != null)
                unlockRootButton.UpdateDependentUnlocks();
        }
        else
        {
            Debug.Log("Not enough resources!");
        }
    }

    private void ApplyUpgradeEffect()
    {
        // Works for any EnumStat
        playerStats.AddStat(nodeInstance.data.stat, nodeInstance.data.perUpgradeValue);
    }

    public void UpdateUnlockStatus()
    {
        if (nodeInstance.unlockRoot == null)
        {
            nodeInstance.unlocked = true;
        }
        else
        {
            nodeInstance.unlocked = nodeInstance.unlockRoot.currentLevel >= nodeInstance.unlockRequirementLevel;
        }
    }

    public void UpdateDependentUnlocks()
    {
        UpgradeButton[] allButtons = FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.nodeInstance.unlockRoot == nodeInstance)
            {
                btn.UpdateUnlockStatus();
            }
        }
    }
}
