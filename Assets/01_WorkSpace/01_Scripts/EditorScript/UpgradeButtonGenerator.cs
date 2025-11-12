using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UpgradeButtonGenerator : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public List<SONodeData> allNodeData;

    [Header("Prefabs & References")]
    public GameObject upgradeButtonPrefab; // Prefab with UpgradeButton component + TMP Text
    public Transform parentContainer; // Parent object to hold buttons
    public PlayerStats playerStats; // Reference to player stats

    public void GenerateButtons()
    {
        if (upgradeButtonPrefab == null || parentContainer == null || playerStats == null)
        {
            Debug.LogWarning("Assign prefab, parent container, and PlayerStats first!");
            return;
        }

        // Clear existing children first (optional)
        for (int i = parentContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentContainer.GetChild(i).gameObject);
        }

        // Create a button for each ScriptableObject
        foreach (var nodeData in allNodeData)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, parentContainer);

            // Use the ScriptableObject's asset name
            buttonObj.name = nodeData.name;

            UpgradeButton btn = buttonObj.GetComponent<UpgradeButton>();
            if (btn != null)
            {
                NodeInstance nodeInstance = new NodeInstance();
                nodeInstance.data = nodeData;
                btn.nodeInstance = nodeInstance;

                btn.playerStats = playerStats;

                TMP_Text tmp = buttonObj.GetComponentInChildren<TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = nodeData.name; // Show asset name
                }
            }
        }

        Debug.Log("Generated " + allNodeData.Count + " Upgrade Buttons!");
    }
}
