using UnityEngine;
using UnityEngine.UI;

public class TestFireBaseEvent : MonoBehaviour
{
    public Button NoAdsPurchase;
    private void Start()
    {
        NoAdsPurchase.onClick.AddListener(() => FireBaseAnalytics.Instance.BuySuccess(Stage.Instance.GetStage(), "NoAdsPurchased"));
    }
}