using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 📚 Tutorial Controller - Shows tutorial steps in sequence, then spawns slime
/// Assign to a Tutorial GameObject, controlled by BreachTerminateManager
/// </summary>
public class TutorialController : MonoBehaviour
{
    [Header("Tutorial Setup")]
    [SerializeField] private List<GameObject> tutorialSteps = new List<GameObject>();
    
    [Header("� Tutorial For Money (after slime dies)")]
    [SerializeField] private List<GameObject> tutorialForMoney = new List<GameObject>();
    
    [Header("🐌 Slime Settings")]
    [SerializeField] private GameObject tutorialSlime; // The slime child object
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float slimeMoveSpeed = 2f;
    [SerializeField] private int slimeMaxHp = 100;
    
    [Header("💰 Rewards")]
    [SerializeField] private int currencyDropAmount = 10;
    [SerializeField] private EnumCurrency currencyType = EnumCurrency.yellowBits;
    
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerCombatUI playerCombatUI;
    
    private int currentStepIndex = -1;
    private int currentMoneyStepIndex = -1;
    private bool isTutorialActive = false;
    private bool isMoneyTutorialActive = false;
    private bool slimeCompleted = false;

    private void OnEnable()
    {
        // Start logic after 2 second delay
        StartCoroutine(InitializeAfterDelay());
    }
    
    /// <summary>
    /// ⏱️ Wait 2 seconds before initializing tutorial logic
    /// </summary>
    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(2f); // Use RealTime because TimeScale might be 0
        
        // Reset state when enabled
        slimeCompleted = false;
        isTutorialActive = false;
        currentStepIndex = -1;
        
        // Hide all tutorial steps at start
        HideAllSteps();
        HideAllMoneySteps();
        
