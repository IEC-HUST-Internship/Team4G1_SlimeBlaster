using UnityEngine;

public class BannerAdsTrigger : MonoBehaviour
{
    void OnEnable()
    {
        ADSController.Instance?.ShowBanner();
    }

    void OnDisable()
    {
        ADSController.Instance?.HideBanner();
    }
}
