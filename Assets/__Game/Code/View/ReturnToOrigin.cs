using System.Collections.Generic;
using UnityEngine;

public class ReturnToOrigin : MonoBehaviour
{
    [SerializeField] private List<GameObject> targets = new List<GameObject>();

    private struct OriginalState
    {
        public RectTransform rectTransform;
        public Vector2 anchoredPosition;
        public Quaternion rotation;
        public Vector3 scale;
    }

    private List<OriginalState> savedStates = new List<OriginalState>();

    private void Awake()
    {
        foreach (var target in targets)
        {
            if (target == null) continue;
            var rt = target.GetComponent<RectTransform>();
            if (rt == null) continue;

            savedStates.Add(new OriginalState
            {
                rectTransform = rt,
                anchoredPosition = rt.anchoredPosition,
                rotation = rt.localRotation,
                scale = rt.localScale
            });
        }

        Invoke(nameof(ResetToOrigin), 0.1f);
    }

    private void LateUpdate()
    {
        ResetToOrigin();
    }

    private void ResetToOrigin()
    {
        foreach (var state in savedStates)
        {
            if (state.rectTransform == null) continue;
            state.rectTransform.anchoredPosition = state.anchoredPosition;
            state.rectTransform.localRotation = state.rotation;
            state.rectTransform.localScale = state.scale;
        }
    }
}
