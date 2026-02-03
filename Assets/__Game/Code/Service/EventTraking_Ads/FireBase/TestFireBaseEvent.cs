using UnityEngine;
using UnityEngine.UI;

public class TestFireBaseEvent : MonoBehaviour
{
    public Button FireBaseLevelCompleteButton;
    public Button FireBaseLevelResetButton;
    private void Start()
    {
        FireBaseLevelCompleteButton.onClick.AddListener(() => FireBaseAnalytics.Instance.LogLevelComplete(1, 1000));
        FireBaseLevelResetButton.onClick.AddListener(() => FireBaseAnalytics.Instance.LogLevelReset(1));
    }
}