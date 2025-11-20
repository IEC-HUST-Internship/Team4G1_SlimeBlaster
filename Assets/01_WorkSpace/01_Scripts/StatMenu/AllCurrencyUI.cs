using UnityEngine;
using TMPro;

public class AllCurrencyUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("TextMeshPro UI")]
    public TextMeshProUGUI redBitsText;
    public TextMeshProUGUI blueBitsText;
    public TextMeshProUGUI purpleBitsText;


    void Update()
    {
        if (playerStats == null) return;

        // Update all three currency text fields
        if (redBitsText != null)
            redBitsText.text = "Red Bits: " + playerStats.redBits.ToString();
        
        if (blueBitsText != null)
            blueBitsText.text = "Blue Bits: " + playerStats.blueBits.ToString();
        
        if (purpleBitsText != null)
            purpleBitsText.text = "Purple Bits: " + playerStats.purpleBits.ToString();
    }
}