        // Position and hide slime at start, set HP
        if (tutorialSlime != null && startPoint != null)
        {
            tutorialSlime.transform.position = startPoint.position;
            tutorialSlime.SetActive(false);
            
            // Set slime HP to 100
            var enemy = tutorialSlime.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetHealth(slimeMaxHp, slimeMaxHp);
            }
        }
        
        // Auto start tutorial after delay
        StartTutorial();
    }

    private void Update()
    {
        // If tutorial is active, check for click to go next
        if (isTutorialActive && Input.GetMouseButtonDown(0))
        {
            ShowNextStep();
        }
        
        // If money tutorial is active, check for click to go next
        if (isMoneyTutorialActive && Input.GetMouseButtonDown(0))
        {
            ShowNextMoneyStep();
        }
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Count == 0)
        {
            Debug.LogWarning("No tutorial steps assigned!");
            OnTutorialComplete();
            return;
        }

        isTutorialActive = true;
        currentStepIndex = -1;
        
        // Stop time when tutorial starts
        Time.timeScale = 0f;
        
        // Show first step
        ShowNextStep();
    }

    public void ShowNextStep()
    {
        // Hide current step
        if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Count)
        {
            tutorialSteps[currentStepIndex].SetActive(false);
        }

        // Move to next
        currentStepIndex++;

        // Check if tutorial ended
        if (currentStepIndex >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        // Show next step (stop time for each step)
        if (tutorialSteps[currentStepIndex] != null)
        {
            Time.timeScale = 0f;
            tutorialSteps[currentStepIndex].SetActive(true);
        }
    }

    private void EndTutorial()
    {
        isTutorialActive = false;
        currentStepIndex = -1;
        
        HideAllSteps();
        
        // Resume time
        Time.timeScale = 1f;
        
        Debug.Log("Tutorial completed! Starting slime...");
        
        // Start slime movement after tutorial text ends
        OnTutorialComplete();
    }
    
    /// <summary>
    /// 📚 Called when all tutorial steps are done - start slime movement
    /// </summary>
    private void OnTutorialComplete()
    {
        // Already in combat scene, just spawn and move slime
        if (tutorialSlime != null && startPoint != null && endPoint != null)
        {
            tutorialSlime.transform.position = startPoint.position;
            tutorialSlime.SetActive(true);
            StartCoroutine(MoveSlimeToEnd());
        }
        else
        {
            Debug.LogWarning("Slime or points not assigned! Showing win directly.");
            OnSlimeDie();
        }
    }
    
    /// <summary>
    /// 🐌 Move slime from start to end point
    /// </summary>
    private IEnumerator MoveSlimeToEnd()
    {
        while (tutorialSlime != null && Vector3.Distance(tutorialSlime.transform.position, endPoint.position) > 0.1f)
        {
            tutorialSlime.transform.position = Vector3.MoveTowards(
                tutorialSlime.transform.position,
                endPoint.position,
                slimeMoveSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        // Slime reached end - "die" and drop currency
        OnSlimeDie();
    }
    
    /// <summary>
    /// 💀 Called when slime reaches end point or is killed
    /// </summary>
    public void OnSlimeDie()
    {
        if (slimeCompleted) return;
        slimeCompleted = true;
        
        Debug.Log($"Tutorial slime died! Dropping {currencyDropAmount} {currencyType}");
        
        // Drop currency
        if (playerStats != null)
        {
            playerStats.AddCurrency(currencyType, currencyDropAmount);
            Debug.Log($"Added {currencyDropAmount} {currencyType} to player!");
        }
        else
        {
            // Try to find PlayerStats
            var stats = FindObjectOfType<PlayerStats>();
            if (stats != null)
            {
                stats.AddCurrency(currencyType, currencyDropAmount);
            }
        }
        
        // Hide slime
        if (tutorialSlime != null)
        {
            tutorialSlime.SetActive(false);
        }
        
        // Start money tutorial sequence
        StartMoneyTutorial();
    }
    
    /// <summary>
    /// 💰 Start showing money tutorial steps
    /// </summary>
    private void StartMoneyTutorial()
    {
        if (tutorialForMoney.Count == 0)
        {
            Debug.Log("No money tutorial steps, showing win after 2 seconds...");
            StartCoroutine(ShowWinAfterDelay());
            return;
        }
        
        isMoneyTutorialActive = true;
        currentMoneyStepIndex = -1;
        
        // Stop time during money tutorial
        Time.timeScale = 0f;
        
        // Show first money step
        ShowNextMoneyStep();
    }
    
    /// <summary>
    /// 💰 Show next money tutorial step
    /// </summary>
    public void ShowNextMoneyStep()
    {
        // Hide current step
        if (currentMoneyStepIndex >= 0 && currentMoneyStepIndex < tutorialForMoney.Count)
        {
            tutorialForMoney[currentMoneyStepIndex].SetActive(false);
        }
        
        currentMoneyStepIndex++;
        
        // Check if money tutorial ended
        if (currentMoneyStepIndex >= tutorialForMoney.Count)
        {
            EndMoneyTutorial();
            return;
        }
        
        // Show next step
        if (tutorialForMoney[currentMoneyStepIndex] != null)
        {
            Time.timeScale = 0f;
            tutorialForMoney[currentMoneyStepIndex].SetActive(true);
        }
    }
    
    /// <summary>
    /// 💰 End money tutorial and show win after 2 seconds
    /// </summary>
    private void EndMoneyTutorial()
    {
        isMoneyTutorialActive = false;
        currentMoneyStepIndex = -1;
        
        HideAllMoneySteps();
        
        // Resume time
        Time.timeScale = 1f;
        
        Debug.Log("Money tutorial completed! Showing win in 2 seconds...");
        
        // Show win after 2 seconds
        StartCoroutine(ShowWinAfterDelay());
    }
    
    /// <summary>
    /// ⏱️ Show win panel after 2 second delay
    /// </summary>
    private IEnumerator ShowWinAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        ShowWinPanel();
    }
    
    /// <summary>
    /// 🏆 Show the win panel
    /// </summary>
    private void ShowWinPanel()
    {
        if (playerCombatUI != null)
        {
            playerCombatUI.ShowWin();
        }
        else
        {
            // Try to find PlayerCombatUI
            PlayerCombatUI combatUI = FindObjectOfType<PlayerCombatUI>();
            if (combatUI != null)
            {
                combatUI.ShowWin();
            }
            else
            {
                Debug.LogWarning("PlayerCombatUI not found! Cannot show win panel.");
            }
        }
        
        // Hide this tutorial object
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 🔄 Can be called externally if slime is killed by player attack
    /// </summary>
    public void KillTutorialSlime()
    {
        StopAllCoroutines();
        OnSlimeDie();
    }

    private void HideAllSteps()
    {
        foreach (var step in tutorialSteps)
        {
            if (step != null)
            {
                step.SetActive(false);
            }
        }
    }
    
    private void HideAllMoneySteps()
    {
        foreach (var step in tutorialForMoney)
        {
            if (step != null)
            {
                step.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ⏭️ Call this to skip/close tutorial anytime
    /// </summary>
    public void SkipTutorial()
    {
        EndTutorial();
    }
}
