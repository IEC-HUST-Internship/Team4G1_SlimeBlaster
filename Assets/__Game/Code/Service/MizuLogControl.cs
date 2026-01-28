using UnityEngine;

/// 📦 MizuLogControl
/// 🎛️ Inspector panel to toggle MizuLog groups ON/OFF
/// Just add this to any GameObject in scene!
/// 
public class MizuLogControl : MonoBehaviour
{
    [Header("🔧 Toggle Log Groups")]
    public bool combat = true;
    public bool ui = true;
    public bool audio = true;
    public bool spawn = true;
    public bool general = true;
    public bool save = true;
    public bool movement = true;
    public bool eventAndAds = true;

    [Header("⚡ Quick Actions")]
    [SerializeField] private bool enableAll = false;
    [SerializeField] private bool disableAll = false;

    private void OnValidate()
    {
        // ⚡ Quick enable all
        if (enableAll)
        {
            enableAll = false;
            combat = ui = audio = spawn = general = save = movement = eventAndAds = true;
        }

        // ⚡ Quick disable all
        if (disableAll)
        {
            disableAll = false;
            combat = ui = audio = spawn = general = save = movement = eventAndAds = false;
        }

        ApplySettings();
    }

    private void Awake()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        if (combat) MizuLog.Enable("Combat"); else MizuLog.Disable("Combat");
        if (ui) MizuLog.Enable("UI"); else MizuLog.Disable("UI");
        if (audio) MizuLog.Enable("Audio"); else MizuLog.Disable("Audio");
        if (spawn) MizuLog.Enable("Spawn"); else MizuLog.Disable("Spawn");
        if (general) MizuLog.Enable("General"); else MizuLog.Disable("General");
        if (save) MizuLog.Enable("Save"); else MizuLog.Disable("Save");
        if (movement) MizuLog.Enable("Movement"); else MizuLog.Disable("Movement");
        if (eventAndAds) MizuLog.Enable("EventAndAds"); else MizuLog.Disable("EventAndAds");
    }
}
