using UnityEngine;
using TMPro;

public class AllCurrencyUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("TextMeshPro UI")]
    public TextMeshProUGUI blueBitsText;
    public TextMeshProUGUI pinkBitsText;
    public TextMeshProUGUI yellowBitsText;
    public TextMeshProUGUI greenBitsText;
    public TextMeshProUGUI xpBitsText;


    void Update()
    {
        if (playerStats == null) return;

        // Update all three currency text fields
        if (blueBitsText != null)
            blueBitsText.text = playerStats.blueBits.ToString();

        if (pinkBitsText != null)
            pinkBitsText.text = playerStats.pinkBits.ToString();

        if (yellowBitsText != null)
            yellowBitsText.text = playerStats.yellowBits.ToString();

        if (greenBitsText != null)
            greenBitsText.text = playerStats.greenBits.ToString();

        if (xpBitsText != null)
            xpBitsText.text = playerStats.xpBits.ToString();
    }
}
