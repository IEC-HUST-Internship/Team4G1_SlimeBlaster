using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreachTerminateManager : MonoBehaviour
{
    private Transition transition;

    [Header("Buttons")]
    public Button breachButton;
    public Button terminateButton;

    [Header("GameOver Buttons")]
    public List<Button> toHomeButtons;
    public List<Button> restartButtons;

    [Header("Targets")]
    public List<GameObject> upgradeScenes;
    public List<GameObject> combatScenes;
    public GameObject gameOverPanel;

    private void Awake()
    {
        transition = GetComponent<Transition>();
        // Add listeners for buttons
        breachButton.onClick.AddListener(OnBreachClicked);
        terminateButton.onClick.AddListener(OnTerminateClicked);
        
        // Add listeners for all toHome buttons
        foreach (var button in toHomeButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnToHomeClicked);
        }
        
        // Add listeners for all restart buttons
        foreach (var button in restartButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnRestartClicked);
        }
    }

    private void Start()
    {
        // ✅ Show Breach/Upgrade first
        foreach (var scene in upgradeScenes)
            if (scene != null) scene.SetActive(true);
        
        foreach (var scene in combatScenes)
            if (scene != null) scene.SetActive(false);

        // ✅ Show Breach button, hide Terminate button
        if (breachButton != null) breachButton.gameObject.SetActive(true);
        if (terminateButton != null) terminateButton.gameObject.SetActive(false);
    }

    private void OnBreachClicked()
    {
        // 🎬 Play transition, then switch to Combat Scene
        transition.PlayTransition(() =>
        {
            // ✅ Switch to Combat Scene
            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(false);
            
            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(true);

            // ✅ Hide Breach button, show Terminate button
            if (breachButton != null) breachButton.gameObject.SetActive(false);
            if (terminateButton != null) terminateButton.gameObject.SetActive(true);
        });
    }

    private void OnTerminateClicked()
    {
        // 🎬 Play transition, then switch back to Upgrade Scene
        transition.PlayTransition(() =>
        {
            // ✅ Switch back to Upgrade Scene
            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(false);
            
            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(true);

            // ✅ Hide Terminate button, show Breach button
            if (terminateButton != null) terminateButton.gameObject.SetActive(false);
            if (breachButton != null) breachButton.gameObject.SetActive(true);
        });
    }

    private void OnToHomeClicked()
    {
        // 🎬 Play transition, then go to home
        transition.PlayTransition(() =>
        {
            // Hide GameOver Panel
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // Switch back to Upgrade Scene (same as Terminate)
            foreach (var scene in combatScenes)
                if (scene != null) scene.SetActive(false);
            
            foreach (var scene in upgradeScenes)
                if (scene != null) scene.SetActive(true);

            // Hide Terminate button, show Breach button
            if (terminateButton != null) terminateButton.gameObject.SetActive(false);
            if (breachButton != null) breachButton.gameObject.SetActive(true);
        });
    }

    private void OnRestartClicked()
    {
        // 🎬 Play transition, then restart
        transition.PlayTransition(() =>
        {
            // Hide GameOver Panel
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // Restart Combat Scene (disable then enable to trigger OnEnable)
            foreach (var scene in combatScenes)
            {
                if (scene != null)
                {
                    scene.SetActive(false);
                    scene.SetActive(true);
                }
            }
        });
    }
}
