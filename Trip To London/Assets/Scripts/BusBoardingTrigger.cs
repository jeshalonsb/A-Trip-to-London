using StarterAssets;
using UnityEngine;

public class BusBoardingTrigger : MonoBehaviour
{
    [SerializeField] private DoubleDeckerBusController busController;

    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController playerController =
            other.GetComponentInParent<ThirdPersonController>();

        if (playerController == null)
            return;

        if (busController == null)
        {
            Debug.LogWarning("Bus Controller has not been assigned.");
            return;
        }

        // Always pass the main player root, not the collider child.
        busController.TryBoardBus(playerController.gameObject);
    }
}