using UnityEngine;
using UnityEngine.UI;

public class BreachBtn : MonoBehaviour
{
    [Header("Target to toggle")]
    public GameObject targetObject;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(ToggleTarget);
    }

    private void ToggleTarget()
    {
        if (targetObject == null) return;

        // Toggle the active state
        targetObject.SetActive(!targetObject.activeSelf);
    }
}
